using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnergyTrackerr.Models;
using EnergyTrackerr.Data;

namespace EnergyTrackerr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // tous les endpoints nécessitent une authentification
    public class AnalyseEnergetiqueController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalyseEnergetiqueController(AppDbContext context)
        {
            _context = context;
        }

        // GET : accessible à tous les utilisateurs connectés
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpGet]
        public async Task<IActionResult> GetAnalyses()
        {
            var analyses = await _context.Analyses
                .Include(a => a.Alerte)
                .Include(a => a.Utilisateur)
                .ToListAsync();
            return Ok(analyses);
        }

        // GET par id : accessible à tous
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnalyse(int id)
        {
            var analyse = await _context.Analyses
                .Include(a => a.Alerte)
                .Include(a => a.Utilisateur)
                .FirstOrDefaultAsync(a => a.IdAnalyse == id);

            if (analyse == null) return NotFound();
            return Ok(analyse);
        }

        // POST : uniquement Admin
        [Authorize(Roles = "administrateur,responsable_energie")]
        [HttpPost]
        public async Task<IActionResult> PostAnalyse([FromBody] AnalyseEnergetique analyse)
        {
            _context.Analyses.Add(analyse);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAnalyse), new { id = analyse.IdAnalyse }, analyse);
        }

        // PUT : uniquement Admin
        [Authorize(Roles = "administrateur,responsable_energie")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnalyse(int id, [FromBody] AnalyseEnergetique analyse)
        {
            if (id != analyse.IdAnalyse) return BadRequest();

            _context.Entry(analyse).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE : uniquement Admin
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnalyse(int id)
        {
            var analyse = await _context.Analyses.FindAsync(id);
            if (analyse == null) return NotFound();

            _context.Analyses.Remove(analyse);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}