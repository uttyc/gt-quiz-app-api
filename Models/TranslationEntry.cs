namespace GoogleTranslateHistoryAPI.Models
{
    public class TranslationEntry
    {
        public int Id { get; set; }
        public string? UserEmail { get; set; }
        public string? SourceText { get; set; }
        public string? TranslatedText { get; set; }
        public string? SourceLang { get; set; } = "en";
        public string? TargetLang { get; set; } = "tr";
        public DateTime Timestamp { get; set; }

        // Quiz-related
        public int CorrectAttempts { get; set; }
        public int WrongAttempts { get; set; }
    }

}
