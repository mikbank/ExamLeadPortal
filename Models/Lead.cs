using System.Text.Json;

namespace ExamLeadPortal.Models
{
    public class Lead
    {
        public string Id { get; set; } = string.Empty;
        public string LeadTitle { get; set; } = string.Empty;
        public string LeadSummary { get; set; } = string.Empty;
        public string LeadValue { get; set; } = string.Empty;
        public string ResourceLink { get; set; } = string.Empty;
        public List<string> AffectedBUs { get; set; } = new();
        public Dictionary<string, JsonElement> LeadPayload { get; set; } = new();
    }
}