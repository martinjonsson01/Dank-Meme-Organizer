using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO.GoogleAPI.Models
{
    /// <summary>
    /// Set of detected entity features.
    /// </summary>
    public class EntityAnnotation
    {
        /// <summary>
        /// Image region to which this entity belongs.
        /// </summary>
        public BoundingPoly BoundingPoly { get; set; }

        /// <summary>
        /// Entity textual description, expressed in its <see cref="Locale"/> language.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The language code for the locale in which the entity textual <see cref="Description"/> is expressed.
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// Opaque entity ID. Some IDs may be available in Google Knowledge Graph Search API.
        /// https://developers.google.com/knowledge-graph/
        /// </summary>
        public string Mid { get; set; }

        /// <summary>
        /// Overall score of the result. Range [0, 1].
        /// The accuracy of the entity detection in an image.
        /// For example, for an image in which the "Eiffel Tower" entity is detected,
        /// this field represents the confidence that there is a tower in the query image. Range [0, 1].
        /// </summary>
        public float Score { get; set; }

        /// <summary>
        /// The relevancy of the ICA (Image Content Annotation) label to the image. 
        /// For example, the relevancy of "tower" is likely higher to an image containing the detected "Eiffel Tower" than 
        /// to an image containing a detected distant towering building, 
        /// even though the confidence that there is a tower in each image may be the same. Range [0, 1].
        /// </summary>
        public float Topicality { get; set; }
    }
}
