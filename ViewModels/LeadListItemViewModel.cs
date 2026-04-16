using ExamLeadPortal.Models;

namespace ExamLeadPortal.ViewModels
{
    public class LeadListItemViewModel
    {
        public RawLead Lead { get; set; } = null!;
        public bool IsIncomplete { get; set; }
    }
}