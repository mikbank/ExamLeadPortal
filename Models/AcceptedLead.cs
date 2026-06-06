namespace ExamLeadPortal.Models
{
    public class AcceptedLead //datastructure of an accepted lead, business logic and validation is not handled here
    {
        public string AcceptedLeadId { get; set; } = string.Empty;
        public string RawLeadId { get; set; } = string.Empty;
        public string LeadTitle { get; set; } = string.Empty;
        public string LeadSummary { get; set; } = string.Empty;
        public string ResponsibleBU { get; set; } = string.Empty;
        public string LeadValue { get; set; } = string.Empty;
        public string Probability { get; set; } = string.Empty;
        public string ResourceLink { get; set; } = string.Empty;
        public string AcceptedBy { get; set; } = string.Empty;
        public DateTime AcceptedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}