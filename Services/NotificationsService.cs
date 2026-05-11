using EnergyTrackerr.Data;
using EnergyTrackerr.Migrations;
using EnergyTrackerr.Models;
using Microsoft.EntityFrameworkCore;

public class NotificationsService
{
    private readonly AppDbContext _context;

    public NotificationsService(AppDbContext context)
    {
        _context = context;
    }

    //  Créer notification
    public async Task CreerNotification(int IdUtilisateur, string message)
    {
        var notification = new Notfication
        {
            IdUtilisateur = IdUtilisateur,
            Message = message,
            IsRead = false,
            DateCreation = DateTime.Now
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    //  Récupérer notifications d’un utilisateur
    public async Task<List<Notfication>> GetUserNotifications(int IdUtilisateur)
    {
        return await _context.Notifications
            .Where(n => n.IdUtilisateur == IdUtilisateur)
            .OrderByDescending(n => n.DateCreation)
            .ToListAsync();
    }

    //  Marquer comme lue
    public async Task MarquerCommeLue(int id)
    {
        var notif = await _context.Notifications.FindAsync(id);
        if (notif != null)
        {
            notif.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }
}
