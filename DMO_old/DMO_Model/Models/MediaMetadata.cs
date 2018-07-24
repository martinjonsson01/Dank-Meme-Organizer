using DMO_Model.GoogleAPI.Models;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DMO_Model.Models
{
    public class MediaMetadata : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <summary>
        /// For database use.
        /// </summary>
        [Key]
        public Guid MediaMetaDataID { get; set; }

        public virtual ObservableCollection<Label> Labels { get; set; }

        public string MediaFilePath { get; set; }

        public virtual AnnotateImageReponse AnnotationData { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        /// <summary>
        /// Date when metadata was added to database.
        /// </summary>
        public DateTime DateAdded { get; set; }

        public DateTime LastModified { get; set; }

        public MediaMetadata()
        {
            
        }

        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanging(string propertyName)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
