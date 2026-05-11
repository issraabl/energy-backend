using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace EnergyTrackerr.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        // ══════════════════════════════════════════════════
        // MÉTHODE SMTP COMMUNE (MailKit)
        // ══════════════════════════════════════════════════
        private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var host = _config["Smtp:Host"]!;
            var port = int.Parse(_config["Smtp:Port"] ?? "587");
            var user = _config["Smtp:User"]!;
            var password = _config["Smtp:Password"]!;
            var fromName = _config["Smtp:FromName"] ?? "TriPower";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, user));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(user, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        // ══════════════════════════════════════════════════
        // EMAIL ALERTE
        // ══════════════════════════════════════════════════
        public async Task EnvoyerEmailAlerteAsync(
            string destinataireEmail,
            string destinataireNom,
            string typeAlerte,
            double valeur,
            double seuil,
            string energieNom)
        {
            var sujet = $"⚠️ Alerte énergie — {typeAlerte} dépassé";

            var corps = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'/>
  <style>
    body {{ font-family: Arial, sans-serif; background: #f4f4f4; margin: 0; padding: 0; }}
    .container {{ max-width: 600px; margin: 30px auto; background: #fff;
                  border-radius: 8px; overflow: hidden;
                  box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
    .header {{ background: #0a1f14; padding: 24px; text-align: center; }}
    .header h1 {{ color: #10b981; margin: 0; font-size: 22px; }}
    .header p  {{ color: #6ee7b7; margin: 4px 0 0; font-size: 13px; }}
    .body {{ padding: 28px; }}
    .alert-box {{ background: #fef2f2; border-left: 4px solid #ef4444;
                  border-radius: 4px; padding: 16px; margin: 20px 0; }}
    .alert-box h2 {{ color: #dc2626; margin: 0 0 8px; font-size: 16px; }}
    .footer {{ background: #f9fafb; padding: 16px 28px;
               font-size: 12px; color: #9ca3af; text-align: center; }}
  </style>
</head>
<body>
  <div class='container'>
    <div class='header'>
      <h1>⚡ TriPower</h1>
      <p>Système de surveillance énergétique</p>
    </div>
    <div class='body'>
      <p>Bonjour <strong>{destinataireNom}</strong>,</p>
      <div class='alert-box'>
        <h2>⚠️ Seuil de consommation dépassé</h2>
        <p>Une consommation anormale a été détectée sur votre système de suivi énergétique.</p>
      </div>
      <table style='width:100%;border-collapse:collapse;margin:20px 0;'>
        <tr style='background:#f3f4f6;'>
          <td style='padding:10px 14px;font-size:13px;color:#6b7280;'>Énergie concernée</td>
          <td style='padding:10px 14px;font-weight:bold;'>{energieNom}</td>
        </tr>
        <tr>
          <td style='padding:10px 14px;font-size:13px;color:#6b7280;'>Valeur mesurée</td>
          <td style='padding:10px 14px;font-weight:bold;color:#ef4444;'>{valeur}</td>
        </tr>
        <tr style='background:#f3f4f6;'>
          <td style='padding:10px 14px;font-size:13px;color:#6b7280;'>Seuil dynamique</td>
          <td style='padding:10px 14px;font-weight:bold;'>{Math.Round(seuil, 2)}</td>
        </tr>
        <tr>
          <td style='padding:10px 14px;font-size:13px;color:#6b7280;'>Type d'alerte</td>
          <td style='padding:10px 14px;font-weight:bold;'>{typeAlerte}</td>
        </tr>
        <tr style='background:#f3f4f6;'>
          <td style='padding:10px 14px;font-size:13px;color:#6b7280;'>Date & heure</td>
          <td style='padding:10px 14px;font-weight:bold;'>{DateTime.Now:dd/MM/yyyy HH:mm:ss}</td>
        </tr>
      </table>
      <p style='color:#6b7280;font-size:13px;'>
        Connectez-vous à votre tableau de bord pour traiter cette alerte et analyser les données.
      </p>
    </div>
    <div class='footer'>
      TriPower · Notification automatique · Ne pas répondre à cet email
    </div>
  </div>
</body>
</html>";

            await SendEmailAsync(destinataireEmail, destinataireNom, sujet, corps);
        }

        // ══════════════════════════════════════════════════
        // EMAIL BIENVENUE
        // ══════════════════════════════════════════════════
        public async Task EnvoyerEmailBienvenueAsync(
            string destinataireEmail,
            string destinataireNom,
            string motDePasse)
        {
            var sujet = "🎉 Bienvenue — Vos identifiants de connexion TriPower";

            var corps = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'/>
  <style>
    body {{ font-family: Arial, sans-serif; background: #f4f4f4; margin: 0; padding: 0; }}
    .container {{ max-width: 600px; margin: 30px auto; background: #fff;
                  border-radius: 8px; overflow: hidden;
                  box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
    .header {{ background: #0a1f14; padding: 24px; text-align: center; }}
    .header h1 {{ color: #10b981; margin: 0; font-size: 22px; }}
    .header p  {{ color: #6ee7b7; margin: 4px 0 0; font-size: 13px; }}
    .body {{ padding: 28px; }}
    .footer {{ background: #f9fafb; padding: 16px 28px;
               font-size: 12px; color: #9ca3af; text-align: center; }}
    .btn {{ display: inline-block; background: #10b981; color: #fff;
            padding: 12px 24px; border-radius: 6px; text-decoration: none;
            font-weight: bold; margin-top: 20px; }}
  </style>
</head>
<body>
  <div class='container'>
    <div class='header'>
      <h1>⚡ TriPower</h1>
      <p>Système de surveillance énergétique</p>
    </div>
    <div class='body'>
      <p>Bonjour <strong>{destinataireNom}</strong>,</p>
      <p>Votre compte a été créé par l'administrateur. Voici vos identifiants de connexion :</p>
      <table style='width:100%;border-collapse:collapse;margin:20px 0;'>
        <tr style='background:#f3f4f6;'>
          <td style='padding:10px 14px;font-size:13px;color:#6b7280;width:140px;'>Email</td>
          <td style='padding:10px 14px;font-weight:bold;'>{destinataireEmail}</td>
        </tr>
        <tr>
          <td style='padding:10px 14px;font-size:13px;color:#6b7280;'>Mot de passe</td>
          <td style='padding:10px 14px;font-weight:bold;color:#10b981;'>{motDePasse}</td>
        </tr>
      </table>
      <div style='text-align:center;'>
        <a class='btn' href='https://votre-site.com/login'>Se connecter →</a>
      </div>
    </div>
    <div class='footer'>
      TriPower · Notification automatique · Ne pas répondre à cet email
    </div>
  </div>
</body>
</html>";

            await SendEmailAsync(destinataireEmail, destinataireNom, sujet, corps);
        }
    }
}