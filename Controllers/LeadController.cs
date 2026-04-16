using ExamLeadPortal.Repositories;
using ExamLeadPortal.Services;
using ExamLeadPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ExamLeadPortal.Controllers
{
    public class LeadController : Controller
    {
        private readonly ILeadQueryService _leadQueryService;
        private readonly IAcceptedLeadRepository _acceptedLeadRepository;
        private readonly ILeadAcceptanceService _leadAcceptanceService;

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
            ILeadAcceptanceService leadAcceptanceService)
        {
            _leadQueryService = leadQueryService;
            _acceptedLeadRepository = acceptedLeadRepository;
            _leadAcceptanceService = leadAcceptanceService;
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
            var lead = _leadQueryService.GetLeadById(id);

            if (lead == null)
            {
                return NotFound();
            }

            var viewModel = new LeadDetailsViewModel
            {
                Lead = lead,
                HasCorrections = false,
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
    }
}