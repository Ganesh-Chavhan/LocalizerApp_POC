using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Localizer_App.Services
{
    public class TranslationMemoryService
    {
        // Why: Local caching service to read/write translations to avoid redundant API queries.
        private readonly string _tmFolder;

        public TranslationMemoryService()
        {
            // Why: Resolve cache directory inside application directory.
            _tmFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TranslationMemory");
            Directory.CreateDirectory(_tmFolder);
        }

        private string GetFilePath(string cultureCode)
        {
            // Why: Get JSON file path for target culture.
            return Path.Combine(_tmFolder, cultureCode + ".json");
        }

        public Dictionary<string, string> LoadMemory(string cultureCode)
        {
            // Why: Load JSON dictionary for culture from file.
            string path = GetFilePath(cultureCode);
            if (!File.Exists(path))
            {
                return CreateNewMemory(path);
            }
            return TryReadMemory(path);
        }

        private Dictionary<string, string> CreateNewMemory(string path)
        {
            // Why: Create default empty settings file.
            File.WriteAllText(path, "{}", System.Text.Encoding.UTF8);
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private Dictionary<string, string> TryReadMemory(string path)
        {
            // Why: Parse JSON cache and handle exceptions cleanly.
            try
            {
                string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return ConvertToDict(dict);
            }
            catch
            {
                return HandleCorrupted(path);
            }
        }

        private Dictionary<string, string> ConvertToDict(Dictionary<string, string>? dict)
        {
            // Why: Create case-insensitive dictionary representation of cache dict.
            if (dict == null) return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return new Dictionary<string, string>(dict, StringComparer.OrdinalIgnoreCase);
        }

        private Dictionary<string, string> HandleCorrupted(string path)
        {
            // Why: If JSON is corrupted, move file to a backup name and create empty settings.
            string time = DateTime.Now.ToString("yyyyMMddHHmmss");
            string backupPath = path + ".corrupted." + time + ".bak";
            if (File.Exists(path)) File.Move(path, backupPath);
            return CreateNewMemory(path);
        }

        public void SaveMemory(string cultureCode, Dictionary<string, string> memory)
        {
            // Why: Save memory dictionary to JSON file.
            string path = GetFilePath(cultureCode);
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(memory, options);
            File.WriteAllText(path, json, System.Text.Encoding.UTF8);
        }
    }
}
