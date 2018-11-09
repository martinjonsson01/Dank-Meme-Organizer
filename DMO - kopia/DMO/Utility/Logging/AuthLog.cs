using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DMO.Utility.Logging
{
    public class AuthLog
    {
        /// <summary>
        /// Logs access token retrieval.
        /// </summary>
        /// <param name="authProvider">The provider of the authentication token.</param>
        public static void GetAccessTokenBegin(string authProvider)
        {
            //EventLog.Log.Info($"Aquiring {authProvider} token");
        }

        /// <summary>
        /// Logs access token aquired.
        /// </summary>
        /// <param name="authProvider">The provider of the authentication token.</param>
        public static void GetAccessTokenEnd(Stopwatch sw, string authProvider, bool successful)
        {
            var successString = successful ? "successfully" : "unsuccessfully";
            EventLog.Log.Info($"Aquired {authProvider} token {successString}. Took {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Logs access token refresh.
        /// </summary>
        /// <param name="authProvider">The provider of the authentication token.</param>
        public static void AccessTokenRefreshed(string authProvider)
        {
            EventLog.Log.Info($"{authProvider} access token refreshed");
        }

        /// <summary>
        /// Logs an OAuth error.
        /// </summary>
        /// <param name="errorParam">The error parameter of the OAuth response.</param>
        public static void OAuthError(string errorParam,
                                     [CallerLineNumber] long callerLineNumber = 0,
                                     [CallerMemberName] string callerMember = "")
        {
            var exceptionMessage = $"OAuth error: {errorParam}";
            EventLog.Log.Error($"Method {callerMember}:{callerLineNumber} => {exceptionMessage}");
        }

        /// <summary>
        /// Logs a wrong response.
        /// </summary>
        /// <param name="data">The data of the wrong response.</param>
        public static void WrongResponse(string data,
                                        [CallerLineNumber] long callerLineNumber = 0,
                                        [CallerMemberName] string callerMember = "")
        {
            var exceptionMessage = $"Wrong response: {data}";
            EventLog.Log.Error($"Method {callerMember}:{callerLineNumber} => {exceptionMessage}");
        }

        /// <summary>
        /// Logs an invalid state.
        /// </summary>
        /// <param name="state">The invalid state.</param>
        public static void InvalidState(string state,
                                       [CallerLineNumber] long callerLineNumber = 0,
                                       [CallerMemberName] string callerMember = "")
        {
            var exceptionMessage = $"Invalid state: {state}";
            EventLog.Log.Error($"Method {callerMember}:{callerLineNumber} => {exceptionMessage}");
        }

        /// <summary>
        /// Logs an authorization code exhange fail.
        /// </summary>
        public static void CodeExhangeFailed([CallerLineNumber] long callerLineNumber = 0,
                                             [CallerMemberName] string callerMember = "")
        {
            var exceptionMessage = "Authorization code exchange failed";
            EventLog.Log.Error($"Method {callerMember}:{callerLineNumber} => {exceptionMessage}");
        }
    }
}
