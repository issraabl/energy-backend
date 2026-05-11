using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnergyTrackerr.Data;
using EnergyTrackerr.Models;
using EnergyTrackerr.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BCrypt.Net;
var builder = WebApplication.CreateBuilder(args);

// ================= SERVICES =================
builder.Services.AddScoped<NotificationsService>();
builder.Services.AddScoped<AlerteService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<StatistiqueService>();
builder.Services.AddScoped<IAnomalieService, AnomalieService>();
// ================= HTTP CLIENT =================
builder.Services.AddHttpClient<OllamaService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["Ollama:Url"] ?? "http://localhost:11434");
    client.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddHttpClient("ollama", client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});

// ================= CONTROLLERS =================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.MaxDepth = 64;
    });

// ================= DB =================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
);

// ================= JWT =================
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddAuthorization();

// ================= CORS =================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ================= SWAGGER =================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EnergyTrackerr API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

// ================= KESTREL =================
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(6);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(6);
});

var app = builder.Build();

// ================= PIPELINE =================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EnergyTrackerr API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ================= SEED ADMIN =================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();

    // ← AJOUT : corriger les rôles mal orthographiés existants
    var usersACorrige = context.Utilisateurs
        .Where(u => u.Role == "Admin" || u.Role == "admin" || u.Role == "Administrateur")
        .ToList();

    foreach (var u in usersACorrige)
        u.Role = "administrateur";

    if (usersACorrige.Any())
        context.SaveChanges();
    // ← FIN AJOUT

    if (!context.Utilisateurs.Any(u => u.Email == "admin@example.com"))
    {
        context.Utilisateurs.Add(new Utilisateur
        {
            Nom = "Admin",
            Email = "admin@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("admin123"), // ✅ hashé
            Role = "administrateur"
        });

        context.SaveChanges();
    }
}

// ================= OLLAMA WARMUP =================
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        try
        {
            var config = app.Services.GetRequiredService<IConfiguration>();
            var url = $"{config["Ollama:Url"]}/api/generate";
            var model = config["Ollama:Modele"] ?? "llama3";

            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(4) };

            var body = JsonSerializer.Serialize(new
            {
                model,
                prompt = "hi",
                stream = false
            });

            await http.PostAsync(url,
                new StringContent(body, Encoding.UTF8, "application/json"));

            Console.WriteLine("✅ Ollama ready");
        }
        catch (Exception ex)
        {
            Console.WriteLine("⚠️ Ollama error: " + ex.Message);
        }
    });
});

app.Run();