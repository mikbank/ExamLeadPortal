using ExamLeadPortal.Models;

namespace ExamLeadPortal.ViewModels
{
    public class LeadListViewModel
    {
        public List<Lead> Leads { get; set; } = new();
        public LeadFilterOptions Filters { get; set; } = new();
    }
}