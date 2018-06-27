using DMO.Models;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace DMO.Behaviours
{
    public class StartingMediaDataDragBehaviour : Behavior<GridView>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.CanDragItems = true;
            AssociatedObject.DragItemsStarting += AssociatedObject_DragItemsStarting;
        }


        private void AssociatedObject_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items != null && e.Items.Any())
            {
                var files = new List<StorageFile>();
                foreach(var item in e.Items)
                {
                    if (item is MediaData data)
                    {
                        files.Add(data.MediaFile);
                    }
                }
                e.Data.SetStorageItems(files);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.DragItemsStarting -= AssociatedObject_DragItemsStarting;

        }
    }
}
