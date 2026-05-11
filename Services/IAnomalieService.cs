using EnergyTrackerr.DTOs;
using EnergyTrackerr.Models;

namespace EnergyTrackerr.Services
{
    public interface IAnomalieService
    {
        Task<AnomalieResultatDto> AnalyserMesureAsync(int mesureId, ParametresDetectionDto? parametres = null);
        Task<List<Anomalie>> GetAllAsync();
        Task VerifierEtCreerAlerteAsync(Mesure mesure, int userId);

        Task<RapportAnomaliesDto> AnalyserEquipementAsync(
            int equipementId,
            DateTime? debut = null,
            DateTime? fin = null,
            ParametresDetectionDto? parametres = null);

        Task<RapportAnomaliesDto> AnalyserPeriodeAsync(
            DateTime debut,
            DateTime fin,
            ParametresDetectionDto? parametres = null);

        Task<RapportAnomaliesDto> DetecterEtAlertAsync(
            int userId,
            ParametresDetectionDto? parametres = null);
    }
}