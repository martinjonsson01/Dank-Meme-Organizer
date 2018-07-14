using DMO.Services.SettingsServices;
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

        private void FirebaseAuthLink_FirebaseAuthRefreshed(object sender, FirebaseAuthEventArgs e)
        {
            accessToken = e.FirebaseAuth.FirebaseToken;
        }
    }
}
