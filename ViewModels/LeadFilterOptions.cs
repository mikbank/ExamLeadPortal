namespace ExamLeadPortal.ViewModels
{//these are the filter values avalable for the view, implemented in query service, and called in  lead index - this is also a viewmodel, but found it beneficial to make them stand out a bit
    public class LeadFilterOptions
    {
        public string? TitleFilter { get; set; }
        public string? ValueFilter { get; set; }
        public string? BuFilter { get; set; }
    }
}