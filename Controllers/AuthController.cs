using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EnergyTrackerr.Data;
using EnergyTrackerr.Models;
using EnergyTrackerr.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace EnergyTrackerr.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public AuthController(AppDbContext context, IConfiguration config, EmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        private static readonly ConcurrentDictionary<string, ResetEntry> _resetCodes = new();

        // ================= REGISTER =================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Utilisateur user)
        {
            if (await _context.Utilisateurs.AnyAsync(u => u.Email == user.Email))
                return BadRequest(new { message = "Email déjà utilisé." });

            // HASH PASSWORD
            var plainPassword = user.Password;
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            _context.Utilisateurs.Add(user);
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.EnvoyerEmailBienvenueAsync(
                    user.Email,
                    user.Nom,
                    plainPassword
                );
            }
            catch { }

            return Ok(new { message = "Utilisateur créé avec succès." });
        }

        // ================= LOGIN =================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var user = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == login.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
                return Unauthorized(new { message = "Email ou mot de passe incorrect." });

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdUtilisateur.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToLower().Trim())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                role = user.Role.ToLower().Trim(),
                email = user.Email,
                nom = user.Nom
            });
        }

        // ================= FORGOT PASSWORD =================
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            var user = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == req.Email);

            if (user == null)
                return NotFound(new { message = "Aucun compte trouvé." });

            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            _resetCodes[req.Email.ToLower()] = new ResetEntry
            {
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                Verified = false
            };

            await SendResetEmailAsync(req.Email, code);

            return Ok(new { message = "Code envoyé." });
        }

        // ================= VERIFY CODE =================
        [HttpPost("verify-reset-code")]
        public IActionResult VerifyResetCode([FromBody] VerifyCodeRequest req)
        {
            if (!_resetCodes.TryGetValue(req.Email.ToLower(), out var entry))
                return BadRequest(new { message = "Code invalide." });

            if (entry.Code != req.Code || DateTime.UtcNow > entry.ExpiresAt)
                return BadRequest(new { message = "Code incorrect ou expiré." });

            entry.Verified = true;
            return Ok(new { message = "Code validé." });
        }

        // ================= RESET PASSWORD =================
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            if (!_resetCodes.TryGetValue(req.Email.ToLower(), out var entry))
                return BadRequest(new { message = "Session invalide." });

            if (!entry.Verified)
                return BadRequest(new { message = "Code non vérifié." });

            var user = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == req.Email);

            if (user == null)
                return NotFound();

            // HASH NEW PASSWORD
            user.Password = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);

            await _context.SaveChangesAsync();

            _resetCodes.TryRemove(req.Email.ToLower(), out _);

            return Ok(new { message = "Mot de passe mis à jour." });
        }

        // ================= SMTP =================
        private async Task SendResetEmailAsync(string toEmail, string code)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("EnergyTrackerr", _config["Smtp:User"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Reset Password Code";

            message.Body = new TextPart("html")
            {
                Text = $"<h3>Code: {code}</h3>"
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_config["Smtp:Host"], 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config["Smtp:User"], _config["Smtp:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        // ================= DTOs =================
        public class LoginDto { public string Email { get; set; } = ""; public string Password { get; set; } = ""; }
        public class ForgotPasswordRequest { public string Email { get; set; } = ""; }
        public class VerifyCodeRequest { public string Email { get; set; } = ""; public string Code { get; set; } = ""; }
        public class ResetPasswordRequest { public string Email { get; set; } = ""; public string Code { get; set; } = ""; public string NewPassword { get; set; } = ""; }
        public class ResetEntry { public string Code { get; set; } = ""; public DateTime ExpiresAt { get; set; } public bool Verified { get; set; } }
    }
}