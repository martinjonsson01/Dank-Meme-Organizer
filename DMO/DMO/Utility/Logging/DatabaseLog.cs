using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DMO.Utility.Logging
{
    public class DatabaseLog
    {
        /// <summary>
        /// Logs start of database loading.
        /// </summary>
        public static void LoadBegin()
        {
            EventLog.Log.Info("Data is being loaded from database");
        }

        /// <summary>
        /// Logs end of database loading.
        /// </summary>
        /// <param name="sw">The stopwatch that timed how long the loading took.</param>
        public static void LoadEnd(Stopwatch sw)
        {
            EventLog.Log.Info($"Data finished loading from database. Took {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Logs start of database query.
        /// </summary>
        public static void QueryBegin()
        {
            //EventLog.Log.Debug("Data is being queried from database");
        }

        /// <summary>
        /// Logs end of database query.
        /// </summary>
        /// <param name="sw">The stopwatch that timed how long the query took.</param>
        /// <param name="queriedObjectsAmount">The amount of objects queried.</param>
        /// <param name="callerMember">The name of the calling memeber. (Automatically assigned)</param>
        public static void QueryEnd(Stopwatch sw, int queriedObjectsAmount, [CallerMemberName] string callerMember = "")
        {
            EventLog.Log.Debug($"{callerMember} quiered {queriedObjectsAmount} objects. Took {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Logs start of serialization.
        /// </summary>
        public static void SerializationBegin()
        {
            //EventLog.Log.Debug("Objects are being serialized");
        }

        /// <summary>
        /// Logs end of serialization.
        /// </summary>
        /// <param name="sw">The stopwatch that timed how long the serialization took.</param>
        /// <param name="serializedObjectsAmount">The amount of objects serialized.</param>
        /// <param name="callerMember">The name of the calling memeber. (Automatically assigned)</param>
        public static void SerializationEnd(Stopwatch sw, int serializedObjectsAmount, [CallerMemberName] string callerMember = "")
        {
            EventLog.Log.Debug($"{callerMember} serialized {serializedObjectsAmount} objects. Took {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Logs start of deserialization.
        /// </summary>
        public static void DeserializationBegin()
        {
            //EventLog.Log.Debug("Objects are being deserialized");
        }

        /// <summary>
        /// Logs end of deserialization.
        /// </summary>
        /// <param name="sw">The stopwatch that timed how long the deserialization took.</param>
        /// <param name="deserializedObjectsAmount">The amount of objects deserialized.</param>
        /// <param name="callerMember">The name of the calling memeber. (Automatically assigned)</param>
        public static void DeserializationEnd(Stopwatch sw, int deserializedObjectsAmount, [CallerMemberName] string callerMember = "")
        {
            EventLog.Log.Debug($"{callerMember} deserialized {deserializedObjectsAmount} objects. Took {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Logs start of <see cref="MediaData"/> creation.
        /// </summary>
        public static void CreateMediaDatasBegin()
        {
            //EventLog.Log.Debug("MediaDatas are being created");
        }

        /// <summary>
        /// Logs end of <see cref="MediaData"/> creation.
        /// </summary>
        public static void CreateMediaDatasEnd(Stopwatch sw)
        {
            EventLog.Log.Debug($"MediaData creation finished. Took {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Logs the beginning of the updating of a single <see cref="MediaMetadata"/>.
        /// </summary>
        public static void UpdateSingleBegin()
        {
            //EventLog.Log.Debug("Updating single MediaMetadata");
        }

        /// <summary>
        /// Logs the end of the updating of a single <see cref="MediaMetadata"/>.
        /// </summary>
        public static void UpdateSingleEnd(Stopwatch sw)
        {
            EventLog.Log.Debug($"Single MediaMetadata updated and saved. Took {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Logs the beginning of saving to database.
        /// </summary>
        public static void SaveBegin()
        {
            //EventLog.Log.Info("Saving MediaMetaJsons to database");
        }

        /// <summary>
        /// Logs the end of saving to database.
        /// </summary>
        /// <param name="savedCount">Amount of items saved.</param>
        public static void SaveEnd(Stopwatch sw, int savedCount)
        {
            EventLog.Log.Info($"Saved {savedCount} MediaMetaJsons. Took {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Logs the beginning of deleting from database.
        /// </summary>
        public static void DeleteBegin()
        {
            //EventLog.Log.Info("Deleting MediaMetaJsons from database");
        }

        /// <summary>
        /// Logs the end of deleting from database.
        /// </summary>
        /// <param name="savedCount">Amount of items deleted.</param>
        public static void DeleteEnd(Stopwatch sw, int savedCount)
        {
            EventLog.Log.Info($"Deleted {savedCount} MediaMetaJsons. Took {sw.ElapsedMilliseconds} ms");
        }
    }
}
