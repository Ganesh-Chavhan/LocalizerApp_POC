using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Localizer_App.Models;

namespace Localizer_App.Services
{
    public class ValidationOutputItem
    {
        public string Key { get; set; } = string.Empty;
        public int Score { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Feedback { get; set; } = string.Empty;
    }

    public class AiValidationService
    {
        // Why: Evaluates translation quality in batches using the Gemini API.
        private readonly GeminiService _geminiService = new GeminiService();
        private const int BatchSize = 25;

        public async Task<List<ValidationOutputItem>> ValidateAsync(List<ResourceString> items, string languageName, string languageCode, string apiKey, string model)
        {
            // Why: Run validation batches and collect all results.
            var results = new List<ValidationOutputItem>();
            for (int i = 0; i < items.Count; i += BatchSize)
            {
                var batch = items.Skip(i).Take(BatchSize).ToList();
                var batchResults = await ValidateBatchAsync(batch, languageName, languageCode, apiKey, model);
                results.AddRange(batchResults);
            }
            return results;
        }

        private async Task<List<ValidationOutputItem>> ValidateBatchAsync(List<ResourceString> batch, string languageName, string languageCode, string apiKey, string model)
        {
            // Why: Call Gemini API for a single validation batch and return quality items.
            string systemInstruction = GetSystemInstruction();
            string prompt = GetPrompt(batch, languageName, languageCode);
            string jsonResponse = await _geminiService.CallApiAsync(systemInstruction, prompt, apiKey, model);
            return ParseResults(jsonResponse);
        }

        private string GetSystemInstruction()
        {
            // Why: Instructions directing the AI model to behave as a QA reviewer.
            return "You are a professional software localization QA system. Evaluate translations based on accuracy, meaning, grammar, and UI suitability (placeholder preservation). Assign a score (0 to 100). Status: 90-100 is 'Excellent', 80-89 is 'Good', below 80 is 'Needs Review'. Provide brief feedback. Output MUST be a valid JSON array of objects with keys 'key', 'score', 'status', and 'feedback'. Do not add markdown blocks.";
        }

        private string GetPrompt(List<ResourceString> batch, string languageName, string languageCode)
        {
            // Why: Build input JSON of original vs translation texts for the QA prompt.
            var inputs = new List<object>();
            foreach (var item in batch)
            {
                inputs.Add(new { key = item.Key, source = item.Text, translation = item.Translated });
            }
            string serialized = JsonSerializer.Serialize(inputs);
            return "Validate translations to " + languageName + " (" + languageCode + ").\n\nInput JSON:\n" + serialized;
        }

        private List<ValidationOutputItem> ParseResults(string jsonResponse)
        {
            // Why: Safely deserialize the QA response from the API.
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var list = JsonSerializer.Deserialize<List<ValidationOutputItem>>(jsonResponse, options);
            return list ?? new List<ValidationOutputItem>();
        }
    }
}
