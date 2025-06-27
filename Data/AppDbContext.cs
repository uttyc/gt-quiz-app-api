using GoogleTranslateHistoryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GoogleTranslateHistoryAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<TranslationEntry> TranslationEntries { get; set; }
    }
}
