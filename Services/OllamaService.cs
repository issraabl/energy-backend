using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

namespace EnergyTrackerr.Services
{
    public class OllamaService
    {
        private readonly HttpClient _http;

        public OllamaService(HttpClient http)
        {
            _http = http;
            _http.Timeout = TimeSpan.FromMinutes(5);
        }

        public async Task<string> AskModelAsync(string question, string modelId = "gemma3:4b")
        {
            if (string.IsNullOrWhiteSpace(question))
                return "Question vide !";

            var request = new
            {
                model = modelId,
                prompt = question,
                stream = false
            };

            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _http.PostAsync("http://localhost:11434/api/generate", content);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);

                
                if (doc.RootElement.TryGetProperty("response", out var res))
                {
                    return res.GetString() ?? "Pas de réponse";
                }

                return "Réponse format inconnue";
            }
            catch (TaskCanceledException)
            {
                return "Timeout : le modèle a mis trop de temps à répondre";
            }
            catch (HttpRequestException ex)
            {
                return $"Erreur HTTP : {ex.Message}";
            }
            catch (JsonException)
            {
                return "Erreur : impossible de lire la réponse JSON";
            }
            catch (Exception ex)
            {
                return $"Erreur inconnue : {ex.Message}";
            }
        }
    }
}