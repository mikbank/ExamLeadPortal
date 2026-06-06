namespace ExamLeadPortal.Services
{//interface for lead acceptance service
    public interface ILeadAcceptanceService
    {
        bool TryAcceptLead(string rawLeadId, string responsibleBU, string acceptedBy, out string errorMessage);
    }
}