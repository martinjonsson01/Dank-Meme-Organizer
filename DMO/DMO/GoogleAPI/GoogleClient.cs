using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DMO.Utility.Logging;
using Windows.Data.Json;
using Windows.Security.Authentication.Web;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace DMO.GoogleAPI
{

    public class GoogleClient
    {
        private enum TokenTypes { AccessToken, RefreshToken }

        private const string GoogleTokenTime = "GoogleTokenTime";

        private const string ClientID = "1074956536699-oekg255srb707goub2cp8he1r3ecr200.apps.googleusercontent.com";

        private const string RedirectURI = "urn:ietf:wg:oauth:2.0:oob";
        private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/auth";
        private const string ApprovalEndpoint = "https://accounts.google.com/o/oauth2/approval";
        private const string TokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
        private const string UserName = "DankMemeOrganizer";
        public static string accessToken = string.Empty;

        public bool IsAuthorized { get; set; }


        private Lazy<DateTimeOffset> tokenLastAccess = new Lazy<DateTimeOffset>(() =>
        {
            return DateTimeOffset.ParseExact(Settings.ReadOrDefault(GoogleTokenTime, DateTimeOffset.MinValue.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture)),
                                                                       "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        });

        private DateTimeOffset TokenLastAccess
        {
            get => tokenLastAccess.Value;
            set
            {
                tokenLastAccess = new Lazy<DateTimeOffset>(() => value);
                Settings.Save(GoogleTokenTime, value.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture));
            }
        }

        public PasswordVault vault = new PasswordVault();

        private static GoogleClient client = null;
        public static GoogleClient Client
        {
            get
            {
                if (client == null)
                    client = new GoogleClient();
                return client;
            }
        }

        private string GetTokenFromVault(TokenTypes tokenType)
        {
            var token = vault.RetrieveAll().FirstOrDefault((x) => x.Resource == tokenType.ToString());
            if (token != null)
            {
                token.RetrievePassword();
                return token.Password;
            }

            return string.Empty;
        }

        public async Task<bool> GetAccessTokenWithoutAuthentication()
        {
            if (DateTimeOffset.UtcNow < TokenLastAccess.AddSeconds(3600))
            {
                accessToken = GetTokenFromVault(TokenTypes.AccessToken);
                IsAuthorized = true;
                return true;
            }
            else
            {
                var token = GetTokenFromVault(TokenTypes.RefreshToken);
                if (!string.IsNullOrWhiteSpace(token))
                {
                    var content = new StringContent($"refresh_token={token}&client_id={ClientID}&grant_type=refresh_token",
                                                              Encoding.UTF8, "application/x-www-form-urlencoded");

                    var response = await App.HttpClient.PostAsync(TokenEndpoint, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var tokens = JsonObject.Parse(responseString);

                        accessToken = tokens.GetNamedString("access_token");

                        foreach (var item in vault.RetrieveAll().Where((x) => x.Resource == TokenTypes.AccessToken.ToString()))
                            vault.Remove(item);

                        vault.Add(new PasswordCredential(TokenTypes.AccessToken.ToString(), UserName, accessToken));
                        TokenLastAccess = DateTimeOffset.UtcNow;
                        IsAuthorized = true;
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<bool> SignInWithGoogleAsync()
        {
            if (await GetAccessTokenWithoutAuthentication())
                return true;

            return await SignInAuthenticate();
        }

        public async Task<bool> SignInAuthenticate()
        {
            var state = RandomDataBase64(32);
            var code_verifier = RandomDataBase64(32);
            var code_challenge = Base64UrlEncodeNoPadding(Sha256(code_verifier));
            const string code_challenge_method = "S256";

            var authString = "https://accounts.google.com/o/oauth2/auth?client_id=" + ClientID;
            authString += "&scope=profile";
            authString += $"&redirect_uri={RedirectURI}";
            authString += $"&state={state}";
            authString += $"&code_challenge={code_challenge}";
            authString += $"&code_challenge_method={code_challenge_method}";
            authString += "&response_type=code";

            var receivedData = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.UseTitle, new Uri(authString), new Uri(ApprovalEndpoint));

            switch (receivedData.ResponseStatus)
            {
                case WebAuthenticationStatus.Success:
                    await GetAccessToken(receivedData.ResponseData.Substring(receivedData.ResponseData.IndexOf(' ') + 1), state, code_verifier);
                    return true;
                case WebAuthenticationStatus.ErrorHttp:
                    // Log HTTP error.
                    WebLog.HttpError(receivedData.ResponseErrorDetail);
                    return false;

                case WebAuthenticationStatus.UserCancel:
                default:
                    return false;
            }
        }

        private async Task GetAccessToken(string data, string expectedState, string codeVerifier)
        {
            // Parses URI params into a dictionary - ref: http://stackoverflow.com/a/11957114/72176 
            var queryStringParams = data.Split('&').ToDictionary(c => c.Split('=')[0], c => Uri.UnescapeDataString(c.Split('=')[1]));

            if (queryStringParams.ContainsKey("error"))
            {
                // Log error.
                AuthLog.OAuthError(queryStringParams["error"]);
                return;
            }

            if (!queryStringParams.ContainsKey("code") || !queryStringParams.ContainsKey("state"))
            {
                // Log wrong response.
                AuthLog.WrongResponse(data);
                return;
            }

            if (queryStringParams["state"] != expectedState)
            {
                // Log invalid state.
                AuthLog.InvalidState(queryStringParams["state"]);
                return;
            }

            var content = new StringContent($"code={queryStringParams["code"]}&redirect_uri={Uri.EscapeDataString(RedirectURI)}&client_id={ClientID}&code_verifier={codeVerifier}&grant_type=authorization_code",
                                                      Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await App.HttpClient.PostAsync(TokenEndpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Log exchange fail.
                AuthLog.CodeExhangeFailed();
                return;
            }

            var tokens = JsonObject.Parse(responseString);
            accessToken = tokens.GetNamedString("access_token");

            foreach (var item in vault.RetrieveAll().Where((x) => x.Resource == TokenTypes.AccessToken.ToString() || x.Resource == TokenTypes.RefreshToken.ToString()))
                vault.Remove(item);

            vault.Add(new PasswordCredential(TokenTypes.AccessToken.ToString(), UserName, accessToken));
            vault.Add(new PasswordCredential(TokenTypes.RefreshToken.ToString(), UserName, tokens.GetNamedString("refresh_token")));
            TokenLastAccess = DateTimeOffset.UtcNow;
            IsAuthorized = true;
        }

        private string RandomDataBase64(uint length)
        {
            var buffer = CryptographicBuffer.GenerateRandom(length);
            return Base64UrlEncodeNoPadding(buffer);
        }

        private IBuffer Sha256(string inputStirng)
        {
            var sha = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            var buff = CryptographicBuffer.ConvertStringToBinary(inputStirng, BinaryStringEncoding.Utf8);
            return sha.HashData(buff);
        }

        private string Base64UrlEncodeNoPadding(IBuffer buffer)
        {
            var base64 = CryptographicBuffer.EncodeToBase64String(buffer);

            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            base64 = base64.Replace("=", string.Empty);

            return base64;
        }
    }
}
