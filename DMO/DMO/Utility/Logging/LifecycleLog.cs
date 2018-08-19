using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace DMO.Utility.Logging
{
    public class LifecycleLog
    {
        /// <summary>
        /// Logs application start.
        /// </summary>
        public static async void AppStarting()
        {
            // Get app version.
            var v = Package.Current.Id.Version;
            string version = $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";

            try
            {
                var slantFontFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Fonts/slant.flf"));
                using (var stream = await slantFontFile.OpenReadAsync())
                {
                    var slantFont = new WenceyWang.FIGlet.FIGletFont(stream.AsStream());
                    var asciiArt = new WenceyWang.FIGlet.AsciiArt($"Dank Meme Organizer v{version}", slantFont);
                    EventLog.Log.Info($"\n{asciiArt.ToString()}\n");
                }
            }
            catch (Exception)
            {
                EventLog.Log.Info($"Dank Meme Organizer v{version} starting");
            }
        }

        /// <summary>
        /// Logs application prelaunch.
        /// </summary>
        public static void AppPrelaunch()
        {
            EventLog.Log.Info("Application is being prelaunched");
        }

        /// <summary>
        /// Logs application suspension.
        /// </summary>
        public static void AppSuspending()
        {
            EventLog.Log.Info("Application is being suspended");
        }

        /// <summary>
        /// Logs result of extension request.
        /// </summary>
        /// <param name="result">The result to log.</param>
        public static void ExtensionRequestResult(ExtendedExecutionResult result)
        {
            EventLog.Log.Info($"Extension request returned result {result}");
        }

        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="e">The exception to log.</param>
        public static void Exception(Exception e,
                                    [CallerLineNumber] long callerLineNumber = 0,
                                    [CallerMemberName] string callerMember = "")
        {
            var exceptionMessage = e != null ? $"Exception message - \n{e.Message} :: \nInnerException - \n{e.InnerException} ::\n" +
                                               string.Format("StackTrace - \n{0}", e.StackTrace)
                                             : "";
            EventLog.Log.Error($"Method {callerMember}:{callerLineNumber} => \n{exceptionMessage}");
        }
        
        /// <summary>
        /// Logs a navigation exception.
        /// </summary>
        /// <param name="args">The arguments of the exception to log.</param>
        public static void NavigationException(NavigationFailedEventArgs args,
                                              [CallerLineNumber] long callerLineNumber = 0,
                                              [CallerMemberName] string callerMember = "")
        {
            EventLog.Log.Error($"Error occured when navigating from {args.SourcePageType.Name}. Navigation failed.");
            var e = args.Exception;
            var exceptionMessage = e != null ? $"Exception message - \n{e.Message} :: \nInnerException - \n{e.InnerException} ::\n" +
                                               string.Format("StackTrace - \n{0}", e.StackTrace)
                                             : "";
            EventLog.Log.Error($"Method {callerMember}:{callerLineNumber} => {exceptionMessage}");
        }
    }
}
