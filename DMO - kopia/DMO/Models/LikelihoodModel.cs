using DMO_Model.GoogleAPI.Models;

namespace DMO.Models
{
    public class LikelihoodModel
    {
        public Likelihood Likelihood { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Windows.UI.Color Color { get; set; }
    }
}
