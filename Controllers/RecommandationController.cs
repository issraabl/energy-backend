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
    public class RecommandationController : ControllerBase
    {
        private readonly AppDbContext _context;
        public RecommandationController(AppDbContext context) => _context = context;
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz,responsable_energie")]
        [HttpGet]
        public async Task<IActionResult> GetRecommandations()
        {
            var recs = await _context.Recommandations
                .Include(r => r.Alerte)
                .Include(r => r.Analyse)
                .Include(r => r.Utilisateur)
                .ToListAsync();
            return Ok(recs);
        }
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz,responsable_energie")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecommandation(int id)
        {
            var rec = await _context.Recommandations
                .Include(r => r.Alerte)
                .Include(r => r.Analyse)
                .Include(r => r.Utilisateur)
                .FirstOrDefaultAsync(r => r.IdRecommandation == id);

            if (rec == null) return NotFound();
            return Ok(rec);
        }
        [Authorize(Roles = "administrateur")]
        
        [HttpPost]
        public async Task<IActionResult> PostRecommandation([FromBody] Recommandation rec)
        {
            _context.Recommandations.Add(rec);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRecommandation), new { id = rec.IdRecommandation }, rec);
        }

        [Authorize(Roles = "administrateur")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecommandation(int id, [FromBody] Recommandation rec)
        {
            if (id != rec.IdRecommandation) return BadRequest();
            _context.Entry(rec).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "administrateur")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecommandation(int id)
        {
            var rec = await _context.Recommandations.FindAsync(id);
            if (rec == null) return NotFound();
            _context.Recommandations.Remove(rec);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}