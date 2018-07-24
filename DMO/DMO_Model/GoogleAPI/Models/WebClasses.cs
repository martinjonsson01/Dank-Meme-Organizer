using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO_Model.GoogleAPI.Models
{
    /// <summary>
    /// Label to provide extra metadata for the web detection.
    /// </summary>
    public class WebLabel
    {
        /// <summary>
        /// For database use.
        /// </summary>
        [Key]
        public Guid ID { get; set; }

        /// <summary>
        /// Label for extra metadata.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The BCP-47 language code for label, such as "en-US" or "sr-Latn". 
        /// For more information, see http://www.unicode.org/reports/tr35/#Unicode_locale_identifier.
        /// </summary>
        public string LangugageCode { get; set; }
    }

    /// <summary>
    /// Metadata for online images.
    /// </summary>
    public class WebImage
    {
        /// <summary>
        /// For database use.
        /// </summary>
        [Key]
        public Guid ID { get; set; }

        /// <summary>
        /// The result image URL.
        /// </summary>
        public string Url { get; set; }
    }

    /// <summary>
    /// Metadata for web pages.
    /// </summary>
    public class WebPage
    {
        /// <summary>
        /// For database use.
        /// </summary>
        [Key]
        public Guid ID { get; set; }

        /// <summary>
        /// Fully matching images on the page. Can include resized copies of the query image.
        /// </summary>
        public virtual List<WebImage> FullMatchingImages { get; set; }

        /// <summary>
        /// Title for the web page, may contain HTML markups.
        /// </summary>
        public string PageTitle { get; set; }

        /// <summary>
        /// Partial matching images on the page. 
        /// Those images are similar enough to share some key-point features.
        /// For example an original image will likely have partial matching for its crops.
        /// </summary>
        public virtual List<WebImage> PartialMatchingImages { get; set; }

        /// <summary>
        /// The result web page URL.
        /// </summary>
        public string Url { get; set; }
    }

    /// <summary>
    /// Entity deduced from similar images on the Internet.
    /// </summary>
    public class WebEntity
    {
        /// <summary>
        /// For database use.
        /// </summary>
        [Key]
        public Guid ID { get; set; }

        /// <summary>
        /// Canonical description of the entity, in English.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Opaque entity ID.
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// Overall relevancy score for the entity. 
        /// Not normalized and not comparable across different image queries.
        /// </summary>
        public float Score { get; set; }
    }
}
