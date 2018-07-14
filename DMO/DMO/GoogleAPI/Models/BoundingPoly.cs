using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO.GoogleAPI.Models
{
    /// <summary>
    /// A bounding polygon for the detected image annotation.
    /// </summary>
    public class BoundingPoly
    {
        /// <summary>
        /// The bounding polygon normalized vertices.
        /// NOTE: the normalized vertex coordinates are relative to the original image and range from 0 to 1.
        /// </summary>
        public List<Vertex> NormalizedVertices { get; set; }

        /// <summary>
        /// The bounding polygon vertices.
        /// NOTE: the vertex coordinates are in the same scale as the original image.
        /// </summary>
        public List<Vertex> Vertices { get; set; }
    }
}
