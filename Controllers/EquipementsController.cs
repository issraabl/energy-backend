using EnergyTrackerr.Data;
using EnergyTrackerr.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EnergyTrackerr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipementsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EquipementsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Equipements
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz,responsable_energie")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Equipement>>> GetEquipements()
        {
            return await _context.Equipements
                                 .Include(e => e.Zone)
                                 .Include(e => e.Energie)
                                 .ToListAsync();
        }

        // GET: api/Equipements/5
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz,responsable_energie")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Equipement>> GetEquipement(int id)
        {
            var eq = await _context.Equipements
                                   .Include(e => e.Zone)
                                   .Include(e => e.Energie)
                                   .FirstOrDefaultAsync(e => e.IdEquipement == id);
            if (eq == null) return NotFound();
            return eq;
        }

        // POST: api/Equipements
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz,responsable_energie")]
        [HttpPost]
        public async Task<ActionResult<Equipement>> CreateEquipement([FromBody] Equipement eq)
        {
            if (eq == null) return BadRequest("Equipement invalide.");

            eq.Zone = null;
            eq.Energie = null;
            eq.Mesures = new();
            eq.Alertes = new();

            _context.Equipements.Add(eq);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEquipement), new { id = eq.IdEquipement }, eq);
        }

        // PUT: api/Equipements/5
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz,responsable_energie")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEquipement(int id, [FromBody] Equipement eq)
        {
            if (id != eq.IdEquipement)
                return BadRequest("ID ne correspond pas.");

            var existing = await _context.Equipements.FindAsync(id);
            if (existing == null)
                return NotFound();

            // ✅ Champs exacts du modèle Equipement
            existing.Nom = eq.Nom;
            existing.TypeEquipement = eq.TypeEquipement;   // ← corrigé
            existing.Statut = eq.Statut;
            existing.Puissance = eq.Puissance;
            existing.Localisation = eq.Localisation;
            existing.DateMiseEnService = eq.DateMiseEnService;
            existing.DateInstallation = eq.DateInstallation;
            existing.Description = eq.Description;

            // FK uniquement
            existing.EnergieId = eq.EnergieId;
            existing.ZoneId = eq.ZoneId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EquipementExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/Equipements/5
        [Authorize(Roles = "administrateur")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEquipement(int id)
        {
            var eq = await _context.Equipements.FindAsync(id);
            if (eq == null) return NotFound();

            _context.Equipements.Remove(eq);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EquipementExists(int id)
        {
            return _context.Equipements.Any(e => e.IdEquipement == id);
        }
    }
}