using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnergyTrackerr.Models;
using EnergyTrackerr.Data;

namespace EnergyTrackerr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RapportController : ControllerBase
    {
        private readonly AppDbContext _context;
        public RapportController(AppDbContext context) => _context = context;
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz")]
        [HttpGet]
        public async Task<IActionResult> GetRapports()
        {
            var rapports = await _context.Rapports
                .Include(r => r.Analyse)
                .Include(r => r.Utilisateur)
                .ToListAsync();
            return Ok(rapports);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRapport(int id)
        {
            var rapport = await _context.Rapports
                .Include(r => r.Analyse)
                .Include(r => r.Utilisateur)
                .FirstOrDefaultAsync(r => r.IdRapport == id);

            if (rapport == null) return NotFound();
            return Ok(rapport);
        }

        [Authorize(Roles = "administrateur,responsable_energie")]
        [HttpPost]
        public async Task<IActionResult> PostRapport([FromBody] Rapport rapport)
        {
            _context.Rapports.Add(rapport);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRapport), new { id = rapport.IdRapport }, rapport);
        }

        [Authorize(Roles = "administrateur,responsable_energie")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRapport(int id, [FromBody] Rapport rapport)
        {
            if (id != rapport.IdRapport) return BadRequest();
            _context.Entry(rapport).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "administrateur,responsable_energie")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRapport(int id)
        {
            var rapport = await _context.Rapports.FindAsync(id);
            if (rapport == null) return NotFound();
            _context.Rapports.Remove(rapport);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}