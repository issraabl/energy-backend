using EnergyTrackerr.Data;
using EnergyTrackerr.Models;
using EnergyTrackerr.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace EnergyTrackerr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MesuresController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AlerteService _alerteService;
        private readonly StatistiqueService _statistiqueService;

        public MesuresController(
            AppDbContext context,
            AlerteService alerteService,
            StatistiqueService statistiqueService)
        {
            _context = context;
            _alerteService = alerteService;
            _statistiqueService = statistiqueService;
        }

        // =====================================================================
        // NORMALISATION : supprime les accents et met en MAJUSCULES
        // "Électricité" → "ELECTRICITE" | "Gazoil" → "GAZOIL"
        // =====================================================================
        private static string Normalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var normalized = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (var c in normalized)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            return sb.ToString().ToUpperInvariant();
        }

        // =====================================================================
        // MAPPING rôle → noms énergie normalisés autorisés (liste pour couvrir
        // les variantes : "Gaz", "Gazoil", "Gaz naturel", etc.)
        // =====================================================================
        private static readonly Dictionary<string, HashSet<string>> RoleEnergieMap = new()
        {
            ["responsable_electricite"] = new() { "ELECTRICITE" },
            ["responsable_gaz"] = new() { "GAZ", "GAZOIL", "GAZ NATUREL", "GAZ DE VILLE" },
            ["responsable_eau"] = new() { "EAU", "EAU POTABLE", "EAU FROIDE", "EAU CHAUDE" },
        };

        // =====================================================================
        // VÉRIFICATION rôle ↔ énergie — avec logs complets
        // =====================================================================
        private async Task<bool> RoleMatchesEnergie(string? role, int energieId)
        {
            Console.WriteLine($"[AUTH] RoleMatchesEnergie → role='{role}' | energieId={energieId}");

            if (string.IsNullOrWhiteSpace(role))
            {
                Console.WriteLine("[AUTH] ECHEC : role est null ou vide");
                return false;
            }

            // Accès total
            if (role == "administrateur" || role == "responsable_energie")
            {
                Console.WriteLine("[AUTH] OK : Acces total (admin/responsable_energie)");
                return true;
            }

            var energie = await _context.Energies.FindAsync(energieId);
            if (energie == null)
            {
                Console.WriteLine($"[AUTH] ECHEC : Aucune energie trouvee pour id={energieId}");
                return false;
            }

            var nomBdd = Normalize(energie.Nom);
            Console.WriteLine($"[AUTH] Energie BDD : brut='{energie.Nom}' | normalise='{nomBdd}'");

            // Cherche le rôle dans la map
            if (!RoleEnergieMap.TryGetValue(role, out var nomsAutorises))
            {
                Console.WriteLine($"[AUTH] ECHEC : Role '{role}' non reconnu dans RoleEnergieMap");
                return false;
            }

            var match = nomsAutorises.Contains(nomBdd);
            Console.WriteLine($"[AUTH] Noms autorises pour '{role}': [{string.Join(", ", nomsAutorises)}]");
            Console.WriteLine($"[AUTH] Comparaison '{nomBdd}' in liste → {(match ? "OK" : "ECHEC")}");

            return match;
        }

        // =====================================================================
        // HELPER : cherche une énergie par nom (insensible aux accents/casse)
        // =====================================================================
        private async Task<Energie?> FindEnergieByNom(string nom)
        {
            var nomNormalized = Normalize(nom);
            Console.WriteLine($"[FIND] Recherche energie : brut='{nom}' | normalise='{nomNormalized}'");

            var energies = await _context.Energies.ToListAsync();

            Console.WriteLine("[FIND] Energies disponibles en BDD :");
            foreach (var e in energies)
                Console.WriteLine($"  id={e.IdEnergie} | brut='{e.Nom}' | normalise='{Normalize(e.Nom)}'");

            var found = energies.FirstOrDefault(e => Normalize(e.Nom) == nomNormalized);
            Console.WriteLine($"[FIND] Resultat : {(found != null ? $"TROUVE id={found.IdEnergie}" : "NON TROUVE")}");

            return found;
        }

        // =====================================================================
        // GET /api/mesures
        // =====================================================================
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MesureReadDto>>> GetMesures([FromQuery] string? energieNom)
        {
            var query = _context.Mesures
                .Include(m => m.Energie)
                .Include(m => m.Equipement)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(energieNom))
            {
                var nomNormalized = Normalize(energieNom);
                var all = await query.ToListAsync();
                all = all.Where(m => Normalize(m.Energie!.Nom) == nomNormalized).ToList();
                return Ok(MapToDtos(all));
            }

            var mesures = await query.ToListAsync();
            return Ok(MapToDtos(mesures));
        }

        // =====================================================================
        // GET /api/mesures/{id}
        // =====================================================================
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpGet("{id}")]
        public async Task<ActionResult<MesureReadDto>> GetMesure(int id)
        {
            var m = await _context.Mesures
                .Include(m => m.Energie)
                .Include(m => m.Equipement)
                .FirstOrDefaultAsync(m => m.IdMesure == id);

            if (m == null) return NotFound();

            return Ok(MapToDto(m));
        }

        // =====================================================================
        // POST /api/mesures
        // =====================================================================
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpPost]
        public async Task<ActionResult> CreateMesure([FromBody] MesureCreateDto dto)
        {
            if (dto == null)
                return BadRequest("DTO invalide.");

            // ── LOGS DE DEBUG ─────────────────────────────────────────────────
            var role = User.FindAll(ClaimTypes.Role).Select(c => c.Value).FirstOrDefault();
            var allClaims = User.Claims.Select(c => $"{c.Type}={c.Value}").ToList();

            Console.WriteLine("==================================================");
            Console.WriteLine("[POST] CreateMesure appele");
            Console.WriteLine($"[POST] EnergieNom recu : '{dto.EnergieNom}'");
            Console.WriteLine($"[POST] Valeur recue    : {dto.Valeur}");
            Console.WriteLine($"[POST] Role extrait    : '{role}'");
            Console.WriteLine($"[POST] Tous les claims : {string.Join(" | ", allClaims)}");
            Console.WriteLine("==================================================");
            // ──────────────────────────────────────────────────────────────────

            if (string.IsNullOrWhiteSpace(dto.EnergieNom))
                return BadRequest("EnergieNom est requis.");

            var energie = await FindEnergieByNom(dto.EnergieNom);

            if (energie == null)
                return BadRequest($"Aucune energie trouvee pour le nom '{dto.EnergieNom}'.");

            if (!await RoleMatchesEnergie(role, energie.IdEnergie))
            {
                Console.WriteLine($"[POST] 403 Forbid → role='{role}' ne peut pas gerer energieId={energie.IdEnergie} ('{energie.Nom}')");
                return Forbid();
            }

            var mesure = new Mesure
            {
                Valeur = dto.Valeur,
                DateMesure = dto.DateMesure,
                DateCreation = DateTime.Now,
                SourceDonnee = dto.SourceDonnee,
                EnergieId = energie.IdEnergie,
                EquipementId = dto.EquipementId
            };

            _context.Mesures.Add(mesure);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[POST] Mesure creee id={mesure.IdMesure}");

            await _alerteService.VerifierMesureEtCreerAlerte(mesure);
            await _statistiqueService.VerifierMesureEtCreerAlerteStatistique(mesure);

            return CreatedAtAction(nameof(GetMesure), new { id = mesure.IdMesure }, mesure);
        }

        // =====================================================================
        // PUT /api/mesures/{id}
        // =====================================================================
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMesure(int id, [FromBody] MesureCreateDto dto)
        {
            if (dto == null)
                return BadRequest("DTO invalide.");

            var role = User.FindAll(ClaimTypes.Role).Select(c => c.Value).FirstOrDefault();

            Console.WriteLine($"[PUT] UpdateMesure id={id} | role='{role}' | EnergieNom='{dto.EnergieNom}'");

            var mesure = await _context.Mesures.FindAsync(id);
            if (mesure == null) return NotFound();

            if (!await RoleMatchesEnergie(role, mesure.EnergieId))
            {
                Console.WriteLine($"[PUT] 403 Forbid sur energieId actuel={mesure.EnergieId}");
                return Forbid();
            }

            if (!string.IsNullOrWhiteSpace(dto.EnergieNom))
            {
                var energie = await FindEnergieByNom(dto.EnergieNom);

                if (energie == null)
                    return BadRequest($"Aucune energie trouvee pour le nom '{dto.EnergieNom}'.");

                if (!await RoleMatchesEnergie(role, energie.IdEnergie))
                {
                    Console.WriteLine($"[PUT] 403 Forbid sur nouvelle energieId={energie.IdEnergie}");
                    return Forbid();
                }

                mesure.EnergieId = energie.IdEnergie;
            }

            mesure.Valeur = dto.Valeur;
            mesure.DateMesure = dto.DateMesure;
            mesure.SourceDonnee = dto.SourceDonnee;
            mesure.EquipementId = dto.EquipementId;

            await _context.SaveChangesAsync();

            Console.WriteLine($"[PUT] Mesure {id} mise a jour");
            return NoContent();
        }

        // =====================================================================
        // DELETE /api/mesures/{id}
        // =====================================================================
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMesure(int id)
        {
            var role = User.FindAll(ClaimTypes.Role).Select(c => c.Value).FirstOrDefault();

            Console.WriteLine($"[DELETE] DeleteMesure id={id} | role='{role}'");

            var mesure = await _context.Mesures.FindAsync(id);
            if (mesure == null) return NotFound();

            if (!await RoleMatchesEnergie(role, mesure.EnergieId))
            {
                Console.WriteLine($"[DELETE] 403 Forbid sur energieId={mesure.EnergieId}");
                return Forbid();
            }

            _context.Mesures.Remove(mesure);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[DELETE] Mesure {id} supprimee");
            return NoContent();
        }

        // =====================================================================
        // MAPPERS PRIVÉS
        // =====================================================================
        private static IEnumerable<MesureReadDto> MapToDtos(IEnumerable<Mesure> mesures)
            => mesures.Select(MapToDto);

        private static MesureReadDto MapToDto(Mesure m) => new MesureReadDto
        {
            IdMesure = m.IdMesure,
            Valeur = m.Valeur,
            DateMesure = m.DateMesure,
            DateCreation = m.DateCreation,
            SourceDonnee = m.SourceDonnee,
            EnergieId = m.EnergieId,
            EquipementId = m.EquipementId,
            Energie = new EnergieDto
            {
                IdEnergie = m.Energie!.IdEnergie,
                Nom = m.Energie.Nom,
                Unite = m.Energie.Unite
            },
            Equipement = m.Equipement == null ? null : new EquipementDto
            {
                IdEquipement = m.Equipement.IdEquipement,
                Nom = m.Equipement.Nom,
                TypeEquipement = m.Equipement.TypeEquipement
            }
        };
    }
}