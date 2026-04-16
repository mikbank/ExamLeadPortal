using ExamLeadPortal.Models;
using ExamLeadPortal.ViewModels;

namespace ExamLeadPortal.Services
{
    public interface ILeadQueryService
    {
        List<LeadListItemViewModel> GetFilteredLeads(LeadFilterOptions filterOptions);
        RawLead? GetLeadById(string id);
    }
}