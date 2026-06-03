using ExamLeadPortal.Models;

namespace ExamLeadPortal.Repositories
{
    public interface ILeadCorrectionRepository
    {
        LeadCorrection? GetByRawLeadId(string rawLeadId);
        void Save(LeadCorrection correction);
        void Delete(string rawLeadId);
    }
}