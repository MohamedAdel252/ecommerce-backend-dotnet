using ECommerceAPI.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ECommerceAPI.Services.Implementations
{
    public class ResendEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ResendEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var apiKey = _configuration["Resend:ApiKey"];

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
                
            var fromEmail = _configuration["Resend:FromEmail"];
            var emailData = new
            {
                from = fromEmail,
                to = new[] { toEmail },
                subject = subject,
                html = body
            };

            var json = JsonSerializer.Serialize(emailData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.resend.com/emails", content);

            var result = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"Resend Response: {result}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Resend failed: {result}");
            }
        }
    }
}
