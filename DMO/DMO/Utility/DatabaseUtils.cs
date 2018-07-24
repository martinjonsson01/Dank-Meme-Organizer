using DMO.Database;
using DMO.Models;
using DMO_Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO.Utility
{
    public static class DatabaseUtils
    {
        public static async Task SaveAllMetadatasAsync(this MediaMetaDatabaseContext context, ICollection<MediaData> mediaDatas)
        {
            var metaDatas = new List<MediaMetadata>();
            foreach (var data in mediaDatas)
                metaDatas.Add(data.Meta);

            await context.SaveAllMetadatasAsync(metaDatas);

            // Update static list.
            App.MediaDatas = new List<MediaData>(mediaDatas);
        }

        public static async Task SaveAllMetadatasAsync(this MediaMetaDatabaseContext context, ICollection<MediaMetadata> metas)
        {
            var sw = new Stopwatch();
            sw.Start();
            var fullList = await context.MediaMetaJsons.Include(m => m.Labels).ToListAsync();
            foreach (var meta in metas)
            {
                // If any MediaMetaJson has a set of labels that are equal to the ones of meta, then remove it so it can be updated.
                var toRemove = fullList.FirstOrDefault(mmj => 
                {
                    return mmj.Labels.ListEquals(meta.Labels.ToList());
                });


                // If there is an existing item to update.
                if (toRemove != null)
                {
                    var metaJson = new MediaMetaJson(meta);
                    toRemove.Json = metaJson.Json;
                }
                else // If a new item needs to be added.
                {
                    meta.DateAdded = DateTime.Now;
                    var metaJson = new MediaMetaJson(meta);
                    context.MediaMetaJsons.Add(metaJson);
                }
            }
            sw.Stop();
            Debug.WriteLine($"Metas serialized and put in list! Elapsed time: {sw.ElapsedMilliseconds} ms Serialized {metas.Count} objects");

            sw.Reset();
            sw.Start();
            await context.SaveChangesAsync();
            sw.Stop();
            Debug.WriteLine($"MediaMetaJsons saved! Elapsed time: {sw.ElapsedMilliseconds} ms Saved {metas.Count} objects");
        }

        public static void DeleteAllMetadatas(this MediaMetaDatabaseContext context)
        {
            var sw = new Stopwatch();
            sw.Start();
            var count = context.MediaMetaJsons.Count();
            context.MediaMetaJsons.RemoveRange(context.MediaMetaJsons);
            context.SaveChanges();
            sw.Stop();
            Debug.WriteLine($"MediaMetaJsons deleted! Elapsed time: {sw.ElapsedMilliseconds} ms Deleted {count} objects");
        }
    }
}
