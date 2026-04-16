using ExamLeadPortal.Models;

namespace ExamLeadPortal.ViewModels
{
    public class LeadDetailsViewModel
    {
        public RawLead? Lead { get; set; }
        public bool HasCorrections { get; set; }

        public List<string> ValidBUs { get; set; } = new();
        public string SelectedResponsibleBU { get; set; } = string.Empty;

        public bool IsAlreadyAccepted { get; set; }
        public string? Message { get; set; }
    }
}