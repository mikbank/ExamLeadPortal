namespace ExamLeadPortal.ViewModels
{//viewmodel for the list of leads
    public class LeadListViewModel
    {
        public List<LeadListItemViewModel> Leads { get; set; } = new();
        public LeadFilterOptions Filters { get; set; } = new();
    }
}