using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO.GoogleAPI.Models
{
    /// <summary>
    /// TextAnnotation contains a structured representation of OCR extracted text.
    /// The hierarchy of an OCR extracted text structure is like this: TextAnnotation -> Page -> Block -> Paragraph -> Word -> Symbol 
    /// Each structural component, starting from Page, may further have their own properties.
    /// Properties describe detected languages, breaks etc.. 
    /// Please refer to the [TextAnnotation.TextProperty][google.cloud.vision.v1.TextAnnotation.TextProperty] message definition below for more detail.
    /// </summary>
    public class FullTextAnnotation
    {
        /// <summary>
        /// List of pages detected by OCR.
        /// </summary>
        //public List<Page> Pages { get; }

        /// <summary>
        /// UTF-8 text detected on the pages.
        /// </summary>
        public string Text { get; set; }
    }
}
