using EnergyTrackerr.Data;
using EnergyTrackerr.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EnergyTrackerr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AlertesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AlertesController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET — filtrage par energieId optionnel
        // Tous les rôles connectés peuvent lire les alertes de n'importe quelle énergie
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Alertes>>> GetAlertes([FromQuery] int? energieId)
        {
            var query = _context.Alertes
                .Include(a => a.Equipement)
                .Include(a => a.Energie)
                .AsQueryable();

            if (energieId.HasValue)
                query = query.Where(a => a.EnergieId == energieId.Value);

            return Ok(await query.OrderByDescending(a => a.DateCreation).ToListAsync());
        }

        // GET: api/Alertes/5
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Alertes>> GetAlerte(int id)
        {
            var alerte = await _context.Alertes
                .Include(a => a.Equipement)
                .Include(a => a.Energie)
                .FirstOrDefaultAsync(a => a.IdAlerte == id);

            if (alerte == null) return NotFound();
            return alerte;
        }

        // ✅ POST — le responsable crée une alerte uniquement pour son énergie
        [Authorize(Roles = "administrateur,responsable_energie,responsable_electricite,responsable_gaz,responsable_eau")]
        [HttpPost]
        public async Task<ActionResult<Alertes>> CreateAlerte(Alertes alerte)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // Vérifier que le responsable crée une alerte pour son énergie uniquement
            if (role == "responsable_electricite" && alerte.EnergieId != 1) return Forbid();
            if (role == "responsable_gaz" && alerte.EnergieId != 2) return Forbid();
            if (role == "responsable_eau" && alerte.EnergieId != 3) return Forbid();

            alerte.DateCreation = DateTime.Now;
            _context.Alertes.Add(alerte);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAlerte), new { id = alerte.IdAlerte }, alerte);
        }

        // ✅ PUT — le responsable modifie uniquement les alertes de son énergie
        [Authorize(Roles = "administrateur,responsable_energie,responsable_electricite,responsable_gaz,responsable_eau")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlerte(int id, Alertes alerte)
        {
            if (id != alerte.IdAlerte) return BadRequest();

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var existing = await _context.Alertes.FindAsync(id);
            if (existing == null) return NotFound();

            if (role == "responsable_electricite" && existing.EnergieId != 1) return Forbid();
            if (role == "responsable_gaz" && existing.EnergieId != 2) return Forbid();
            if (role == "responsable_eau" && existing.EnergieId != 3) return Forbid();

            _context.Entry(existing).CurrentValues.SetValues(alerte);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ✅ DELETE — le responsable supprime uniquement les alertes de son énergie
        [Authorize(Roles = "administrateur,responsable_energie,responsable_electricite,responsable_gaz,responsable_eau")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlerte(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var alerte = await _context.Alertes.FindAsync(id);
            if (alerte == null) return NotFound();

            if (role == "responsable_electricite" && alerte.EnergieId != 1) return Forbid();
            if (role == "responsable_gaz" && alerte.EnergieId != 2) return Forbid();
            if (role == "responsable_eau" && alerte.EnergieId != 3) return Forbid();

            _context.Alertes.Remove(alerte);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}