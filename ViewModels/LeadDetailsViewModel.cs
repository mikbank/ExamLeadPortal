using ExamLeadPortal.Models;

namespace ExamLeadPortal.ViewModels
{
    public class LeadDetailsViewModel
    {
        public RawLead? Lead { get; set; }
        public bool HasCorrections { get; set; }
    }
}