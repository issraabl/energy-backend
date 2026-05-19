using Microsoft.AspNetCore.Mvc;
using EnergyTrackerr.Services;

namespace EnergyTrackerr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly ExcelImportService _importService;

        public ImportController(ExcelImportService importService)
            => _importService = importService;

        [HttpPost("mesures")]
        public async Task<IActionResult> ImporterMesures(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Fichier manquant ou vide." });

            if (!file.FileName.EndsWith(".xlsx"))
                return BadRequest(new { message = "Format invalide. Utilisez un fichier .xlsx" });

            try
            {
                var count = await _importService.ImporterFichier(file);
                return Ok(new { message = $"{count} mesures importées avec succès." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erreur lors de l'import.", detail = ex.Message });
            }
        }
    }
}