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
        public static async Task SaveAllMetadatasAsync(ICollection<MediaData> mediaDatas)
        {
            using (var context = new MediaMetaDatabaseContext())
            {
                var metaDatas = new List<MediaMetadata>();
                foreach (var data in mediaDatas)
                    metaDatas.Add(data.Meta);
                try
                {
                    await context.SaveAllMetadatasAsync(metaDatas);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Could not save all media datas in database because of exception:");
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);
                }
                // Update static list.
                App.MediaDatas = new List<MediaData>(mediaDatas);
            }
        }

        public static async Task UpdateMediaMetaDataAsync(this MediaMetaDatabaseContext context, MediaMetadata metaToSave)
        {
            var sw = new Stopwatch();
            sw.Start();

            try
            {
                // Find MediaMetaJson in database list.
                var metaJson = context.MediaMetaJsons.Include(m => m.Labels).SingleOrDefault(mj => mj.Labels.ListEquals(metaToSave.Labels));

                if (metaJson != null)
                {
                    var newMetaJson = new MediaMetaJson(metaToSave);
                    metaJson.Json = newMetaJson.Json;

                    await context.SaveChangesAsync();
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine("Could not update media data in database because of exception:");
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
            finally
            {
                sw.Stop();
                Debug.WriteLine($"Single MediaMetadata updated! Elapsed time: {sw.ElapsedMilliseconds} ms Saved 1 object");
            }
        }

        public static async Task SaveAllMetadatasAsync(this MediaMetaDatabaseContext context, ICollection<MediaMetadata> metas)
        {
            var sw = new Stopwatch();
            sw.Start();
            var fullList = await context.MediaMetaJsons.Include(m => m.Labels).ToListAsync();
            foreach (var meta in metas)
            {
                // If any MediaMetaJson has a set of labels that are equal to the ones of meta, then update it.
                var toUpdate = fullList.FirstOrDefault(mmj => 
                {
                    return mmj.Labels.ListEquals(meta.Labels);
                });


                // If there is an existing item to update.
                if (toUpdate != null)
                {
                    var metaJson = await MediaMetaJson.FromMediaMetaAsync(meta);
                    toUpdate.Json = metaJson.Json;
                }
                else // If a new item needs to be added.
                {
                    meta.DateAdded = DateTime.Now;
                    var metaJson = await MediaMetaJson.FromMediaMetaAsync(meta);
                    context.MediaMetaJsons.Add(metaJson);
                }
            }
            sw.Stop();
            Debug.WriteLine($"Metas serialized and put in list! Elapsed time: {sw.ElapsedMilliseconds} ms Serialized {metas.Count} objects");

            sw.Reset();
            sw.Start();
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not save changes to database because of exception:");
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
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
