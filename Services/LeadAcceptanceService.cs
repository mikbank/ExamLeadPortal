using System.Text.Json;
using ExamLeadPortal.Models;
using ExamLeadPortal.Repositories;

namespace ExamLeadPortal.Services
{//this service is used to accept a lead - please note contains a helper for displaying propbability value which is often returened on ai generated leads
    public class LeadAcceptanceService : ILeadAcceptanceService
    {
        private readonly ILeadQueryService _leadQueryService;
        private readonly IAcceptedLeadRepository _acceptedLeadRepository;

        private static readonly List<string> ValidBUs = //old static list of BU's in the real world this would come from either SQL or at least the app settings
        [
            "BUILD",
            "ENERGY",
            "ENV",
            "GLIT",
            "INFRA",
            "IT",
            "VAFO"
        ];

        public LeadAcceptanceService(
            ILeadQueryService leadQueryService,
            IAcceptedLeadRepository acceptedLeadRepository)
        {
            _leadQueryService = leadQueryService;
            _acceptedLeadRepository = acceptedLeadRepository;
        }

        public bool TryAcceptLead(string rawLeadId, string responsibleBU, string acceptedBy, out string errorMessage)
        {
            errorMessage = string.Empty;

            var currentLead = _leadQueryService.GetLeadById(rawLeadId);//using query service to get the current state of the lead for pushing to acceptance-state

            if (currentLead == null)
            {
                errorMessage = "Raw lead was not found.";
                return false;
            }

            if (_acceptedLeadRepository.ExistsForRawLead(rawLeadId))
            {
                errorMessage = "This lead has already been accepted.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(responsibleBU) || !ValidBUs.Contains(responsibleBU))
            {
                errorMessage = "Responsible BU is not valid.";
                return false;
            }

            var acceptedLead = new AcceptedLead
            {
                AcceptedLeadId = Guid.NewGuid().ToString(),
                RawLeadId = currentLead.Id,
                LeadTitle = currentLead.LeadTitle,
                LeadSummary = currentLead.LeadSummary,
                ResponsibleBU = responsibleBU,
                LeadValue = currentLead.LeadValue,
                Probability = GetProbability(currentLead),
                ResourceLink = currentLead.ResourceLink,
                AcceptedBy = acceptedBy,//!!!!!!!!!please note currently defined statically in the Lead Controller!!!!
                AcceptedAt = DateTime.UtcNow,
                Status = "Accepted"
            };

            _acceptedLeadRepository.Save(acceptedLead);

            return true;
        }

        private static string GetProbability(RawLead rawLead) //Helper function to try fetching a probability element in the json, most AI lead generators use this.
        {
            if (rawLead.LeadPayload.TryGetValue("Probability", out JsonElement probabilityElement))
            {
                return probabilityElement.ToString();
            }

            return "Unknown";
        }
    }
}