using DMO.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DMO.Models
{
    public class ImageData : MediaData
    {
        public ImageData(StorageFile file) : base(file)
        {

        }

        public async Task Evaluate(MemeClassifierModel model)
        {
            var input = new MemeClassifierModelInput();

            SoftwareBitmap softwareBitmap;
            using (var stream = await MediaFile.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream 
                var decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file in BGRA8 format
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                // Apply data to input.
                input.Data =  VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
            }

            // Evaluate input...
            var output = await model.EvaluateAsync(input);

            // Order tags by descending loss and take 2.
            var tagsAndLoss = output.Loss.OrderByDescending(pair => pair.Value).Take(2).ToList();

            // Get image properties.
            var imageProperties = await MediaFile.Properties.GetImagePropertiesAsync();
            // Add the two tags to file properties.
            imageProperties.Keywords.Add(tagsAndLoss[0].Key);
            imageProperties.Keywords.Add(tagsAndLoss[1].Key);
        }
    }
}
