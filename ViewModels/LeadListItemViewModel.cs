using ExamLeadPortal.Models;

namespace ExamLeadPortal.ViewModels
{//view model for list items, so each item in a list should follow this
    public class LeadListItemViewModel
    {
        public RawLead Lead { get; set; } = null!;
        public bool IsIncomplete { get; set; }
    }
}