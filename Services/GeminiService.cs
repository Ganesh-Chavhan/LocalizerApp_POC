using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Localizer_App.Services
{
    public class GeminiService
    {
        // Why: Centralized service to send structured JSON queries to the Google Gemini API.
        private static readonly HttpClient Client = new HttpClient();

        public async Task<string> CallApiAsync(string systemInstruction, string prompt, string apiKey, string model)
        {
            // Why: Main method to call Gemini generateContent endpoint.
            string url = "https://generativelanguage.googleapis.com/v1beta/models/" + model + ":generateContent?key=" + apiKey;
            string requestJson = BuildRequestJson(systemInstruction, prompt);
            StringContent content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await Client.PostAsync(url, content);
            return await ParseResponseAsync(response);
        }

        private string BuildRequestJson(string systemInstruction, string prompt)
        {
            // Why: Construct the request payload for the API ensuring JSON response mime type.
            var payload = new
            {
                systemInstruction = new { parts = new[] { new { text = systemInstruction } } },
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { responseMimeType = "application/json", temperature = 0.0 }
            };
            return JsonSerializer.Serialize(payload);
        }

        private async Task<string> ParseResponseAsync(HttpResponseMessage response)
        {
            // Why: Handle errors and read the text candidates from the HTTP response.
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception("Gemini API error (" + response.StatusCode + "): " + error);
            }
            string json = await response.Content.ReadAsStringAsync();
            return ExtractTextFromJson(json);
        }

        private string ExtractTextFromJson(string json)
        {
            // Why: Drill down into candidates -> content -> parts -> text to get response.
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                JsonElement root = document.RootElement;
                JsonElement candidates = root.GetProperty("candidates");
                JsonElement part = candidates[0].GetProperty("content").GetProperty("parts")[0];
                return part.GetProperty("text").GetString() ?? string.Empty;
            }
        }
    }
}
