namespace ExamLeadPortal.ViewModels
{
    public class LeadListViewModel
    {
        public List<LeadListItemViewModel> Leads { get; set; } = new();
        public LeadFilterOptions Filters { get; set; } = new();
    }
}