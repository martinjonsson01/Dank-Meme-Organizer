using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO_Model.GoogleAPI.Models
{
    /// <summary>
    /// Stores image properties, such as dominant colors.
    /// </summary>
    public class ImagePropertiesAnnotation
    {
        /// <summary>
        /// For database use.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Key]
        public Guid ImagePropertiesAnnotationID { get; set; }
        
        /// <summary>
        /// If present, dominant colors completed successfully.
        /// </summary>
        public virtual DominantColorsAnnotation DominantColors { get; set; }
    }

    /// <summary>
    /// Set of dominant colors and their corresponding scores.
    /// </summary>
    public class DominantColorsAnnotation
    {
        /// <summary>
        /// For database use.
        /// </summary>
        public Guid DominantColorsAnnotationID { get; set; }

        /// <summary>
        /// RGB color values with their score and pixel fraction.
        /// </summary>
        virtual public List<ColorInfo> Colors { get; set; }
    }

    /// <summary>
    /// Color information consists of RGB channels, score, and the fraction of the image that the color occupies in the image.
    /// </summary>
    public class ColorInfo
    {
        /// <summary>
        /// For database use.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Key]
        public Guid ColorInfoID { get; set; }
        
        /// <summary>
        /// RGB components of the color.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// The fraction of pixels the color occupies in the image. 
        /// Value in range [0, 1].
        /// </summary>
        public float PixelFraction { get; set; }

        /// <summary>
        /// Image-specific score for this color. 
        /// Value in range [0, 1].
        /// </summary>
        public float Score { get; set; }
    }

    /// <summary>
    /// Represents a color in the RGBA color space.
    /// </summary>
    public class Color
    {
        /// <summary>
        /// For database use.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Key]
        public Guid ColorID { get; set; }

        /// <summary>
        /// <para>The fraction of this color that should be applied to the pixel. That is, the final pixel color is defined by the equation:</para>
        /// <para>pixel color = alpha * (this color) + (1.0 - alpha) * (background color)</para>
        /// <para>This means that a value of 1.0 corresponds to a solid color, whereas a value of 0.0 corresponds to a completely transparent color. 
        /// This uses a wrapper message rather than a simple float scalar so that it is possible to distinguish between a default value and the value being unset. 
        /// If omitted, this color object is to be rendered as a solid color (as if the alpha value had been explicitly given with a value of 1.0).</para>
        /// </summary>
        public float? Alpha { get; set; }

        /// <summary>
        /// The amount of blue in the color as a value in the interval [0, 1].
        /// </summary>
        public float Blue { get; set; }

        /// <summary>
        /// The amount of green in the color as a value in the interval [0, 1].
        /// </summary>
        public float Green { get; set; }

        /// <summary>
        /// The amount of red in the color as a value in the interval [0, 1].
        /// </summary>
        public float Red { get; set; }
    }
}
