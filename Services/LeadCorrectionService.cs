using System.Text.Json;
using ExamLeadPortal.Models;
using ExamLeadPortal.Repositories;

namespace ExamLeadPortal.Services
{
    public class LeadCorrectionService : ILeadCorrectionService
    {
        private readonly ILeadCorrectionRepository _leadCorrectionRepository;

        public LeadCorrectionService(ILeadCorrectionRepository leadCorrectionRepository)
        {
            _leadCorrectionRepository = leadCorrectionRepository;
        }

        public LeadCorrection? GetCorrection(string rawLeadId)
        {
            if (string.IsNullOrWhiteSpace(rawLeadId))
            {
                throw new ArgumentException("Raw lead id cannot be empty.", nameof(rawLeadId));
            }

            return _leadCorrectionRepository.GetByRawLeadId(rawLeadId);
        }

        public void SaveCorrection(LeadCorrection correction)
        {
            if (correction == null)
            {
                throw new ArgumentNullException(nameof(correction));
            }

            if (string.IsNullOrWhiteSpace(correction.RawLeadId))
            {
                throw new ArgumentException("Correction must reference a raw lead id.");
            }

            _leadCorrectionRepository.Save(correction);
        }

        public void DeleteCorrection(string rawLeadId)
        {
            if (string.IsNullOrWhiteSpace(rawLeadId))
            {
                throw new ArgumentException("Raw lead id cannot be empty.", nameof(rawLeadId));
            }

            _leadCorrectionRepository.Delete(rawLeadId);
        }

        public RawLead ApplyCorrection(RawLead rawLead, LeadCorrection? correction)
        {
            if (rawLead == null)
            {
                throw new ArgumentNullException(nameof(rawLead));
            }

            if (correction == null)
            {
                return rawLead;
            }

            var correctedPayload = new Dictionary<string, JsonElement>(
                rawLead.LeadPayload
            );

            foreach (var correctedField in correction.CorrectedPayload)
            {
                correctedPayload[correctedField.Key] = correctedField.Value;
            }

            return new RawLead
            {
                Id = rawLead.Id,

                LeadTitle = correction.LeadTitle ?? rawLead.LeadTitle,
                LeadSummary = correction.LeadSummary ?? rawLead.LeadSummary,
                LeadValue = correction.LeadValue ?? rawLead.LeadValue,
                ResourceLink = correction.ResourceLink ?? rawLead.ResourceLink,

                AffectedBUs = correction.AffectedBUs != null
                    ? new List<string>(correction.AffectedBUs)
                    : new List<string>(rawLead.AffectedBUs),

                LeadPayload = correctedPayload
            };
        }
    }
}