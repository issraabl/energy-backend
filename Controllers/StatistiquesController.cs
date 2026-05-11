using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using EnergyTrackerr.Data;
using EnergyTrackerr.Models;
using EnergyTrackerr.DTOs;

namespace EnergyTrackerr.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class StatistiquesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatistiquesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Statistiques/ConsommationTotale
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz")]
        [HttpGet("ConsommationTotale")]
        public async Task<IActionResult> GetConsommationTotale()
        {
            var stats = await _context.Mesures
                .Include(m => m.Energie)
                .GroupBy(m => m.Energie != null ? m.Energie.GetType().Name : "Inconnu")
                .Select(g => new StatistiqueDto
                {
                    EnergieType = g.Key,
                    Total = g.Sum(m => m.Valeur),
                    Moyenne = g.Average(m => m.Valeur),
                    Max = g.Max(m => m.Valeur),
                    Min = g.Min(m => m.Valeur),
                    DateDebut = g.Min(m => m.DateMesure),
                    DateFin = g.Max(m => m.DateMesure)
                })
                .ToListAsync();

            return Ok(stats);
        }

        // GET: api/Statistiques/ParPeriode?start=2026-01-01&end=2026-01-31
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz")]
        [HttpGet("ParPeriode")]
        public async Task<IActionResult> GetParPeriode(DateTime start, DateTime end)
        {
            if (start > end)
                return BadRequest(new { message = "La date de début doit être avant la date de fin" });

            var stats = await _context.Mesures
                .Include(m => m.Energie)
                .Where(m => m.DateMesure >= start && m.DateMesure <= end)
                .GroupBy(m => m.Energie != null ? m.Energie.GetType().Name : "Inconnu")
                .Select(g => new StatistiqueDto
                {
                    EnergieType = g.Key,
                    Total = g.Sum(m => m.Valeur),
                    Moyenne = g.Average(m => m.Valeur),
                    Max = g.Max(m => m.Valeur),
                    Min = g.Min(m => m.Valeur),
                    DateDebut = start,
                    DateFin = end
                })
                .ToListAsync();

            return Ok(stats);
        }

        // GET: api/Statistiques/ParJour?date=2026-01-15
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz")]
        [HttpGet("ParJour")]
        public async Task<IActionResult> GetParJour(DateTime date)
        {
            var stats = await _context.Mesures
                .Include(m => m.Energie)
                .Where(m => m.DateMesure.Date == date.Date)
                .GroupBy(m => m.Energie != null ? m.Energie.GetType().Name : "Inconnu")
                .Select(g => new StatistiqueDto
                {
                    EnergieType = g.Key,
                    Total = g.Sum(m => m.Valeur),
                    Moyenne = g.Average(m => m.Valeur),
                    Max = g.Max(m => m.Valeur),
                    Min = g.Min(m => m.Valeur),
                    DateDebut = date,
                    DateFin = date
                })
                .ToListAsync();

            return Ok(stats);
        }

        // GET: api/Statistiques/ParMois?mois=1&annee=2026
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz")]
        [HttpGet("ParMois")]
        public async Task<IActionResult> GetParMois(int mois, int annee)
        {
            var start = new DateTime(annee, mois, 1);
            var end = new DateTime(annee, mois, DateTime.DaysInMonth(annee, mois));

            var stats = await _context.Mesures
                .Include(m => m.Energie)
                .Where(m => m.DateMesure >= start && m.DateMesure <= end)
                .GroupBy(m => m.Energie != null ? m.Energie.GetType().Name : "Inconnu")
                .Select(g => new StatistiqueDto
                {
                    EnergieType = g.Key,
                    Total = g.Sum(m => m.Valeur),
                    Moyenne = g.Average(m => m.Valeur),
                    Max = g.Max(m => m.Valeur),
                    Min = g.Min(m => m.Valeur),
                    DateDebut = start,
                    DateFin = end
                })
                .ToListAsync();

            return Ok(stats);
        }
    }
}