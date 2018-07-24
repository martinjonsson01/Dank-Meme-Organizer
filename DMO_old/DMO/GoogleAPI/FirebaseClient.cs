using DMO.Services.SettingsServices;
using DMO.Views;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Authentication.Web;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace DMO.GoogleAPI
{

    public class FirebaseClient
    {
        private const string ApiKey = "AIzaSyAGe5nfTmS6TmalsoJg8L3EbYq4szq9CuQ";

        public static string accessToken = string.Empty;

        private FirebaseAuthLink FirebaseAuthLink;
        
        private static FirebaseClient client = null;
        public static FirebaseClient Client
        {
            get
            {
                if (client == null) client = new FirebaseClient();
                return client;
            }
        }
        
        public async Task<bool> SignInWithFirebaseAsync(string googleAccessToken)
        {
            try
            {
                var authProvider = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));

                FirebaseAuthLink = await authProvider.SignInWithOAuthAsync(FirebaseAuthType.Google, googleAccessToken);

                accessToken = FirebaseAuthLink.FirebaseToken;

                FirebaseAuthLink.FirebaseAuthRefreshed += FirebaseAuthLink_FirebaseAuthRefreshed;

                return true;
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
                Debug.Write(e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// This is the go-to method to handle the authorization and sign-in OAuth flow.
        /// It tries to fetch the google auth token from cache, and if it can't it will try to
        /// fetch it using a refresh token, if that's not possible it will prompt the user to 
        /// authorize the application resulting in a google auth token.
        /// It will then use this google auth token to try to sign into firebase, and if that
        /// doesn't work the first time it will retry a second time after having the using re-authorize
        /// the application. (It does this since the refresh token could have been revoked.)
        /// </summary>
        /// <returns><c>true</c> if sign in was successful, <c>false</c> otherwise.</returns>
        public async Task<bool> SignInPromptUserIfNecessary()
        {
            try
            {
                string googleToken;

                // Try to fetch google token without prompting the user (authorizing).
                if (await GoogleClient.Client.GetAccessTokenWithoutAuthentication())
                    googleToken = GoogleClient.accessToken;
                else // Prompt the user to authorize the application.
                    googleToken = await AuthModal.AuthorizeAndGetGoogleAccessTokenAsync();
                Debug.WriteLine($"Google access token successfully aquired: {googleToken}");

                // Try to sign into firebase with the googleToken.
                if (await SignInWithFirebaseAsync(googleToken))
                {
                    Debug.WriteLine($"Firebase access token successfully aquired: {accessToken}");
                    return true;
                }
                else
                {
                    // Could not log into firebase. Could be caused by a refresh token revocation, try re-authenticating with Google.
                    googleToken = await AuthModal.AuthorizeAndGetGoogleAccessTokenAsync();
                    // Retry firebase login.
                    if (await SignInWithFirebaseAsync(googleToken))
                    {
                        Debug.WriteLine($"Firebase access token successfully aquired: {accessToken}");
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
                Debug.Write(e.StackTrace);
                return false;
            }
        }

        private void FirebaseAuthLink_FirebaseAuthRefreshed(object sender, FirebaseAuthEventArgs e)
        {
            Debug.WriteLine("Firebase access token refreshed!");
            accessToken = e.FirebaseAuth.FirebaseToken;
        }
    }
}
