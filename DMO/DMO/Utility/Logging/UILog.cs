using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace DMO.Utility.Logging
{
    public class UILog
    {
        /// <summary>
        /// Logs bitmap loading start.
        /// </summary>
        public static void BitmapLoadBegin()
        {
            //EventLog.Log.Debug($"Loading bitmap");
        }

        /// <summary>
        /// Logs bitmap loading end.
        /// </summary>
        public static void BitmapLoadEnd(Stopwatch sw)
        {
            EventLog.Log.Debug($"Bitmap loaded. Took {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Logs the result of a request to enable startup task.
        /// </summary>
        /// <param name="newState"></param>
        public static void StartupTaskRequestEnableResult(StartupTaskState newState)
        {
            EventLog.Log.Info($"Request to enable startup resulted in new state: {newState}");
        }
    }
}
