namespace ExamLeadPortal.ViewModels
{
    public class LeadCorrectionEditViewModel
    {
        public string RawLeadId { get; set; } = string.Empty;

        public string LeadTitle { get; set; } = string.Empty;
        public string LeadSummary { get; set; } = string.Empty;
        public string LeadValue { get; set; } = string.Empty;
        public string ResourceLink { get; set; } = string.Empty;

        public List<string> AffectedBUs { get; set; } = new();

        public Dictionary<string, string> PayloadValues { get; set; } = new();

        public List<string> ValidBUs { get; set; } = new();

        public string? Message { get; set; }
    }
}