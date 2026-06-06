using ExamLeadPortal.Models;
using ExamLeadPortal.ViewModels;

namespace ExamLeadPortal.Services
{//interface for the queryservice - leads can either be returned by parsing in filter options or by parsing in the ID
    public interface ILeadQueryService
    {
        List<LeadListItemViewModel> GetFilteredLeads(LeadFilterOptions filterOptions);
        RawLead? GetLeadById(string id);
    }
}