using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO_Model.GoogleAPI.Models
{
    /// <summary>
    /// A vertex represents a 2D point in the image. 
    /// </summary>
    public class Vertex
    {
        /// <summary>
        /// For database use.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Key]
        public Guid VertexID { get; set; }
        
        /// <summary>
        /// X coordinate.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Y coordinate.
        /// </summary>
        public float Y { get; set; }
    }
}
