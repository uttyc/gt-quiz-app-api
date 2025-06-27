using GoogleTranslateHistoryAPI.Data;
using GoogleTranslateHistoryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace GoogleTranslateHistoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuizController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuizController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetRandomQuestion()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null) return Unauthorized();

            var userEntries = await _context.TranslationEntries
                .Where(e => e.UserEmail == email)
                .ToListAsync();

            if (!userEntries.Any()) return NotFound("No translations found.");

            var rnd = new Random();
            var question = userEntries[rnd.Next(userEntries.Count)];

            // Get distractor options (other translations)
            var choices = userEntries
                .Where(e => e.Id != question.Id)
                .OrderBy(_ => Guid.NewGuid())
                .Take(3)
                .Select(e => e.TranslatedText)
                .ToList();

            choices.Add(question.TranslatedText);
            choices = choices.OrderBy(_ => Guid.NewGuid()).ToList();

            return Ok(new
            {
                questionId = question.Id,
                sourceText = question.SourceText,
                choices
            });
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitAnswer([FromBody] QuizAnswerDto dto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null) return Unauthorized();

            var entry = await _context.TranslationEntries
                .FirstOrDefaultAsync(e => e.Id == dto.QuestionId && e.UserEmail == email);

            if (entry == null) return NotFound("Question not found.");

            if (entry.TranslatedText.Trim().ToLower() == dto.Answer.Trim().ToLower())
            {
                entry.CorrectAttempts++;
                await _context.SaveChangesAsync();
                return Ok(new { correct = true });
            }

            entry.WrongAttempts++;
            await _context.SaveChangesAsync();
            return Ok(new { correct = false, correctAnswer = entry.TranslatedText });
        }
        [HttpGet("flashcard")]
        public async Task<IActionResult> GetFlashcard()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null) return Unauthorized();

            var entries = await _context.TranslationEntries
                .Where(e => e.UserEmail == email)
                .ToListAsync();

            if (!entries.Any()) return NotFound("No translations found.");

            var rnd = new Random();
            var word = entries[rnd.Next(entries.Count)];

            return Ok(new
            {
                sourceText = word.SourceText,
                translatedText = word.TranslatedText // You may hide this client-side until revealed
            });
        }

        [HttpGet("fill-in-the-blank")]
        public async Task<IActionResult> GetFillInTheBlank()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null) return Unauthorized();

            var entries = await _context.TranslationEntries
                .Where(e => e.UserEmail == email)
                .ToListAsync();

            if (!entries.Any()) return NotFound("No translations found.");

            var rnd = new Random();
            var word = entries[rnd.Next(entries.Count)];

            return Ok(new
            {
                questionId = word.Id,
                sourceText = word.SourceText
            });
        }

        [HttpPost("fill-in-the-blank/submit")]
        public async Task<IActionResult> SubmitFillInBlank([FromBody] QuizAnswerDto dto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null) return Unauthorized();

            var entry = await _context.TranslationEntries
                .FirstOrDefaultAsync(e => e.Id == dto.QuestionId && e.UserEmail == email);

            if (entry == null) return NotFound("Question not found.");

            bool isCorrect = entry.TranslatedText.Trim().ToLower() == dto.Answer.Trim().ToLower();

            if (isCorrect)
                entry.CorrectAttempts++;
            else
                entry.WrongAttempts++;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                correct = isCorrect,
                correctAnswer = entry.TranslatedText
            });
        }

        [HttpGet("reverse")]
        public async Task<IActionResult> GetReverseQuiz()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null) return Unauthorized();

            var entries = await _context.TranslationEntries
                .Where(e => e.UserEmail == email)
                .ToListAsync();

            if (!entries.Any()) return NotFound("No translations found.");

            var rnd = new Random();
            var question = entries[rnd.Next(entries.Count)];

            var choices = entries
                .Where(e => e.Id != question.Id)
                .OrderBy(_ => Guid.NewGuid())
                .Take(3)
                .Select(e => e.SourceText)
                .ToList();

            choices.Add(question.SourceText);
            choices = choices.OrderBy(_ => Guid.NewGuid()).ToList();

            return Ok(new
            {
                questionId = question.Id,
                translatedText = question.TranslatedText,
                choices
            });
        }


        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null) return Unauthorized();

            var stats = await _context.TranslationEntries
                .Where(e => e.UserEmail == email)
                .GroupBy(e => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Correct = g.Sum(x => x.CorrectAttempts),
                    Wrong = g.Sum(x => x.WrongAttempts)
                })
                .FirstOrDefaultAsync();

            return Ok(stats ?? new { Total = 0, Correct = 0, Wrong = 0 });
        }
    }

    public class QuizAnswerDto
    {
        public int QuestionId { get; set; }
        public string Answer { get; set; }
    }
}