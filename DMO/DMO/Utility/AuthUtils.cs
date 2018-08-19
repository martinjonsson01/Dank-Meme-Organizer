using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;

namespace DMO.Utility
{
    public static class AuthUtils
    {
        /// <summary>
        /// OAuth 2.0 client configuration.
        /// </summary>
        public const string clientID = "1074956536699-oekg255srb707goub2cp8he1r3ecr200.apps.googleusercontent.com";
        public static string redirectURI = "urn:ietf:wg:oauth:2.0:oob";
        public const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        public const string tokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
        public const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";

        public static Uri CreateGoogleAuthorizationRequest()
        {
            // Generates state and PKCE values.
            string state = RandomDataBase64url(32);
            string code_verifier = RandomDataBase64url(32);
            string code_challenge = Base64urlencodeNoPadding(Sha256(code_verifier));
            const string code_challenge_method = "S256";

            // Stores the state and code_verifier values into local settings.
            // Member variables of this class may not be present when the app is resumed with the
            // authorization response, so LocalSettings can be used to persist any needed values.
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["state"] = state;
            localSettings.Values["code_verifier"] = code_verifier;

            // Creates the OAuth 2.0 authorization request.
            string authorizationRequest = string.Format("{0}?response_type=code&scope=openid%20profile&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                authorizationEndpoint,
                Uri.EscapeDataString(redirectURI),
                clientID,
                state,
                code_challenge,
                code_challenge_method);
            
            // Return the Authorization URI.
            return new Uri(authorizationRequest);
        }

        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        public static string RandomDataBase64url(uint length)
        {
            var buffer = CryptographicBuffer.GenerateRandom(length);
            return Base64urlencodeNoPadding(buffer);
        }

        /// <summary>
        /// Returns the SHA256 hash of the input string.
        /// </summary>
        /// <param name="inputStirng"></param>
        /// <returns></returns>
        public static IBuffer Sha256(string inputStirng)
        {
            var sha = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            var buff = CryptographicBuffer.ConvertStringToBinary(inputStirng, BinaryStringEncoding.Utf8);
            return sha.HashData(buff);
        }

        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string Base64urlencodeNoPadding(IBuffer buffer)
        {
            var base64 = CryptographicBuffer.EncodeToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }
    }
}
