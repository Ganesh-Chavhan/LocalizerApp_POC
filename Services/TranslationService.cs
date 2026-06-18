using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Localizer_App.Models;

namespace Localizer_App.Services
{
    public class TranslationService
    {
        // Why: Translates list of ResourceString items in batches using the Gemini API.
        private readonly GeminiService _geminiService = new GeminiService();
        private const int BatchSize = 50;

        public async Task TranslateAsync(List<ResourceString> items, string languageName, string languageCode, string apiKey, string model)
        {
            // Why: Loop through strings in batches and translate them using Gemini.
            for (int i = 0; i < items.Count; i += BatchSize)
            {
                var batch = items.Skip(i).Take(BatchSize).ToList();
                await TranslateBatchAsync(batch, languageName, languageCode, apiKey, model);
            }
        }

        private async Task TranslateBatchAsync(List<ResourceString> batch, string languageName, string languageCode, string apiKey, string model)
        {
            // Why: Call Gemini API for a single batch of resource strings and update translations.
            string systemInstruction = GetSystemInstruction();
            string prompt = GetPrompt(batch, languageName, languageCode);
            string jsonResponse = await _geminiService.CallApiAsync(systemInstruction, prompt, apiKey, model);
            UpdateTranslations(batch, jsonResponse);
        }

        private string GetSystemInstruction()
        {
            // Why: System rules for the translator (preserve formatting, return clean JSON).
            return "You are a professional software localization system. Translate Visual C++ resource strings into the target language. Keep any formatting placeholders (%s, %d, %f, %1, %2, {0}, {1}, {2}) and escape characters (like \\n or \\t) exactly as they are. Output MUST be a valid JSON array of objects with keys 'key' and 'translated'. Do not add markdown blocks.";
        }

        private string GetPrompt(List<ResourceString> batch, string languageName, string languageCode)
        {
            // Why: Create user prompt including target language info and batch JSON content.
            var inputs = new List<object>();
            foreach (var item in batch)
            {
                inputs.Add(new { key = item.Key, text = item.Text });
            }
            string serialized = JsonSerializer.Serialize(inputs);
            return "Translate to " + languageName + " (" + languageCode + ").\n\nInput JSON:\n" + serialized;
        }

        private void UpdateTranslations(List<ResourceString> batch, string jsonResponse)
        {
            // Why: Deserialize target JSON and match keys to assign translated values.
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var outputs = JsonSerializer.Deserialize<List<TranslationOutput>>(jsonResponse, options);
            if (outputs == null) return;
            
            foreach (var item in batch)
            {
                var match = outputs.FirstOrDefault(x => x.Key == item.Key);
                if (match != null) item.Translated = match.Translated;
            }
        }
    }

    internal class TranslationOutput
    {
        public string Key { get; set; } = string.Empty;
        public string Translated { get; set; } = string.Empty;
    }
}
