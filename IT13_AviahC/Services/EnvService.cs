using System;
using System.Collections.Generic;
using System.IO;

namespace IT13_AviahC.Services
{
    public static class EnvService
    {
        private static readonly Dictionary<string, string> _envVars = new();

        static EnvService()
        {
            try
            {
                // Try to find .env file in current directory or parent directories
                // This works well for local development on Windows.
                string? currentDir = AppDomain.CurrentDomain.BaseDirectory;
                string? envFilePath = null;

                while (currentDir != null)
                {
                    string potentialPath = Path.Combine(currentDir, ".env");
                    if (File.Exists(potentialPath))
                    {
                        envFilePath = potentialPath;
                        break;
                    }
                    
                    // Also check for the case where we are in the it13_aviahc folder
                    string parentDir = Directory.GetParent(currentDir)?.FullName;
                    if (parentDir == null) break;
                    
                    currentDir = parentDir;
                }

                if (envFilePath != null)
                {
                    var lines = File.ReadAllLines(envFilePath);
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                        var parts = line.Split('=', 2);
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string value = parts[1].Trim().Trim('"').Trim('\'');
                            _envVars[key] = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load .env file: {ex.Message}");
            }
        }

        public static string Get(string key, string defaultValue = "")
        {
            return _envVars.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}
