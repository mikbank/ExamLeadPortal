using System.Text.Json;
using ExamLeadPortal.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ExamLeadPortal.Repositories
{
    public class JsonLeadCorrectionRepository : ILeadCorrectionRepository
    {//repository for handling lead corrections - here we handle loading, saving, deleting and editing the corrections and store to json - this should be rewritten if using API or SQL
        private readonly string _jsonFilePath;
        private readonly ILogger<JsonLeadCorrectionRepository> _logger;

        public JsonLeadCorrectionRepository(
            IConfiguration configuration,
            ILogger<JsonLeadCorrectionRepository> logger)
        {
            _jsonFilePath = configuration["LeadCorrectionSettings:FilePath"]
                ?? throw new Exception("LeadCorrectionSettings:FilePath is not configured");

            _logger = logger;
        }

        public LeadCorrection? GetByRawLeadId(string rawLeadId)
        {
            if (string.IsNullOrWhiteSpace(rawLeadId))
            {
                return null;
            }

            return GetAll()
                .FirstOrDefault(x => x.RawLeadId == rawLeadId); //needs explanation
        }

        public void Save(LeadCorrection correction) //save a coorrection
        {
            if (correction == null)
            {
                throw new ArgumentNullException(nameof(correction));
            }

            if (string.IsNullOrWhiteSpace(correction.RawLeadId))
            {
                throw new ArgumentException("Correction must reference a raw lead id.");
            }

            var corrections = GetAll();

            var existingCorrection = corrections
                .FirstOrDefault(x => x.RawLeadId == correction.RawLeadId);

            if (existingCorrection != null)
            {
                corrections.Remove(existingCorrection);
            }

            correction.CorrectedAt = DateTime.Now;
            corrections.Add(correction);

            SaveAll(corrections);

            _logger.LogInformation(
                "Saved lead correction for raw lead {RawLeadId}",
                correction.RawLeadId);
        }

        public void Delete(string rawLeadId) // delete a correction
        {
            if (string.IsNullOrWhiteSpace(rawLeadId))
            {
                return;
            }

            var corrections = GetAll();

            var correctionToRemove = corrections
                .FirstOrDefault(x => x.RawLeadId == rawLeadId);

            if (correctionToRemove == null)
            {
                return;
            }

            corrections.Remove(correctionToRemove);
            SaveAll(corrections);

            _logger.LogInformation(
                "Deleted lead correction for raw lead {RawLeadId}",
                rawLeadId);
        }

        private List<LeadCorrection> GetAll() //fetches all corrections from json
        {
            EnsureFileExists();

            try
            {
                var json = File.ReadAllText(_jsonFilePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<LeadCorrection>();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<List<LeadCorrection>>(json, options)
                    ?? new List<LeadCorrection>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON in lead correction file: {FilePath}", _jsonFilePath);
                return new List<LeadCorrection>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while reading lead correction file: {FilePath}", _jsonFilePath);
                return new List<LeadCorrection>();
            }
        }

        private void SaveAll(List<LeadCorrection> corrections)
        {
            EnsureDirectoryExists();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(corrections, options);

            File.WriteAllText(_jsonFilePath, json);
        }

        private void EnsureFileExists()
        {
            EnsureDirectoryExists();

            if (!File.Exists(_jsonFilePath))
            {
                File.WriteAllText(_jsonFilePath, "[]");

                _logger.LogInformation(
                    "Created lead correction JSON file at {FilePath}",
                    _jsonFilePath);
            }
        }

        private void EnsureDirectoryExists()
        {
            var directory = Path.GetDirectoryName(_jsonFilePath);

            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}