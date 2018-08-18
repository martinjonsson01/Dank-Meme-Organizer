using DMO.Models;
using DMO.Utility;
using DMO_Model.GoogleAPI.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace DMO.Converters
{
    public class SafeSearchAnnotationToLikelihoodModelListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SafeSearchAnnotation safeSearch)
            {
                var types = new List<LikelihoodModel>
                {
                    new LikelihoodModel
                    {
                        Name ="Adult:",
                        Likelihood = safeSearch.Adult,
                        Color = safeSearch.Adult.GetColor(),
                        Description = "Represents the adult content likelihood for the image. Adult content may contain elements such as nudity, pornographic images or cartoons, or sexual activities."
                    },
                    new LikelihoodModel
                    {
                        Name ="Racy:",
                        Likelihood = safeSearch.Racy,
                        Color = safeSearch.Racy.GetColor(),
                        Description = "Likelihood that the image contains racy content. Racy content may include (but is not limited to) skimpy or sheer clothing, strategically covered nudity, lewd or provocative poses, or close-ups of sensitive body areas."
                    },
                    new LikelihoodModel
                    {
                        Name ="Medical:",
                        Likelihood = safeSearch.Medical,
                        Color = safeSearch.Medical.GetColor(),
                        Description = "Likelihood that this is a medical image."
                    },
                    new LikelihoodModel
                    {
                        Name ="Spoof:",
                        Likelihood = safeSearch.Spoof,
                        Color = safeSearch.Spoof.GetColor(),
                        Description = "Spoof likelihood. The likelihood that a modification was made to the image's canonical version to make it appear funny or offensive."
                    },
                    new LikelihoodModel
                    {
                        Name ="Violence:",
                        Likelihood = safeSearch.Violence,
                        Color = safeSearch.Violence.GetColor(),
                        Description = "Likelihood that this image contains violent content."
                    },
                };
                return types;
            }
            return new[] { new LikelihoodModel { Name = "Likelihood:", Likelihood = Likelihood.UNKNOWN, Color = Likelihood.UNKNOWN.GetColor() } };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
