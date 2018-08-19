using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO.Utility.Logging
{
    public class NavigationLog
    {
        /// <summary>
        /// Logs page navigation start.
        /// </summary>
        /// <param name="navigatingToName">The name of the page being navigated to.</param>
        public static void NavBegin(string navigatingToName)
        {
            EventLog.Log.Debug($"Navigating to {navigatingToName}");
        }

        /// <summary>
        /// Logs page navigation end.
        /// </summary>
        /// <param name="navigatingToName">The name of the page being navigated to.</param>
        public static void NavEnd(Stopwatch sw, string navigatedToName)
        {
            EventLog.Log.Debug($"Navigated to {navigatedToName}. Took {sw.ElapsedMilliseconds} ms");
        }
    }
}
