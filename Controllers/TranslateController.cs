using GoogleTranslateHistoryAPI.Data;
using GoogleTranslateHistoryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using System.Web;

namespace GoogleTranslateHistoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TranslateController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public TranslateController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        [HttpPost]
        public async Task<IActionResult> TranslateText([FromBody] TranslationRequestDto dto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null) return Unauthorized();

            // Normalize text for consistent matching
            var normalizedSource = dto.SourceText.Trim().ToLower();

            // Check if translation already exists
            var existing = await _context.TranslationEntries.FirstOrDefaultAsync(e =>
                e.UserEmail == email &&
                e.SourceText.ToLower() == normalizedSource &&
                e.SourceLang == dto.SourceLang &&
                e.TargetLang == dto.TargetLang
            );

            if (existing != null)
            {
                return Ok(new TranslationEntry
                {
                    SourceText = existing.SourceText,
                    TranslatedText = existing.TranslatedText,
                    SourceLang = existing.SourceLang,
                    TargetLang = existing.TargetLang,
                });
            }

            // Call Google Translate API
            var apiKey = _configuration["GoogleTranslate:ApiKey"];
            var url = $"https://translation.googleapis.com/language/translate/v2?key={apiKey}";

            var payload = new
            {
                q = normalizedSource,
                source = dto.SourceLang,
                target = dto.TargetLang,
                format = "text"
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Translation API failed.");

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var translated = doc.RootElement
                .GetProperty("data")
                .GetProperty("translations")[0]
                .GetProperty("translatedText")
                .GetString();

            // Save to DB
            var entry = new TranslationEntry
            {
                UserEmail = email,
                SourceText = normalizedSource,
                TranslatedText = translated,
                SourceLang = dto.SourceLang,
                TargetLang = dto.TargetLang,
                Timestamp = DateTime.UtcNow
            };

            _context.TranslationEntries.Add(entry);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                SourceText = dto.SourceText,
                TranslatedText = translated,
                SourceLang = dto.SourceLang,
                TargetLang = dto.TargetLang,
                FromCache = false
            });
        }


        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null) return Unauthorized();

            var entries = await _context.TranslationEntries
                .Where(e => e.UserEmail == email)
                .OrderByDescending(e => e.Timestamp)
                .ToListAsync();

            return Ok(entries);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null) return Unauthorized();

            var entry = await _context.TranslationEntries
                .FirstOrDefaultAsync(e => e.Id == id && e.UserEmail == email);

            if (entry == null) return NotFound();

            _context.TranslationEntries.Remove(entry);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class TranslationRequestDto
    {
        public string SourceText { get; set; }
        public string SourceLang { get; set; } = "en";
        public string TargetLang { get; set; } = "tr";
    }
}
