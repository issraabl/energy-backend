using EnergyTrackerr.Data;
using EnergyTrackerr.Models;
using EnergyTrackerr.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UtilisateursController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;

    public UtilisateursController(AppDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // GET ALL USERS
    [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz")]
    [HttpGet]
    public async Task<IActionResult> GetUtilisateurs()
    {
        var users = await _context.Utilisateurs.ToListAsync();

        // hide passwords
        users.ForEach(u => u.Password = null);

        return Ok(users);
    }

    // GET BY ID
    [Authorize(Roles = "administrateur,responsable_electricite,responsable_eau,responsable_gaz")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUtilisateur(int id)
    {
        var user = await _context.Utilisateurs.FindAsync(id);

        if (user == null)
            return NotFound();

        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (User.IsInRole("administrateur") || currentUserId == id.ToString())
        {
            user.Password = null;
            return Ok(user);
        }

        return Forbid();
    }

    // CREATE USER (ADMIN ONLY)
    [Authorize(Roles = "administrateur")]
    [HttpPost]
    public async Task<IActionResult> PostUtilisateur([FromBody] Utilisateur user)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var plainPassword = user.Password;

        // HASH PASSWORD
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

        user.Password = null;

        return CreatedAtAction(nameof(GetUtilisateur),
            new { id = user.IdUtilisateur }, user);
    }

    // UPDATE USER
    [Authorize(Roles = "administrateur")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUtilisateur(int id, [FromBody] Utilisateur user)
    {
        var existing = await _context.Utilisateurs.FindAsync(id);

        if (existing == null)
            return NotFound();

        existing.Nom = user.Nom;
        existing.Email = user.Email;
        existing.Role = user.Role;

        // hash password if updated
        if (!string.IsNullOrEmpty(user.Password))
        {
            existing.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE USER
    [Authorize(Roles = "administrateur")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUtilisateur(int id)
    {
        var user = await _context.Utilisateurs.FindAsync(id);

        if (user == null)
            return NotFound();

        _context.Utilisateurs.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}