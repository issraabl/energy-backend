using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace WicmicEnergyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OllamaController : ControllerBase
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<OllamaController> _logger;
        private readonly string _ollamaUrl;
        private readonly string _ollamaModel;

        public OllamaController(
            IHttpClientFactory httpFactory,
            ILogger<OllamaController> logger,
            IConfiguration config)
        {
            _httpFactory = httpFactory;
            _logger = logger;
            // ── Lit depuis appsettings.json ──────────────────────────────
            var baseUrl = config["Ollama:Url"] ?? "http://localhost:11434";
            _ollamaModel = config["Ollama:Modele"] ?? "llama3";
            _ollamaUrl = $"{baseUrl.TrimEnd('/')}/api/generate";
        }

        public class ChatRequest { public string Prompt { get; set; } = ""; }
        public class ChatResponse { public string Response { get; set; } = ""; }

        [HttpPost("chat")]
        public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest(new { error = "Le prompt ne peut pas être vide." });

            try
            {
                var http = _httpFactory.CreateClient("ollama");

                // ── Prompt système pour forcer le français ──────────────────────
                var systemInstruction = "Réponds toujours en français. Tu es un expert en gestion énergétique industrielle pour l'entreprise WICMIC en Tunisie. Sois concis et précis.";

                var ollamaBody = new
                {
                    model = _ollamaModel,
                    system = systemInstruction,   // ← instruction système séparée
                    prompt = request.Prompt,
                    stream = false,
                    options = new
                    {
                        temperature = 0.3,   // ← réduit pour des réponses plus cohérentes
                        num_predict = 400,
                        num_ctx = 2048,
                    }
                };

                var json = JsonSerializer.Serialize(ollamaBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Envoi prompt à Ollama ({model})...", _ollamaModel);

                var httpResp = await http.PostAsync(_ollamaUrl, content);

                if (!httpResp.IsSuccessStatusCode)
                {
                    _logger.LogError("Ollama status: {status}", httpResp.StatusCode);
                    return StatusCode(503, new ChatResponse
                    {
                        Response = "Le service Ollama n'est pas disponible. Assurez-vous qu'Ollama est démarré."
                    });
                }

                var respJson = await httpResp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(respJson);
                var responseText = doc.RootElement
                    .GetProperty("response")
                    .GetString() ?? "Réponse vide.";

                _logger.LogInformation("Réponse Ollama reçue ({chars} caractères).", responseText.Length);

                return Ok(new ChatResponse { Response = responseText });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erreur connexion Ollama ({url})", _ollamaUrl);
                return StatusCode(503, new ChatResponse
                {
                    Response = $"Impossible de joindre Ollama sur {_ollamaUrl}. Vérifiez que le service est démarré."
                });
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Timeout Ollama — modèle trop lent");
                return StatusCode(504, new ChatResponse
                {
                    Response = "Le modèle est en cours de chargement. Patientez 30 secondes puis réessayez."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue Ollama");
                return StatusCode(500, new ChatResponse { Response = "Erreur interne du serveur." });
            }
        }
    }
}