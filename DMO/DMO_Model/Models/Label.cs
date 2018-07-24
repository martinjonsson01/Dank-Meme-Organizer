using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO_Model.Models
{
    public class Label : IEquatable<Label>
    {
        /// <summary>
        /// For database use.
        /// </summary>
        [Key]
        public Guid ID { get; set; }
        
        public string Name { get; set; }

        public float Probability { get; set; }

        public bool Equals(Label other)
        {
            if (other == null) return false;
            if (other is null) return false;

            if (Name == null && other?.Name == null &&
                Probability == other.Probability) return true;
            if (Name == other?.Name && Probability == other?.Probability) return true;
            if (Name.Equals(other?.Name) && NearlyEquals(Probability, other.Probability, 0.01f)) return true;
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as Label);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = (int)2166136261;
                // Suitable nullity checks etc, of course :)
                if (!string.IsNullOrEmpty(Name))
                    hash = (hash * 16777619) ^ Name.GetHashCode();
                hash = (hash * 16777619) ^ Probability.GetHashCode();
                return hash;
            }
        }

        public static bool NearlyEquals(float f1, float f2, float epsilon)
        {
            // Equal if they are within epsilon of each other.
            return Math.Abs(f1 - f2) < epsilon;
        }
    }
}
