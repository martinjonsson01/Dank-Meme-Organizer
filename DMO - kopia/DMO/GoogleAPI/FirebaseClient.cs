using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DMO.Utility.Logging;
using DMO.Views;
using Firebase.Auth;

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
                if (client == null)
                    client = new FirebaseClient();
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
                // Log Exception.
                LifecycleLog.Exception(e);
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

                using (new DisposableLogger(() => AuthLog.GetAccessTokenBegin("Google"), (sw) => AuthLog.GetAccessTokenEnd(sw, "Google", !string.IsNullOrEmpty(GoogleClient.accessToken))))
                {
                    // Try to fetch google token without prompting the user (authorizing).
                    if (await GoogleClient.Client.GetAccessTokenWithoutAuthentication())
                        googleToken = GoogleClient.accessToken;
                    else // Prompt the user to authorize the application.
                        googleToken = await AuthModal.AuthorizeAndGetGoogleAccessTokenAsync();
                }

                var firebaseSignInSuccess = false;
                using (new DisposableLogger(() => AuthLog.GetAccessTokenBegin("Firebase"), (sw) => AuthLog.GetAccessTokenEnd(sw, "Firebase", firebaseSignInSuccess)))
                {
                    // Try to sign into firebase with the googleToken.
                    firebaseSignInSuccess = await SignInWithFirebaseAsync(googleToken);
                }
                if (!firebaseSignInSuccess)
                {
                    // Could not log into firebase. Could be caused by a refresh token revocation, try re-authenticating with Google.
                    googleToken = await AuthModal.AuthorizeAndGetGoogleAccessTokenAsync();

                    using (new DisposableLogger(() => AuthLog.GetAccessTokenBegin("Firebase"), (sw) => AuthLog.GetAccessTokenEnd(sw, "Firebase", firebaseSignInSuccess)))
                    {
                        // Retry firebase login.
                        firebaseSignInSuccess = await SignInWithFirebaseAsync(googleToken);
                        // Return result.
                        return firebaseSignInSuccess;
                    }
                }

                return firebaseSignInSuccess;
            }
            catch (Exception e)
            {
                // Log Exception.
                LifecycleLog.Exception(e);
                return false;
            }
        }

        private void FirebaseAuthLink_FirebaseAuthRefreshed(object sender, FirebaseAuthEventArgs e)
        {
            AuthLog.AccessTokenRefreshed("Firebase");
            accessToken = e.FirebaseAuth.FirebaseToken;
        }
    }
}
