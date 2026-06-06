using System.Text.Json;
using ExamLeadPortal.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ExamLeadPortal.Repositories
{//repository for persistent raw leads fetched from json files, POTENTIALLY a bit more realistic if leads are stored just as json objects in for example onelake
//
    public class RawLeadRepository : IRawLeadRepository
    {
        private readonly string _rawLeadFolderPath;
        private readonly ILogger<RawLeadRepository> _logger;

        public RawLeadRepository(IConfiguration configuration, ILogger<RawLeadRepository> logger)
        {
            _rawLeadFolderPath = configuration["RawLeadSettings:FolderPath"]
                ?? throw new Exception("RawLeadSettings:FolderPath is not configured");
            _logger = logger;
        }

        public List<RawLead> GetAll()//fetches all leads
        {
            if (!Directory.Exists(_rawLeadFolderPath))
            {
                _logger.LogWarning("Raw lead folder does not exist: {FolderPath}", _rawLeadFolderPath);//checks if local storage exists if not returns empty list
                return new List<RawLead>();
            }

            var files = Directory.GetFiles(_rawLeadFolderPath, "*.json");// fetches all json files from folder
            var rawLeads = new List<RawLead>();

            foreach (var file in files)//try desirializing all found json files, we trust this source
            {

                if (TryDeserializeRawLead(file, out var rawLead))
                {
                    rawLeads.Add(rawLead);
                }
            }

            _logger.LogInformation("Loaded {Count} raw leads from {FolderPath}", rawLeads.Count, _rawLeadFolderPath);

            return rawLeads;
        }

        public RawLead? GetById(string id)//fetch a specific raw lead
        {
            return GetAll().FirstOrDefault(x => x.Id == id);
        }

        private bool TryDeserializeRawLead(string filePath, out RawLead rawLead)
        {
            rawLead = null!;

            try
            {
                var json = File.ReadAllText(filePath);

                if (string.IsNullOrWhiteSpace(json))//if json file is empty, skip and log
                    {
                        _logger.LogWarning("Skipped empty JSON file: {FilePath}", filePath);
                        return false;
                    }

                var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                var result = JsonSerializer.Deserialize<RawLead>(json, options);// deserealize the file to rawlead

                if (result == null)
                    {
                        _logger.LogWarning("Deserialization returned null for file: {FilePath}", filePath);
                        return false;
                    }

                
                if (string.IsNullOrWhiteSpace(result.Id))
                    {
                        _logger.LogWarning("Raw lead missing Id in file: {FilePath}", filePath); // validation check on ID, mainly used for my own debugging, a lead service would presumes having constraints
                        return false;
                    }

                rawLead = result;

                _logger.LogTrace("Successfully deserialized raw lead: {LeadId}", rawLead.Id);               
                return true;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON in file: {FilePath}", filePath);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while reading file: {FilePath}", filePath);//outer catch-all exception on wierd errors 
                return false;
            }
        }
    }
}