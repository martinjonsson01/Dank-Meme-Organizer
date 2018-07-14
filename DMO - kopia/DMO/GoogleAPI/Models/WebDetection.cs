using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO.GoogleAPI.Models
{
    /// <summary>
    /// Relevant information for the image from the Internet.
    /// </summary>
    public class WebDetection
    {
        /// <summary>
        /// Best guess text labels for the request image.
        /// </summary>
        [JsonProperty("bestGuessLabels")]
        public List<WebLabel> BestGuessLabels { get; set; }

        /// <summary>
        /// Fully matching images from the Internet. Can include resized copies of the query image.
        /// </summary>
        [JsonProperty("fullMatchingImages")]
        public List<WebImage> FullMatchingImages { get; set; }

        /// <summary>
        /// Web pages containing the matching images from the Internet.
        /// </summary>
        [JsonProperty("pagesWithMatchingImages")]
        public List<WebPage> PagesWithMatchingImages { get; set; }

        /// <summary>
        /// Partial matching images from the Internet. 
        /// Those images are similar enough to share some key-point features. 
        /// For example an original image will likely have partial matching for its crops.
        /// </summary>
        [JsonProperty("partialMatchingImages")]
        public List<WebImage> PartialMatchingImages { get; set; }

        /// <summary>
        /// The visually similar image results.
        /// </summary>
        [JsonProperty("visuallySimilarImages")]
        public List<WebImage> VisuallySimilarImages { get; set; }

        /// <summary>
        /// Deduced entities from similar images on the Internet.
        /// </summary>
        [JsonProperty("webEntities")]
        public List<WebEntity> WebEntities { get; set; }
    }
}
