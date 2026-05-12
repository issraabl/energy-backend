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

        // Retourne le nom d'énergie attendu selon le rôle
        private static string? GetEnergieNomForRole(string? role) => role switch
        {
            "responsable_electricite" => "electricite",
            "responsable_gaz" => "gaz",
            "responsable_eau" => "eau",
            _ => null   // admin => pas de restriction
        };

        // Vérifie que le nom de l'énergie de l'alerte correspond au rôle
        private async Task<bool> RoleMatchesEnergie(string? role, int? energieId)
        {
            var nomAttendu = GetEnergieNomForRole(role);
            if (nomAttendu == null) return true; // admin, pas de restriction

            if (energieId == null) return false;

            var energie = await _context.Energies.FindAsync(energieId.Value);
            if (energie == null) return false;

            return energie.Nom.ToLower() == nomAttendu;
        }

        // ✅ GET — filtrage par energieNom optionnel (ou energieId pour rétro-compatibilité)
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Alertes>>> GetAlertes([FromQuery] string? energieNom)
        {
            var query = _context.Alertes
                .Include(a => a.Equipement)
                .Include(a => a.Energie)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(energieNom))
            {
                var nom = energieNom.ToLower();
                query = query.Where(a => a.Energie!.Nom.ToLower() == nom);
            }

            return Ok(await query.OrderByDescending(a => a.DateCreation).ToListAsync());
        }

        // GET: api/Alertes/5
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau")]
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

        // ✅ POST — le responsable crée une alerte uniquement pour son énergie (par nom)
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau")]
        [HttpPost]
        public async Task<ActionResult<Alertes>> CreateAlerte(Alertes alerte)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!await RoleMatchesEnergie(role, alerte.EnergieId))
                return Forbid();

            alerte.DateCreation = DateTime.Now;
            _context.Alertes.Add(alerte);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAlerte), new { id = alerte.IdAlerte }, alerte);
        }

        // ✅ PUT — le responsable modifie uniquement les alertes de son énergie (par nom)
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlerte(int id, Alertes alerte)
        {
            if (id != alerte.IdAlerte) return BadRequest();

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var existing = await _context.Alertes.FindAsync(id);
            if (existing == null) return NotFound();

            if (!await RoleMatchesEnergie(role, existing.EnergieId))
                return Forbid();

            _context.Entry(existing).CurrentValues.SetValues(alerte);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ✅ DELETE — le responsable supprime uniquement les alertes de son énergie (par nom)
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlerte(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var alerte = await _context.Alertes.FindAsync(id);
            if (alerte == null) return NotFound();

            if (!await RoleMatchesEnergie(role, alerte.EnergieId))
                return Forbid();

            _context.Alertes.Remove(alerte);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}