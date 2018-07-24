using DMO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DMO.Services
{
    public class MediaDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ImageTemplate { get; set; }

        public DataTemplate VideoTemplate { get; set; }

        public DataTemplate GifTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            switch (item)
            {
                case ImageData imageData:
                    return ImageTemplate;
                case VideoData videoData:
                    return VideoTemplate;
                case GifData gifData:
                    return GifTemplate;
                default:
                    return VideoTemplate;
            }
        }
    }
}
