using Microsoft.AspNetCore.Mvc;

namespace EnergyTrackerr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationsService _notificationsService;

        public NotificationsController(NotificationsService notificationsService)
        {
            _notificationsService = notificationsService;
        }

        public class NotificationDto
        {
            public int IdUtilisateur { get; set; }
            public string Message { get; set; }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreerNotif([FromBody] NotificationDto dto)
        {
            await _notificationsService.CreerNotification(dto.IdUtilisateur, dto.Message);
            return Ok("Notification créée !");
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetNotifs(int userId)
        {
            var notifs = await _notificationsService.GetUserNotifications(userId);
            return Ok(notifs);
        }

        [HttpPut("read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationsService.MarquerCommeLue(id);
            return Ok("Notification marquée comme lue");
        }
    }
}