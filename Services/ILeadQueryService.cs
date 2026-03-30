using ExamLeadPortal.Models;
using ExamLeadPortal.ViewModels;

namespace ExamLeadPortal.Services
{
    public interface ILeadQueryService
    {
        List<Lead> GetFilteredLeads(LeadFilterOptions filterOptions);
        Lead? GetLeadById(string id);
    }
}