using ExamLeadPortal.Repositories;
using ExamLeadPortal.Services;
using ExamLeadPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ExamLeadPortal.Models;

namespace ExamLeadPortal.Controllers
{
    public class LeadController : Controller
    {
        private readonly ILeadQueryService _leadQueryService;
        private readonly IAcceptedLeadRepository _acceptedLeadRepository;
        private readonly ILeadAcceptanceService _leadAcceptanceService;
        private readonly ILeadCorrectionService _leadCorrectionService;

        private static readonly List<string> ValidBUs =
        [
            "BUILD",
            "ENERGY",
            "ENV",
            "GLIT",
            "INFRA",
            "IT",
            "VAFO"
        ];

        public LeadController(
            ILeadQueryService leadQueryService,
            IAcceptedLeadRepository acceptedLeadRepository,
            ILeadAcceptanceService leadAcceptanceService,
            ILeadCorrectionService leadCorrectionService)
        {
            _leadQueryService = leadQueryService;
            _acceptedLeadRepository = acceptedLeadRepository;
            _leadAcceptanceService = leadAcceptanceService;
            _leadCorrectionService = leadCorrectionService;
        }

        public IActionResult Index(LeadFilterOptions filters)
        {
            var viewModel = new LeadListViewModel
            {
                Filters = filters,
                Leads = _leadQueryService.GetFilteredLeads(filters)
            };

            return View(viewModel);
        }

        public IActionResult Details(string id, string? message = null)
        {
            var rawLead = _leadQueryService.GetLeadById(id);

            if (rawLead == null)
            {
                return NotFound();
            }

            var correction = _leadCorrectionService.GetCorrection(id);
            var correctedLead = _leadCorrectionService.ApplyCorrection(rawLead, correction);

            var viewModel = new LeadDetailsViewModel
            {
                Lead = correctedLead,
                HasCorrections = correction != null,
                ValidBUs = ValidBUs,
                IsAlreadyAccepted = _acceptedLeadRepository.ExistsForRawLead(id),
                Message = message
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Accept(string rawLeadId, string responsibleBU)
        {
            var success = _leadAcceptanceService.TryAcceptLead(
                rawLeadId,
                responsibleBU,
                "ExamUser",
                out var errorMessage);

            if (!success)
            {
                return RedirectToAction("Details", new { id = rawLeadId, message = errorMessage });
            }

            return RedirectToAction("Details", new { id = rawLeadId, message = "Lead accepted successfully." });
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var rawLead = _leadQueryService.GetLeadById(id);

            if (rawLead == null)
            {
                return NotFound();
            }

            if (_acceptedLeadRepository.ExistsForRawLead(id))
            {
                return RedirectToAction("Details", new
                {
                    id,
                    message = "This lead has already been accepted and can no longer be edited."
                });
            }

            var correction = _leadCorrectionService.GetCorrection(id);
            var correctedLead = _leadCorrectionService.ApplyCorrection(rawLead, correction);

            var viewModel = new LeadCorrectionEditViewModel
            {
                RawLeadId = correctedLead.Id,
                LeadTitle = correctedLead.LeadTitle,
                LeadSummary = correctedLead.LeadSummary,
                LeadValue = correctedLead.LeadValue,
                ResourceLink = correctedLead.ResourceLink,
                AffectedBUs = new List<string>(correctedLead.AffectedBUs),
                ValidBUs = ValidBUs,
                PayloadValues = correctedLead.LeadPayload.ToDictionary(
                    x => x.Key,
                    x => x.Value.ToString()
                )
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SaveEdits(LeadCorrectionEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ValidBUs = ValidBUs;
                return View("Edit", model);
            }

            var rawLead = _leadQueryService.GetLeadById(model.RawLeadId);

            if (rawLead == null)
            {
                return NotFound();
            }

             if (_acceptedLeadRepository.ExistsForRawLead(model.RawLeadId))
                {
                    return RedirectToAction("Details", new
                    {
                        id = model.RawLeadId,
                        message = "This lead has already been accepted and can no longer be edited."
                    });}

            var correctedPayload = new Dictionary<string, JsonElement>();

            foreach (var item in model.PayloadValues)
            {
                correctedPayload[item.Key] = JsonSerializer.SerializeToElement(item.Value ?? string.Empty);
            }

            var correction = new LeadCorrection
            {
                RawLeadId = model.RawLeadId,
                LeadTitle = model.LeadTitle,
                LeadSummary = model.LeadSummary,
                LeadValue = model.LeadValue,
                ResourceLink = model.ResourceLink,
                AffectedBUs = model.AffectedBUs,
                CorrectedPayload = correctedPayload,
                CorrectedAt = DateTime.Now
            };

            _leadCorrectionService.SaveCorrection(correction);

            return RedirectToAction("Details", new
            {
                id = model.RawLeadId,
                message = "Lead edits saved successfully."
            });
        }
    }
    
}