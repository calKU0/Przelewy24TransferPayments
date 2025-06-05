using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Przelewy24TransferPayments.Logging
{
    public static class ResponseLogger
    {
        public static void SaveResponseToFile(string content, string endpoint)
        {
            try
            {
                var safeEndpoint = string.Join("_", endpoint.Split(Path.GetInvalidFileNameChars()));
                var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "Responses", safeEndpoint);
                Directory.CreateDirectory(logsDir);

                var fileName = $"response_{DateTime.Now:yyyyMMddHHmmss}.json";
                var filePath = Path.Combine(logsDir, fileName);

                string output;

                try
                {
                    var jsonDoc = JsonDocument.Parse(content);
                    output = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                }
                catch (JsonException)
                {
                    output = content;
                }

                File.WriteAllText(filePath, output);
                Log.Information("Zapisano response do pliku: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Nie udało się zapisać odpowiedzi do pliku.");
            }
        }

    }
}

