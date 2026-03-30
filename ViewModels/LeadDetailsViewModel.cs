using ExamLeadPortal.Models;

namespace ExamLeadPortal.ViewModels
{
    public class LeadDetailsViewModel
    {
        public Lead? Lead { get; set; }
        public bool HasCorrections { get; set; }
    }
}