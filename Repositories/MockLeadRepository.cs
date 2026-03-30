using System.Text.Json;
using ExamLeadPortal.Models;

namespace ExamLeadPortal.Repositories
{
    public class MockLeadRepository : ILeadRepository
    {
        public List<Lead> GetAll()
        {
            var jsonStrings = new List<string>
            {
                """
                {
                  "Id": "11111111-1111-1111-1111-111111111111",
                  "LeadTitle": "Pipeline improvements in Tønder",
                  "LeadSummary": "Municipality wants to continue work on pipelines and improvements.",
                  "LeadValue": "Limited",
                  "ResourceLink": "https://example.com/doc1",
                  "AffectedBUs": ["GLIT", "VAFO", "BUILD"],
                  "LeadPayload": {
                    "Probability": "High",
                    "LongDescription": "Tønder kommune ønsker at arbejde videre med rørledninger og forbedringer i næste kvartal.",
                    "PossibleSubjects": ["Profylakseanalyse", "Bygningskonstruktion", "Generel rådgivning"]
                  }
                }
                """,
                """
                {
                  "Id": "22222222-2222-2222-2222-222222222222",
                  "LeadTitle": "Bridge maintenance tender",
                  "LeadSummary": "Potential future tender related to bridge inspections.",
                  "LeadValue": "Medium",
                  "ResourceLink": "https://example.com/doc2",
                  "AffectedBUs": ["BUILD", "INFRA"],
                  "LeadPayload": {
                    "Probability": "Medium",
                    "TenderType": "Public",
                    "BudgetEstimate": "5-10 million DKK"
                  }
                }
                """,
                """
                {
                  "Id": "33333333-3333-3333-3333-333333333333",
                  "LeadTitle": "Water environment analysis",
                  "LeadSummary": "Environmental monitoring work may expand over coming years.",
                  "LeadValue": "High",
                  "ResourceLink": "https://example.com/doc3",
                  "AffectedBUs": ["ENV", "VAFO"],
                  "LeadPayload": {
                    "Probability": "High",
                    "TimeHorizon": "3 years",
                    "PossibleSubjects": ["Miljøanalyse", "Vandkvalitet", "Rådgivning"]
                  }
                }
                """,
                """
                {
                  "Id": "44444444-4444-4444-4444-444444444444",
                  "LeadTitle": "School renovation program",
                  "LeadSummary": "Renovation plans indicate possible advisory opportunities.",
                  "LeadValue": "Low",
                  "ResourceLink": "https://example.com/doc4",
                  "AffectedBUs": ["BUILD"],
                  "LeadPayload": {
                    "Probability": "Low",
                    "LongDescription": "Several school buildings may need renovation assessment.",
                    "Region": "Sjælland"
                  }
                }
                """
            };

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return jsonStrings
                .Select(json => JsonSerializer.Deserialize<Lead>(json, options))
                .Where(lead => lead != null)
                .Cast<Lead>()
                .ToList();
        }

        public Lead? GetById(string id)
        {
            return GetAll().FirstOrDefault(x => x.Id == id);
        }
    }
}