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
    public class SeuilController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SeuilController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/seuil → tous les seuils
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var seuils = await _context.Seuils
                .Include(s => s.Energie)
                .ToListAsync();

            return Ok(seuils);
        }

        // GET api/seuil/energie/3
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz")]
        [HttpGet("energie/{energieId}")]
        public async Task<IActionResult> GetByEnergie(int energieId)
        {
            var seuil = await _context.Seuils
                .FirstOrDefaultAsync(s => s.EnergieId == energieId);

            if (seuil == null)
                return NotFound(new
                {
                    message = "Aucun seuil défini pour cette énergie."
                });

            return Ok(seuil);
        }

        // POST api/seuil
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Seuil seuil)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // Vérification des droits
            if (
                (role == "responsable_electricite" && seuil.EnergieId != 1) ||
                (role == "responsable_gaz" && seuil.EnergieId != 2) ||
                (role == "responsable_eau" && seuil.EnergieId != 3)
               )
            {
                return Forbid("Vous ne pouvez créer un seuil que pour votre énergie.");
            }

            var existant = await _context.Seuils
                .FirstOrDefaultAsync(s => s.EnergieId == seuil.EnergieId);

            if (existant != null)
            {
                return BadRequest(new
                {
                    message = "Un seuil existe déjà pour cette énergie."
                });
            }

            _context.Seuils.Add(seuil);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByEnergie),
                new { energieId = seuil.EnergieId }, seuil);
        }

        // PUT api/seuil/energie/3
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau")]
        [HttpPut("energie/{energieId}")]
        public async Task<IActionResult> Update(int energieId, [FromBody] Seuil updated)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // Vérification des droits
            if (
                (role == "responsable_electricite" && energieId != 1) ||
                (role == "responsable_gaz" && energieId != 2) ||
                (role == "responsable_eau" && energieId != 3)
               )
            {
                return Forbid("Vous ne pouvez modifier que votre énergie.");
            }

            var seuil = await _context.Seuils
                .FirstOrDefaultAsync(s => s.EnergieId == energieId);

            if (seuil == null)
            {
                return NotFound(new
                {
                    message = "Aucun seuil trouvé pour cette énergie."
                });
            }

            seuil.Valeur = updated.Valeur;
            seuil.Periode = updated.Periode;

            await _context.SaveChangesAsync();

            return Ok(seuil);
        }

        // DELETE api/seuil/energie/3
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau")]
        [HttpDelete("energie/{energieId}")]
        public async Task<IActionResult> Delete(int energieId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // Vérification des droits
            if (
                (role == "responsable_electricite" && energieId != 1) ||
                (role == "responsable_gaz" && energieId != 2) ||
                (role == "responsable_eau" && energieId != 3)
               )
            {
                return Forbid("Vous ne pouvez supprimer que votre énergie.");
            }

            var seuil = await _context.Seuils
                .FirstOrDefaultAsync(s => s.EnergieId == energieId);

            if (seuil == null)
            {
                return NotFound(new
                {
                    message = "Aucun seuil trouvé."
                });
            }

            _context.Seuils.Remove(seuil);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Seuil supprimé."
            });
        }
    }
}