using EnergyTrackerr.Data;
using EnergyTrackerr.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyTrackerr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SitesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SitesController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Site>>> GetSites()
        {
            return await _context.Sites.ToListAsync();
        }

        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Site>> GetSite(int id)
        {
            var site = await _context.Sites.FindAsync(id);
            if (site == null) return NotFound();
            return site;
        }

        [Authorize(Roles = "administrateur")]
        [HttpPost]
        public async Task<ActionResult<Site>> CreateSite([FromBody] Site site)
        {
            site.IdSite = 0; // laisser la DB générer l'ID
            _context.Sites.Add(site);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSite), new { id = site.IdSite }, site);
        }

        [Authorize(Roles = "administrateur")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSite(int id, [FromBody] Site site)
        {
            if (id != site.IdSite) return BadRequest("ID mismatch");

            var existing = await _context.Sites.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = site.Name;
            existing.Address = site.Address;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "administrateur")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSite(int id)
        {
            var site = await _context.Sites.FindAsync(id);
            if (site == null) return NotFound();
            _context.Sites.Remove(site);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}