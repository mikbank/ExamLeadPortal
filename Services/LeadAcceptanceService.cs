using System.Text.Json;
using ExamLeadPortal.Models;
using ExamLeadPortal.Repositories;

namespace ExamLeadPortal.Services
{
    public class LeadAcceptanceService : ILeadAcceptanceService
    {
        private readonly IRawLeadRepository _rawLeadRepository;
        private readonly IAcceptedLeadRepository _acceptedLeadRepository;

        private static readonly List<string> ValidBUs =
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
            IRawLeadRepository rawLeadRepository,
            IAcceptedLeadRepository acceptedLeadRepository)
        {
            _rawLeadRepository = rawLeadRepository;
            _acceptedLeadRepository = acceptedLeadRepository;
        }

        public bool TryAcceptLead(string rawLeadId, string responsibleBU, string acceptedBy, out string errorMessage)
        {
            errorMessage = string.Empty;

            var rawLead = _rawLeadRepository.GetById(rawLeadId);

            if (rawLead == null)
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
                RawLeadId = rawLead.Id,
                LeadTitle = rawLead.LeadTitle,
                LeadSummary = rawLead.LeadSummary,
                ResponsibleBU = responsibleBU,
                LeadValue = rawLead.LeadValue,
                Probability = GetProbability(rawLead),
                ResourceLink = rawLead.ResourceLink,
                AcceptedBy = acceptedBy,
                AcceptedAt = DateTime.UtcNow,
                Status = "Accepted"
            };

            _acceptedLeadRepository.Save(acceptedLead);

            return true;
        }

        private static string GetProbability(RawLead rawLead)
        {
            if (rawLead.LeadPayload.TryGetValue("Probability", out JsonElement probabilityElement))
            {
                return probabilityElement.ToString();
            }

            return "Unknown";
        }
    }
}