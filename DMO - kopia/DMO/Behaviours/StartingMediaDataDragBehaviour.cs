using DMO.Models;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            e.Data.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
            if (e.Items != null && e.Items.Any())
            {
                foreach(var item in e.Items)
                {
                    if (item is MediaData data)
                    {
                        e.Data.Properties.Add(data.Title, data.MediaFile);
                    }
                }
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.DragItemsStarting -= AssociatedObject_DragItemsStarting;

        }
    }
}
