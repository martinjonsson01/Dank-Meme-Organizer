using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO_Model.GoogleAPI.Models
{
    /// <summary>
    /// A bounding polygon for the detected image annotation.
    /// </summary>
    public class BoundingPoly
    {
        /// <summary>
        /// For database use.
        /// </summary>
        public Guid BoundingPolyID { get; set; }

        /// <summary>
        /// The bounding polygon normalized vertices.
        /// NOTE: the normalized vertex coordinates are relative to the original image and range from 0 to 1.
        /// </summary>
        public virtual List<Vertex> NormalizedVertices { get; set; }

        /// <summary>
        /// The bounding polygon vertices.
        /// NOTE: the vertex coordinates are in the same scale as the original image.
        /// </summary>
        public virtual List<Vertex> Vertices { get; set; }
    }
}
