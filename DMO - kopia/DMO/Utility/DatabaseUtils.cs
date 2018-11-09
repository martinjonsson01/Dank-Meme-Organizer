using DMO.Database;
using DMO.Models;
using DMO.Utility.Logging;
using DMO_Model.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                    // Log Exception.
                    LifecycleLog.Exception(e);
                }
                // Update static list.
                App.MediaDatas = new List<MediaData>(mediaDatas);
            }
        }

        public static async Task<List<MediaMetadata>> GetAllMetadatasAsync(this MediaMetaDatabaseContext context)
        {
            List<MediaMetaJson> metaJsons = null;
            using (new DisposableLogger(
                    DatabaseLog.QueryBegin,
                    (sw) => DatabaseLog.QueryEnd(sw, metaJsons?.Count ?? 0)
                ))
            {
                // Get all MediaMetaJsons from database using this LINQ query.
                metaJsons = await context.MediaMetaJsons
                                        .Include(j => j.Labels)
                                        .ToListAsync();
            }

            var metas = new List<MediaMetadata>();
            using (new DisposableLogger(
                    DatabaseLog.DeserializationBegin,
                    (sw) => DatabaseLog.DeserializationEnd(sw, metas?.Count ?? 0)
                ))
            {
                // Deserialize the JSON data in each MediaMetaJson object into a MediaMetadata object and add it to metas.
                foreach (var metaJson in metaJsons)
                {
                    var meta = await Task.Run(() => JsonConvert.DeserializeObject<MediaMetadata>(metaJson.Json));
                    meta.Labels = new ObservableCollection<Label>(metaJson.Labels);
                    metas.Add(meta);
                }
            }

            return metas;
        }

        public static async Task UpdateMediaMetaDataAsync(this MediaMetaDatabaseContext context, MediaMetadata metaToSave)
        {
            try
            {
                // Time and log updating and saving.
                using (new DisposableLogger(DatabaseLog.UpdateSingleBegin, DatabaseLog.UpdateSingleEnd))
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
            }
            catch(Exception e)
            {
                // Log Exception.
                LifecycleLog.Exception(e);
            }
        }

        public static async Task SaveAllMetadatasAsync(this MediaMetaDatabaseContext context, ICollection<MediaMetadata> metas)
        {
            // Time and log serialization.
            using (new DisposableLogger(DatabaseLog.SerializationBegin, (sw) => DatabaseLog.SerializationEnd(sw, metas.Count)))
            {
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
            }
            
            try
            {
                // Time and log saving of MediaMetaJsons.
                using (new DisposableLogger(DatabaseLog.SaveBegin, (sw) => DatabaseLog.SaveEnd(sw, metas.Count)))
                {
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                // Log Exception.
                LifecycleLog.Exception(e);
            }
        }

        public static void DeleteAllMetadatas(this MediaMetaDatabaseContext context)
        {
            var count = context.MediaMetaJsons.Count();
            // Time and log deletion.
            using (new DisposableLogger(DatabaseLog.DeleteBegin, (sw) => DatabaseLog.DeleteEnd(sw, count)))
            {
                context.MediaMetaJsons.RemoveRange(context.MediaMetaJsons);
                context.SaveChanges();
            }
        }
    }
}
