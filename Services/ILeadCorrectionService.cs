using ExamLeadPortal.Models;

namespace ExamLeadPortal.Services
{
    public interface ILeadCorrectionService
    {
        LeadCorrection? GetCorrection(string rawLeadId);
        void SaveCorrection(LeadCorrection correction);
        void DeleteCorrection(string rawLeadId);
        RawLead ApplyCorrection(RawLead rawLead, LeadCorrection? correction);
    }
}