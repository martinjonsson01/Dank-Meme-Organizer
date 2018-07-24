using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DMO_Model.Models
{
    public class MediaMetaJson
    {
        /// <summary>
        /// For database use.
        /// </summary>
        [Key]
        public Guid ID { get; set; }

        public virtual List<Label> Labels { get; set; }

        /// <summary>
        /// A <see cref="MediaMetadata"/> object in JSON format.
        /// </summary>
        public string Json { get; set; }

        public MediaMetaJson()
        {

        }

        public MediaMetaJson(MediaMetadata mediaMetadata)
        {
            Labels = new List<Label>(mediaMetadata.Labels);
            Json = JsonConvert.SerializeObject(mediaMetadata);
        }
    }
}
