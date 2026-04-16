namespace ExamLeadPortal.Services
{
    public interface ILeadAcceptanceService
    {
        bool TryAcceptLead(string rawLeadId, string responsibleBU, string acceptedBy, out string errorMessage);
    }
}