using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO.GoogleAPI.Models
{
    /// <summary>
    /// Response to an image annotation request.
    /// </summary>
    public class AnnotateImageReponse
    {
        /// <summary>
        /// If present, text (OCR) detection has completed successfully.
        /// </summary>
        public List<EntityAnnotation> TextAnnotations { get; set; }

        /// <summary>
        /// If present, safe-search annotation has completed successfully.
        /// </summary>
        public SafeSearchAnnotation SafeSearchAnnotation { get; set; }

        /// <summary>
        /// If present, image properties were extracted successfully.
        /// </summary>
        public ImagePropertiesAnnotation ImagePropertiesAnnotation { get; set; }

        /// <summary>
        /// If set, represents the error message for the operation.
        /// Note that filled-in image annotations are guaranteed to be correct, even when error is set.
        /// </summary>
        public Status Error { get; set; }

        /// <summary>
        /// If present, text (OCR) detection or document (OCR) text detection has completed successfully.
        /// This annotation provides the structural hierarchy for the OCR detected text.
        /// </summary>
        public FullTextAnnotation FullTextAnnotation { get; set; }

        /// <summary>
        /// If present, web detection has completed successfully.
        /// </summary>
        [JsonProperty("webDetection")]
        public WebDetection WebDetection { get; set; }
    }
}
