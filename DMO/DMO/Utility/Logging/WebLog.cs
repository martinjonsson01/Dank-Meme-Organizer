using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DMO.Utility.Logging
{
    public class WebLog
    {
        /// <summary>
        /// Logs an HTTP error.
        /// </summary>
        /// <param name="errorCode">The HTTP error code.</param>
        /// <param name="callerLineNumber">The line number of the caller. (Automatically set)</param>
        /// <param name="callerMember">The calling member. (Automatically set)</param>
        public static void HttpError(uint errorCode,
                                    [CallerLineNumber] long callerLineNumber = 0,
                                    [CallerMemberName] string callerMember = "")
        {
            var errorType = (HttpStatusCode)errorCode;
            var exceptionMessage = $"HTTP error code: {errorCode} => {errorType.ToString()}";
            EventLog.Log.Error($"Method {callerMember}:{callerLineNumber} => {exceptionMessage}");
        }

        /// <summary>
        /// Logs the start of getting web dimensions.
        /// </summary>
        public static void GetWebDimensionsBegin()
        {
            //EventLog.Log.Debug("Getting web dimensions without downloading entire image");
        }

        /// <summary>
        /// Logs the end of getting web dimensions.
        /// </summary>
        public static void GetWebDimensionsEnd(Stopwatch sw)
        {
            EventLog.Log.Debug($"Got web dimensions without downloading entire image. Took {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Logs the start of getting online file size.
        /// </summary>
        public static void GetOnlineFileSizeBegin()
        {
            //EventLog.Log.Debug("Getting web dimensions without downloading entire image");
        }

        /// <summary>
        /// Logs the end of getting online file size.
        /// </summary>
        public static void GetOnlineFileSizeEnd(Stopwatch sw)
        {
            EventLog.Log.Debug($"Got online file size without downloading entire file. Took {sw.ElapsedMilliseconds} ms");
        }
    }
}
