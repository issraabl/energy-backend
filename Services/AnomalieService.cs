using EnergyTrackerr.Data;
using EnergyTrackerr.Models;
using EnergyTrackerr.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EnergyTrackerr.Services
{
    public class AnomalieService : IAnomalieService
    {
        private readonly AppDbContext _context;

        public AnomalieService(AppDbContext _context)
        {
            this._context = _context;
        }

        public async Task<List<Anomalie>> GetAllAsync()
        {
            return await _context.Anomalies.ToListAsync();
        }

        // Les autres méthodes de l'interface — à compléter
        public Task<AnomalieResultatDto> AnalyserMesureAsync(int mesureId, ParametresDetectionDto? parametres = null)
            => throw new NotImplementedException();

        public Task VerifierEtCreerAlerteAsync(Mesure mesure, int userId)
            => throw new NotImplementedException();

        public Task<RapportAnomaliesDto> AnalyserEquipementAsync(int equipementId, DateTime? debut = null, DateTime? fin = null, ParametresDetectionDto? parametres = null)
            => throw new NotImplementedException();

        public Task<RapportAnomaliesDto> AnalyserPeriodeAsync(DateTime debut, DateTime fin, ParametresDetectionDto? parametres = null)
            => throw new NotImplementedException();

        public Task<RapportAnomaliesDto> DetecterEtAlertAsync(int userId, ParametresDetectionDto? parametres = null)
            => throw new NotImplementedException();
    }
}