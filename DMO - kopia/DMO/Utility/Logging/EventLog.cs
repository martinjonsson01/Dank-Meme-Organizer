using System;
using System.Diagnostics.Tracing;
using System.IO;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel;

namespace DMO.Utility.Logging
{
    public sealed class EventLog : EventSource
    {
        public static EventLog Log = new EventLog();

        #region General logging methods

        /// <summary>
        /// <para>Logs a debug message.</para>
        /// <para>The lowest EventLevel of logging.</para>
        /// </summary>
        /// <param name="message">The message to log.</param>
        [Event(1, Level = EventLevel.Verbose)]
        public void Debug(string message,
                         [CallerFilePath] string callerPath = "")
        {
            GenericLog(1, message, callerPath);
        }

        /// <summary>
        /// <para>Logs an info message.</para>
        /// <para>The second lowest EventLevel of logging.</para>
        /// </summary>
        /// <param name="message">The message to log.</param>
        [Event(2, Level = EventLevel.Informational)]
        public void Info(string message,
                        [CallerFilePath] string callerPath = "")
        {
            GenericLog(2, message, callerPath);
        }

        /// <summary>
        /// <para>Logs a warning message.</para>
        /// <para>The EventLevel of logging in the middle.</para>
        /// </summary>
        /// <param name="message">The message to log.</param>
        [Event(3, Level = EventLevel.Warning)]
        public void Warn(string message,
                        [CallerFilePath] string callerPath = "")
        {
            GenericLog(3, message, callerPath);
        }

        /// <summary>
        /// <para>Logs an error message.</para>
        /// <para>The next highest EventLevel of logging.</para>
        /// </summary>
        /// <param name="message">The message to log.</param>
        [Event(4, Level = EventLevel.Error)]
        public void Error(string message,
                         [CallerFilePath] string callerPath = "")
        {
            GenericLog(4, message, callerPath);
        }

        /// <summary>
        /// <para>Logs a critical message.</para>
        /// <para>The highest EventLevel of logging.</para>
        /// </summary>
        /// <param name="message">The message to log.</param>
        [Event(5, Level = EventLevel.Critical)]
        public void Critical(string message,
                            [CallerFilePath] string callerPath = "")
        {
            GenericLog(5, message, callerPath);
        }
        #endregion

        private void GenericLog(int eventLevel, string message, string callerPath)
        {
            var fileName = Path.GetFileNameWithoutExtension(callerPath);
            string logType;
            switch (fileName)
            {
                case nameof(AuthLog):
                    logType = nameof(AuthLog);
                    break;
                case nameof(DatabaseLog):
                    logType = nameof(DatabaseLog);
                    break;
                case nameof(GalleryLog):
                    logType = nameof(GalleryLog);
                    break;
                case nameof(LifecycleLog):
                    logType = nameof(LifecycleLog);
                    break;
                case nameof(NavigationLog):
                    logType = nameof(NavigationLog);
                    break;
                case nameof(UILog):
                    logType = nameof(UILog);
                    break;
                case nameof(WebLog):
                    logType = nameof(WebLog);
                    break;
                default:
                    logType = "Default";
                    break;
            }
            // Remove the word "Log".
            logType.Replace("Log", string.Empty);

            WriteEvent(eventLevel, message, logType);
        }
    }
}
