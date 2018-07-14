using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO.Models
{
    public class Label
    {
        [Key]
        public string Name { get; set; }

        public float Probability { get; set; }
    }
}
