using EnergyTrackerr.Data;
using EnergyTrackerr.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyTrackerr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnergiesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EnergiesController(AppDbContext context)
        {
            _context = context;
        }

        // ---------------------- GET ----------------------
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Energie>>> GetEnergies()
        {
            return await _context.Energies.ToListAsync();
        }

        [HttpGet("electricite")]
        public async Task<ActionResult<IEnumerable<Electricite>>> GetElectricites()
        {
            return await _context.Electricites.ToListAsync();
        }

        [HttpGet("gasoil")]
        public async Task<ActionResult<IEnumerable<Gasoil>>> GetGasoils()
        {
            return await _context.Gasoils.ToListAsync();
        }

        [HttpGet("eau")]
        public async Task<ActionResult<IEnumerable<Eau>>> GetEaux()
        {
            return await _context.Eaux.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Energie>> GetEnergie(int id)
        {
            var en = await _context.Energies.FindAsync(id);
            if (en == null) return NotFound();
            return en;
        }

        // ---------------------- POST ----------------------

        [Authorize(Roles = "administrateur,responsable_energie")]
        [HttpPost("electricite")]
        public async Task<ActionResult<Electricite>> CreateElectricite(Electricite e)
        {
            _context.Energies.Add(e);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetElectricites), new { id = e.IdEnergie }, e);
        }

        [Authorize(Roles = "administrateur,responsable_energie")]
        [HttpPost("gasoil")]
        public async Task<ActionResult<Gasoil>> CreateGasoil(Gasoil g)
        {
            _context.Energies.Add(g);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGasoils), new { id = g.IdEnergie }, g);
        }

        [Authorize(Roles = "administrateur,responsable_energie")]
        [HttpPost("eau")]
        public async Task<ActionResult<Eau>> CreateEau(Eau e)
        {
            _context.Energies.Add(e);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEaux), new { id = e.IdEnergie }, e);
        }

        // ---------------------- PUT ----------------------

        [Authorize(Roles = "administrateur,responsable_energie")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEnergie(int id, Energie e)
        {
            if (id != e.IdEnergie) return BadRequest();

            _context.Entry(e).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Energies.Any(en => en.IdEnergie == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // ---------------------- DELETE ----------------------

        [Authorize(Roles = "administrateur,responsable_energie")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnergie(int id)
        {
            var en = await _context.Energies.FindAsync(id);
            if (en == null) return NotFound();

            _context.Energies.Remove(en);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}