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
        
        public async Task<List<MediaMetadata>> GetAllMetadatasAsync()
        {
            var sw = new Stopwatch();
            sw.Start();
            // Get all MediaMetaJsons from database using this LINQ query.
            var metaJsons = await MediaMetaJsons
                .Include(j => j.Labels)
                .ToListAsync();
            sw.Stop();
            Debug.WriteLine($"MediaMetadatas queried! Elapsed time: {sw.ElapsedMilliseconds} ms Queried {metaJsons.Count} objects");

            sw.Reset();
            sw.Start();
            var metas = new List<MediaMetadata>();
            // Deserialize the JSON data in each MediaMetaJson object into a MediaMetadata object and add it to metas.
            foreach (var metaJson in metaJsons)
            {
                var meta = await Task.Run(() => JsonConvert.DeserializeObject<MediaMetadata>(metaJson.Json));
                meta.Labels = new ObservableCollection<Label>(metaJson.Labels);
                metas.Add(meta);
            }
            sw.Stop();
            Debug.WriteLine($"MediaMetaJsons deserialized! Elapsed time: {sw.ElapsedMilliseconds} ms Deserialized {metas.Count} objects");

            return metas;
        }

    }
}
