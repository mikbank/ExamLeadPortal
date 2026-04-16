using ExamLeadPortal.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ExamLeadPortal.Controllers
{
    public class AcceptedLeadController : Controller
    {
        private readonly IAcceptedLeadRepository _acceptedLeadRepository;

        public AcceptedLeadController(IAcceptedLeadRepository acceptedLeadRepository)
        {
            _acceptedLeadRepository = acceptedLeadRepository;
        }

        public IActionResult Index()
        {
            var acceptedLeads = _acceptedLeadRepository.GetAll();
            return View(acceptedLeads);
        }
    }
}