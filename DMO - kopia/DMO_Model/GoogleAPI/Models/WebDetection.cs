using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO_Model.GoogleAPI.Models
{
    /// <summary>
    /// Relevant information for the image from the Internet.
    /// </summary>
    public class WebDetection
    {
        /// <summary>
        /// For database use.
        /// </summary>
        public Guid WebDetectionID { get; set; }

        /// <summary>
        /// For database use.
        /// </summary>
        public Guid AnnotateImageReponseID { get; set; }

        /// <summary>
        /// Best guess text labels for the request image.
        /// </summary>
        public virtual List<WebLabel> BestGuessLabels { get; set; }

        /// <summary>
        /// Fully matching images from the Internet. Can include resized copies of the query image.
        /// </summary>
        public virtual List<WebImage> FullMatchingImages { get; set; }

        /// <summary>
        /// Web pages containing the matching images from the Internet.
        /// </summary>
        public virtual List<WebPage> PagesWithMatchingImages { get; set; }

        /// <summary>
        /// Partial matching images from the Internet. 
        /// Those images are similar enough to share some key-point features. 
        /// For example an original image will likely have partial matching for its crops.
        /// </summary>
        public virtual List<WebImage> PartialMatchingImages { get; set; }

        /// <summary>
        /// The visually similar image results.
        /// </summary>
        public virtual List<WebImage> VisuallySimilarImages { get; set; }

        /// <summary>
        /// Deduced entities from similar images on the Internet.
        /// </summary>
        public virtual List<WebEntity> WebEntities { get; set; }
    }
}
