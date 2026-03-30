namespace ExamLeadPortal.Models
{
    public class LeadCorrection
    {
        public string LeadId { get; set; } = string.Empty;
        public List<string> CorrectedAffectedBUs { get; set; } = new();
        public string CorrectedBy { get; set; } = string.Empty;
        public DateTime CorrectedAt { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}