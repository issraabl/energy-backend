using EnergyTrackerr.Data;
using EnergyTrackerr.Models;
using EnergyTrackerr.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        // ✅ GET — lecture autorisée pour tous les rôles connectés
        // Le filtre par rôle est supprimé du GET pour permettre la consultation
        // cross-énergie (ex: responsable_eau peut lire les données gaz/electricite).
        // Les restrictions d'écriture restent sur POST/PUT/DELETE.
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MesureReadDto>>> GetMesures([FromQuery] int? energieId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var query = _context.Mesures
                .Include(m => m.Energie)
                .Include(m => m.Equipement)
                .AsQueryable();

            // ✅ FIX : on ne filtre PAS par rôle ici.
            // Tous les utilisateurs authentifiés peuvent lire toutes les mesures,
            // mais uniquement via le paramètre energieId pour cibler un vecteur.
            // Si energieId est fourni, on filtre ; sinon on retourne tout
            // (utile pour l'admin et le responsable_energie).
            if (energieId.HasValue)
                query = query.Where(m => m.EnergieId == energieId.Value);

            var mesures = await query.ToListAsync();

            var dtos = mesures.Select(m => new MesureReadDto
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
            }).ToList();

            return Ok(dtos);
        }

        // ✅ GET par ID — lecture autorisée pour tous les rôles connectés
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpGet("{id}")]
        public async Task<ActionResult<MesureReadDto>> GetMesure(int id)
        {
            var m = await _context.Mesures
                .Include(m => m.Energie)
                .Include(m => m.Equipement)
                .FirstOrDefaultAsync(m => m.IdMesure == id);

            if (m == null)
                return NotFound();

            return Ok(new MesureReadDto
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
            });
        }

        // ✅ POST — écriture réservée au responsable de l'énergie concernée + admin
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpPost]
        public async Task<ActionResult> CreateMesure([FromBody] MesureCreateDto dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // Vérifier que le responsable crée uniquement dans son énergie
            if (role == "responsable_electricite" && dto.EnergieId != 1) return Forbid();
            if (role == "responsable_gaz" && dto.EnergieId != 2) return Forbid();
            if (role == "responsable_eau" && dto.EnergieId != 3) return Forbid();

            var energie = await _context.Energies.FindAsync(dto.EnergieId);
            if (energie == null)
                return BadRequest($"Aucune Energie trouvée avec Id = {dto.EnergieId}");

            var mesure = new Mesure
            {
                Valeur = dto.Valeur,
                DateMesure = dto.DateMesure,
                DateCreation = DateTime.Now,
                SourceDonnee = dto.SourceDonnee,
                EnergieId = dto.EnergieId,
                EquipementId = dto.EquipementId
            };

            _context.Mesures.Add(mesure);
            await _context.SaveChangesAsync();

            await _alerteService.VerifierMesureEtCreerAlerte(mesure);
            await _statistiqueService.VerifierMesureEtCreerAlerteStatistique(mesure);

            return CreatedAtAction(nameof(GetMesure), new { id = mesure.IdMesure }, mesure);
        }

        // ✅ PUT — chaque responsable modifie uniquement son énergie
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMesure(int id, [FromBody] MesureCreateDto dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var mesure = await _context.Mesures.FindAsync(id);
            if (mesure == null)
                return NotFound();

            // Vérifier que le responsable modifie uniquement son énergie
            if (role == "responsable_electricite" && mesure.EnergieId != 1) return Forbid();
            if (role == "responsable_gaz" && mesure.EnergieId != 2) return Forbid();
            if (role == "responsable_eau" && mesure.EnergieId != 3) return Forbid();

            // Empêcher de changer l'énergie vers une autre
            if (role == "responsable_electricite" && dto.EnergieId != 1) return Forbid();
            if (role == "responsable_gaz" && dto.EnergieId != 2) return Forbid();
            if (role == "responsable_eau" && dto.EnergieId != 3) return Forbid();

            mesure.Valeur = dto.Valeur;
            mesure.DateMesure = dto.DateMesure;
            mesure.SourceDonnee = dto.SourceDonnee;
            mesure.EnergieId = dto.EnergieId;
            mesure.EquipementId = dto.EquipementId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ DELETE — chaque responsable supprime uniquement son énergie
        [Authorize(Roles = "administrateur,responsable_electricite,responsable_gaz,responsable_eau,responsable_energie")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMesure(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var mesure = await _context.Mesures.FindAsync(id);
            if (mesure == null)
                return NotFound();

            // Vérifier que le responsable supprime uniquement son énergie
            if (role == "responsable_electricite" && mesure.EnergieId != 1) return Forbid();
            if (role == "responsable_gaz" && mesure.EnergieId != 2) return Forbid();
            if (role == "responsable_eau" && mesure.EnergieId != 3) return Forbid();

            _context.Mesures.Remove(mesure);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}