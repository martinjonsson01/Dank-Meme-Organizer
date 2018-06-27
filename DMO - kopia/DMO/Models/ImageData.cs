using DMO.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;

namespace DMO.Models
{
    public class ImageData : MediaData
    {
        public IList<KeyValuePair<string, float>> Tags = new List<KeyValuePair<string, float>>();

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

            // Order tags by descending loss and remove any that have a probability lower than 10%.
            var tagsAndLoss = output.Loss.OrderByDescending(pair => pair.Value).Where(pair => pair.Value > 0.1f).ToList();
            // If tagsAndLoss contains less than two tags, use the two upper tags.
            if (tagsAndLoss.Count < 2)
            {
                // Order tags by descending loss and take 2.
                tagsAndLoss = output.Loss.OrderByDescending(pair => pair.Value).Take(2).ToList();
            }
            // Update Tags. TODO: Put tags in a persistent database.
            Tags = tagsAndLoss;
        }
    }
}
