using EnergyTrackerr.DTOs;
using EnergyTrackerr.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnergyTrackerr.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AnomaliesController : ControllerBase
    {
        private readonly IAnomalieService _anomalieService;

        public AnomaliesController(IAnomalieService anomalieService)
        {
            _anomalieService = anomalieService;
        }
        [HttpGet]
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        public async Task<IActionResult> GetAll()
        {
            var anomalies = await _anomalieService.GetAllAsync();
            return Ok(anomalies);
        }
        //  GET api/Anomalies/mesure/5
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpGet("mesure/{mesureId}")]
        public async Task<IActionResult> AnalyserMesure(
            int mesureId,
            [FromQuery] double seuilEcart = 30,
            [FromQuery] double seuilPic = 50)
        {
            try
            {
                var parametres = new ParametresDetectionDto
                {
                    SeuilEcartMoyenne = seuilEcart,
                    SeuilPicSoudain = seuilPic
                };

                var resultat = await _anomalieService.AnalyserMesureAsync(mesureId, parametres);
                return Ok(resultat);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erreur = ex.Message });
            }
        }

        //  GET api/Anomalies/equipement/5
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpGet("equipement/{equipementId}")]
        public async Task<IActionResult> AnalyserEquipement(
            int equipementId,
            [FromQuery] DateTime? debut,
            [FromQuery] DateTime? fin,
            [FromQuery] double seuilEcart = 30,
            [FromQuery] double seuilPic = 50)
        {
            var parametres = new ParametresDetectionDto
            {
                SeuilEcartMoyenne = seuilEcart,
                SeuilPicSoudain = seuilPic
            };

            var rapport = await _anomalieService.AnalyserEquipementAsync(
                equipementId, debut, fin, parametres);

            return Ok(rapport);
        }

        //  GET api/Anomalies/periode
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpGet("periode")]
        public async Task<IActionResult> AnalyserPeriode(
            [FromQuery] DateTime debut,
            [FromQuery] DateTime fin,
            [FromQuery] double seuilEcart = 30,
            [FromQuery] double seuilPic = 50)
        {
            if (debut >= fin)
                return BadRequest(new { erreur = "La date de début doit être antérieure à la date de fin" });

            var parametres = new ParametresDetectionDto
            {
                SeuilEcartMoyenne = seuilEcart,
                SeuilPicSoudain = seuilPic
            };

            var rapport = await _anomalieService.AnalyserPeriodeAsync(debut, fin, parametres);
            return Ok(rapport);
        }

        //  POST api/Anomalies/detecter-et-alerter
        [HttpPost("detecter-et-alerter")]
        [Authorize(Roles = "administrateur,responsable_energie")]
        public async Task<IActionResult> DetecterEtAlerter(
            [FromBody] ParametresDetectionDto? parametres = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { erreur = "Utilisateur non identifié" });

            var rapport = await _anomalieService.DetecterEtAlertAsync(userId, parametres);

            return Ok(new
            {
                message = $"Détection terminée : {rapport.TotalAnomaliesDetectees} anomalie(s) détectée(s)",
                rapport
            });
        }
    }
}