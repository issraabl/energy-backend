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

        // ══════════════════════════════════════════════════════════════════════════
        // GET
        // ══════════════════════════════════════════════════════════════════════════

        // GET /api/Energies — retourne TOUTES les énergies (tous types confondus)
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Energie>>> GetEnergies()
        {
            return await _context.Energies.ToListAsync();
        }

        // GET /api/Energies/electricite
        [Authorize(Roles = "administrateur,responsable_electricite")]
        [HttpGet("electricite")]
        public async Task<ActionResult<IEnumerable<Electricite>>> GetElectricites()
        {
            return await _context.Electricites.ToListAsync();
        }

        // GET /api/Energies/gasoil
        [Authorize(Roles = "administrateur,responsable_gaz")]
        [HttpGet("gasoil")]
        public async Task<ActionResult<IEnumerable<Gasoil>>> GetGasoils()
        {
            return await _context.Gasoils.ToListAsync();
        }

        // GET /api/Energies/eau
        [Authorize(Roles = "administrateur,responsable_eau")]
        [HttpGet("eau")]
        public async Task<ActionResult<IEnumerable<Eau>>> GetEaux()
        {
            return await _context.Eaux.ToListAsync();
        }

        // GET /api/Energies/generiques — retourne uniquement les énergies personnalisées
        [Authorize(Roles = "administrateur")]
        [HttpGet("generiques")]
        public async Task<ActionResult<IEnumerable<EnergieGenerique>>> GetEnergiesGeneriques()
        {
            return await _context.EnergiesGeneriques.ToListAsync();
        }

        // GET /api/Energies/byNom/{nom} — recherche par nom (insensible à la casse)
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpGet("byNom/{nom}")]
        public async Task<ActionResult<Energie>> GetEnergieByNom(string nom)
        {
            var en = await _context.Energies
                .FirstOrDefaultAsync(e => e.Nom.ToLower() == nom.ToLower());

            if (en == null)
                return NotFound($"Aucune énergie trouvée avec le nom '{nom}'");

            return en;
        }

        // GET /api/Energies/{id}
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Energie>> GetEnergie(int id)
        {
            var en = await _context.Energies.FindAsync(id);
            if (en == null) return NotFound();
            return en;
        }

        // ══════════════════════════════════════════════════════════════════════════
        // POST
        // ══════════════════════════════════════════════════════════════════════════

        // ✅ POST /api/Energies — créer une énergie personnalisée (tout type de nom)
        // C'est cet endpoint qu'utilise le frontend Angular pour créer n'importe
        // quelle énergie (Solaire, Biomasse, Vapeur, etc.)
        [Authorize(Roles = "administrateur")]
        [HttpPost]
        public async Task<ActionResult<Energie>> CreateEnergie([FromBody] EnergieGeneriqueDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nom))
                return BadRequest(new { message = "Le nom de l'énergie est requis." });

            if (string.IsNullOrWhiteSpace(dto.Unite))
                return BadRequest(new { message = "L'unité est requise." });

            // On vérifie si une énergie avec ce nom existe déjà
            var existe = await _context.Energies
                .AnyAsync(e => e.Nom.ToLower() == dto.Nom.Trim().ToLower());
            if (existe)
                return Conflict(new { message = $"Une énergie nommée '{dto.Nom}' existe déjà." });

            var energie = new EnergieGenerique
            {
                Nom = dto.Nom.Trim(),
                Unite = dto.Unite.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                FacteurConversion = dto.FacteurConversion > 0 ? dto.FacteurConversion : 1,
                Couleur = string.IsNullOrWhiteSpace(dto.Couleur) ? "#6366f1" : dto.Couleur.Trim(),
                DateCreation = DateTime.Now
            };

            _context.Energies.Add(energie);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEnergie), new { id = energie.IdEnergie }, energie);
        }

        // POST /api/Energies/electricite — créer une électricité (route spécifique existante)
        [Authorize(Roles = "administrateur")]
        [HttpPost("electricite")]
        public async Task<ActionResult<Electricite>> CreateElectricite(Electricite e)
        {
            e.IdEnergie = 0;
            e.DateCreation = DateTime.Now;
            _context.Energies.Add(e);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetElectricites), new { id = e.IdEnergie }, e);
        }

        // POST /api/Energies/gasoil
        [Authorize(Roles = "administrateur")]
        [HttpPost("gasoil")]
        public async Task<ActionResult<Gasoil>> CreateGasoil(Gasoil g)
        {
            g.IdEnergie = 0;
            g.DateCreation = DateTime.Now;
            _context.Energies.Add(g);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGasoils), new { id = g.IdEnergie }, g);
        }

        // POST /api/Energies/eau
        [Authorize(Roles = "administrateur")]
        [HttpPost("eau")]
        public async Task<ActionResult<Eau>> CreateEau(Eau e)
        {
            e.IdEnergie = 0;
            e.DateCreation = DateTime.Now;
            _context.Energies.Add(e);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEaux), new { id = e.IdEnergie }, e);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // PUT
        // ══════════════════════════════════════════════════════════════════════════

        // PUT /api/Energies/{id} — mettre à jour n'importe quelle énergie
        [Authorize(Roles = "administrateur")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEnergie(int id, [FromBody] EnergieGeneriqueDto dto)
        {
            var energie = await _context.Energies.FindAsync(id);
            if (energie == null) return NotFound(new { message = $"Énergie #{id} introuvable." });

            // Vérifier doublon de nom (en excluant l'énergie courante)
            var doublon = await _context.Energies
                .AnyAsync(e => e.Nom.ToLower() == dto.Nom.Trim().ToLower() && e.IdEnergie != id);
            if (doublon)
                return Conflict(new { message = $"Une autre énergie nommée '{dto.Nom}' existe déjà." });

            // Mise à jour des champs communs uniquement (ne touche pas aux champs
            // spécifiques comme Tension, Volume, Debit pour ne pas casser les sous-classes)
            energie.Nom = dto.Nom.Trim();
            energie.Unite = dto.Unite.Trim();
            energie.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            energie.FacteurConversion = dto.FacteurConversion > 0 ? dto.FacteurConversion : 1;
            energie.Couleur = string.IsNullOrWhiteSpace(dto.Couleur) ? "#6366f1" : dto.Couleur.Trim();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Energies.Any(e => e.IdEnergie == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // ══════════════════════════════════════════════════════════════════════════
        // DELETE
        // ══════════════════════════════════════════════════════════════════════════

        // DELETE /api/Energies/{id}
        [Authorize(Roles = "administrateur")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnergie(int id)
        {
            var en = await _context.Energies.FindAsync(id);
            if (en == null) return NotFound(new { message = $"Énergie #{id} introuvable." });

            // Vérifier si des mesures sont liées avant de supprimer
            var mesuresLiees = await _context.Mesures.AnyAsync(m => m.EnergieId == id);
            if (mesuresLiees)
                return Conflict(new { message = "Impossible de supprimer : des mesures sont liées à cette énergie." });

            _context.Energies.Remove(en);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // DTO — Objet de transfert pour POST et PUT
    // Évite les problèmes de désérialisation avec les sous-classes (polymorphisme)
    // ══════════════════════════════════════════════════════════════════════════════
    public class EnergieGeneriqueDto
    {
        public string Nom { get; set; } = "";
        public string Unite { get; set; } = "";
        public string? Description { get; set; }
        public double FacteurConversion { get; set; } = 1;
        public string? Couleur { get; set; } = "#6366f1";
    }
}