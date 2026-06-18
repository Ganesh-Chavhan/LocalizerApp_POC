using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Localizer_App.Models;

namespace Localizer_App.Services
{
    public class RcResourceLoaderService
    {
        // Why: Service to load resource scripts and resolve default directories.
        private readonly RcParserService _parser = new RcParserService();

        public Dictionary<string, string> LoadFromFile(string filePath)
        {
            // Why: Load localized resources from file path.
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            string content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            return LoadFromContent(content);
        }

        public Dictionary<string, string> LoadFromContent(string content)
        {
            // Why: Load resources from in-memory string.
            var parsed = _parser.Parse(content);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in parsed)
            {
                if (!string.IsNullOrEmpty(item.Key))
                {
                    result[item.Key] = item.Text;
                }
            }
            return result;
        }

        public static string GetDefaultResourcesDirectory()
        {
            // Why: Get local resources path inside application executable directory.
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
        }

        public static string GetResourceFilePath(string resourcesDirectory, string cultureCode)
        {
            // Why: Combine resources directory and culture code to form path.
            return Path.Combine(resourcesDirectory, cultureCode + ".rc");
        }
    }
}
