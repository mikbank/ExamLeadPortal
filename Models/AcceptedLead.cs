namespace ExamLeadPortal.Models
{
    public class AcceptedLead //datastructure of an accepted lead, business logic and validation is not handled here
    {
        public required string AcceptedLeadId { get; set; } = string.Empty;
        public required string RawLeadId { get; set; } = string.Empty;
        public required string LeadTitle { get; set; } = string.Empty;
        public required string LeadSummary { get; set; } = string.Empty;
        public required string ResponsibleBU { get; set; } = string.Empty;
        public required string LeadValue { get; set; } = string.Empty;
        public required string Probability { get; set; } = string.Empty;
        public required string ResourceLink { get; set; } = string.Empty;
        public required string AcceptedBy { get; set; } = string.Empty;
        public required  DateTime AcceptedAt { get; set; }
        public required string Status { get; set; } = string.Empty;
    }
}