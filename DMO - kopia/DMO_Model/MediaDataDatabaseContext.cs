using DMO_Model.GoogleAPI.Models;
using DMO_Model.Models;
using DMO_Model.Utility;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;

namespace DMO.Database
{
    /// <summary>
    /// This class handles the SQLite database.
    /// </summary>
    public class MediaMetaDatabaseContext : DbContext
    {
        public const string DatabaseFileName = "dankMemesMetadata.db";

        //public DbSet<MediaMetadata> MediaMetaDatas { get; set; }

        public DbSet<MediaMetaJson> MediaMetaJsons { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DatabaseFileName}", x => x.SuppressForeignKeyEnforcement());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
