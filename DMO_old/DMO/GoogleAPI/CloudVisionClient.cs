using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace DMO.GoogleAPI
{
    public class CloudVisionClient
    {
        private const string ApiEndpoint = "https://us-central1-dank-meme-organizer.cloudfunctions.net/api/analyzeimage";

        private static CloudVisionClient client = null;
        public static CloudVisionClient Client
        {
            get
            {
                if (client == null) client = new CloudVisionClient();
                return client;
            }
        }

        public async Task<string> SendFirebaseAnalyzeRequest(IRandomAccessStream imageStream, string firebaseToken)
        {
            var imageBase64 = await ImageStreamToBase64(imageStream.AsStreamForRead());

            return await SendFirebaseAnalyzeRequest(imageBase64, firebaseToken);
        }

        public async Task<string> SendFirebaseAnalyzeRequest(string imageBase64, string firebaseToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, ApiEndpoint)
            {
                Content = new StringContent(imageBase64)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", firebaseToken);

            var response = await App.HttpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            // TODO: Handle different HTTP status codes more gracefully.
            return string.Empty;
        }

        public async Task<string> ImageStreamToBase64(Stream imageStream)
        {
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {

                await imageStream.CopyToAsync(memoryStream);
                bytes = memoryStream.ToArray();
            }
            return Convert.ToBase64String(bytes);
        }
    }
}
