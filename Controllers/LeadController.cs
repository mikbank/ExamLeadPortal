using ExamLeadPortal.Services;
using ExamLeadPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ExamLeadPortal.Controllers
{
    public class LeadController : Controller
    {
        private readonly ILeadQueryService _leadQueryService;

        public LeadController(ILeadQueryService leadQueryService)
        {
            _leadQueryService = leadQueryService;
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

        public IActionResult Details(string id)
        {
            var lead = _leadQueryService.GetLeadById(id);

            if (lead == null)
            {
                return NotFound();
            }

            var viewModel = new LeadDetailsViewModel
            {
                Lead = lead,
                HasCorrections = false
            };

            return View(viewModel);
        }
    }
}