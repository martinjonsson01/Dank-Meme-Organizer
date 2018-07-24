using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO_Model.GoogleAPI.Models
{
    /// <summary>
    /// Set of features pertaining to the image, 
    /// computed by computer vision methods over safe-search verticals (for example, adult, spoof, medical, violence).
    /// </summary>
    public class SafeSearchAnnotation
    {
        /// <summary>
        /// For database use.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Key]
        public Guid SafeSearchAnnotationID { get; set; }
        
        /// <summary>
        /// Represents the adult content likelihood for the image. 
        /// Adult content may contain elements such as nudity, pornographic images or cartoons, or sexual activities.
        /// </summary>
        public Likelihood Adult { get; set; }

        /// <summary>
        /// Likelihood that this is a medical image.
        /// </summary>
        public Likelihood Medical { get; set; }

        /// <summary>
        /// Likelihood that the request image contains racy content. 
        /// Racy content may include (but is not limited to) skimpy or sheer clothing, 
        /// strategically covered nudity, lewd or provocative poses, or close-ups of sensitive body areas.
        /// </summary>
        public Likelihood Racy { get; set; }

        /// <summary>
        /// Spoof likelihood. 
        /// The likelihood that an modification was made to the image's canonical version to make it appear funny or offensive.
        /// </summary>
        public Likelihood Spoof { get; set; }

        /// <summary>
        /// Likelihood that this image contains violent content.
        /// </summary>
        public Likelihood Violence { get; set; }
    }

    /// <summary>
    /// A bucketized representation of likelihood, which is intended to give clients highly stable results across model upgrades.
    /// </summary>
    public enum Likelihood
    {
        LIKELY,
        POSSIBLE,
        UNKNOWN,
        UNLIKELY,
        VERY_LIKELY,
        VERY_UNLIKELY
    }
}
