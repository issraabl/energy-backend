using EnergyTrackerr.Data;
using EnergyTrackerr.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyTrackerr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // tous les endpoints nécessitent authentification
    public class ZonesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ZonesController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/Zones – accessible à tous les utilisateurs connectés
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz,responsable_energie")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Zone>>> GetZones()
        {
            return await _context.Zones.ToListAsync();
        }

        // GET api/Zones/{id} – accessible à tous
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz,responsable_energie")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Zone>> GetZone(int id)
        {
            var zone = await _context.Zones.FindAsync(id);
            if (zone == null) return NotFound();
            return zone;
        }

        // POST api/Zones – Admin seulement
        [Authorize(Roles = "administrateur")]
        [HttpPost]
        public async Task<ActionResult<Zone>> CreateZone(Zone zone)
        {
            _context.Zones.Add(zone);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetZone), new { id = zone.IdZone }, zone);
        }

        // PUT api/Zones/{id} – Admin seulement
        [Authorize(Roles = "administrateur")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateZone(int id, Zone zone)
        {
            if (id != zone.IdZone) return BadRequest();
            _context.Entry(zone).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/Zones/{id} – Admin seulement
       [Authorize(Roles = "administrateur")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteZone(int id)
        {
            var zone = await _context.Zones.FindAsync(id);
            if (zone == null) return NotFound();
            _context.Zones.Remove(zone);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}