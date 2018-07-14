using DMO.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO.Database
{
    /// <summary>
    /// This class handles the SQLite database.
    /// </summary>
    public class MediaDataDatabaseContext : DbContext
    {
        public const string DatabaseFileName = "dankMemes.db";

        public DbSet<MediaData> MediaDatas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DatabaseFileName}");
        }
    }
}
