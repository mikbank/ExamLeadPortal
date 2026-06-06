using ExamLeadPortal.Models;
using ExamLeadPortal.Repositories;
using ExamLeadPortal.Services;
using ExamLeadPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ExamLeadPortal.Controllers
{
    public class LeadController : Controller
    {
        private readonly ILeadQueryService _leadQueryService;
        private readonly IAcceptedLeadRepository _acceptedLeadRepository;
        private readonly ILeadAcceptanceService _leadAcceptanceService;
        private readonly ILeadRevisionService _leadRevisionService;

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
            ILeadRevisionService leadRevisionService)
        {
            _leadQueryService = leadQueryService;
            _acceptedLeadRepository = acceptedLeadRepository;
            _leadAcceptanceService = leadAcceptanceService;
            _leadRevisionService = leadRevisionService;
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

        public IActionResult Details( //Get details for selected lead
            string id,
            string? message = null)
        {
            var currentLead = _leadQueryService.GetLeadById(id);

            if (currentLead == null)
            {
                return NotFound();
            }

            var latestRevision =
                _leadRevisionService.GetLatestRevision(id); //always fetches latest revision of a lead upon inspecting details, mayhaps a bit muddy on the contract? could be query instead?

            var viewModel = new LeadDetailsViewModel
            {
                Lead = currentLead,

            
                HasCorrections = latestRevision != null, //if latest revision is not null, then the lead has been revised at some point

                ValidBUs = ValidBUs,

                IsAlreadyAccepted =
                    _acceptedLeadRepository.ExistsForRawLead(id),

                Message = message
            };

            return View(viewModel);
        }

        [HttpPost]

        public IActionResult Accept(
            string rawLeadId,
            string responsibleBU)
        {
            var success =
                _leadAcceptanceService.TryAcceptLead(
                    rawLeadId,
                    responsibleBU,
                    "ExamUser",//!!! REPLACE THIS ON WORKING OUT AUTHENTICATION
                    out var errorMessage);

            if (!success)
            {
                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = rawLeadId,
                        message = errorMessage
                    });
            }

            return RedirectToAction(
                "Details",
                new
                {
                    id = rawLeadId,
                    message = "Lead accepted successfully."
                });
        }

        [HttpGet]//fetches edit view
        public IActionResult Edit(string id)
        {
            if (_acceptedLeadRepository.ExistsForRawLead(id))
            {
                return RedirectToAction(
                    "Details",
                    new
                    {
                        id,
                        message =
                            "This lead has already been accepted " +
                            "and can no longer be edited."
                    });
            }

            var currentLead =
                _leadQueryService.GetLeadById(id);

            if (currentLead == null)
            {
                return NotFound();
            }

            var latestRevision =
                _leadRevisionService.GetLatestRevision(id);

            var viewModel = new LeadRevisionEditViewModel
            {
                RawLeadId = currentLead.Id,
                LeadTitle = currentLead.LeadTitle,
                LeadSummary = currentLead.LeadSummary,
                LeadValue = currentLead.LeadValue,
                ResourceLink = currentLead.ResourceLink,

                AffectedBUs =
                    new List<string>(
                        currentLead.AffectedBUs),

                ValidBUs = ValidBUs,

                PayloadValues =
                    currentLead.LeadPayload.ToDictionary(
                        x => x.Key,
                        x => JsonElementToDisplayString(x.Value)),

                CurrentRevisionVersion =
                    latestRevision?.VersionNumber
            };

            return View(viewModel);
        }

        [HttpPost]//saves the edits
        public IActionResult SaveEdits(
            LeadRevisionEditViewModel model)
        {
            if (_acceptedLeadRepository.ExistsForRawLead(
                    model.RawLeadId))
            {
                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = model.RawLeadId,
                        message =
                            "This lead has already been accepted " +
                            "and can no longer be edited."
                    });
            }

            if (!ModelState.IsValid)
            {
                PopulateEditMetadata(model);
                return View("Edit", model);
            }

            var currentLead =
                _leadQueryService.GetLeadById(
                    model.RawLeadId);

            if (currentLead == null)
            {
                return NotFound();
            }

            var revisedLead = new RawLead
            {
                Id = model.RawLeadId,
                LeadTitle = model.LeadTitle,
                LeadSummary = model.LeadSummary,
                LeadValue = model.LeadValue,
                ResourceLink = model.ResourceLink,

                AffectedBUs =
                    new List<string>(
                        model.AffectedBUs
                        ?? new List<string>()),

                LeadPayload =
                    ConvertPayloadValues(
                        model.PayloadValues)
            };

            var revision =
                _leadRevisionService.CreateRevision(
                    model.RawLeadId,
                    revisedLead,
                    "ExamUser");

            return RedirectToAction(
                "Details",
                new
                {
                    id = model.RawLeadId,
                    message =
                        $"Revision {revision.VersionNumber} " +
                        "saved successfully."
                });
        }

        private void PopulateEditMetadata(
            LeadRevisionEditViewModel model)
        {
            model.ValidBUs = ValidBUs;

            model.CurrentRevisionVersion =
                _leadRevisionService
                    .GetLatestRevision(model.RawLeadId)
                    ?.VersionNumber;
        }

        private static Dictionary<string, JsonElement>
            ConvertPayloadValues(
                Dictionary<string, string>? payloadValues)
        {
            var convertedPayload =
                new Dictionary<string, JsonElement>();

            if (payloadValues == null)
            {
                return convertedPayload;
            }

            foreach (var item in payloadValues)
            {
                convertedPayload[item.Key] =
                    JsonSerializer.SerializeToElement(
                        item.Value ?? string.Empty);
            }

            return convertedPayload;
        }

        private static string JsonElementToDisplayString(
            JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String =>
                    element.GetString()
                    ?? string.Empty,

                JsonValueKind.Null =>
                    string.Empty,

                _ => element.ToString()
            };
        }
    }
}