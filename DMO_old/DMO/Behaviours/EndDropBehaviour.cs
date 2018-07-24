using DMO.Models;
using DMO.Utility;
using DMO.ViewModels;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Controls;

namespace DMO.Behaviours
{
    public class EndDropBehavior : Behavior<GridView>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AllowDrop = true;
            AssociatedObject.Drop += AssociatedObject_Drop;
            AssociatedObject.DragOver += AssociatedObject_DragOver;
        }

        private async void AssociatedObject_Drop(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            if (e.DataView != null &&
                e.DataView.Properties != null)
            {
                var def = e.GetDeferral();
                
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    if (sender is GridView grid)
                    {
                        if (grid.DataContext is GalleryPageViewModel vm)
                        {
                            var items = await e.DataView.GetStorageItemsAsync();
                            foreach (var item in items)
                            {
                                if (item is StorageFile file)
                                {
                                    // Check if file type is supported. Using MIME allows for all kinds of videos and images.
                                    if (FileTypes.IsSupportedMIME(file.ContentType))
                                    {
                                        var folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("gallery");
                                        var mediaFile = await file.CopyAsync(folder, file.Name, NameCollisionOption.GenerateUniqueName);
                                        // Add new media file to gallery.
                                        await vm.Gallery.AddFile(vm.TileSize, mediaFile);
                                    }
                                }
                            }
                        }
                    }
                } 

                def.Complete();
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }

        private void AssociatedObject_DragOver(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                // Specify allowed operation.   
                e.AcceptedOperation = DataPackageOperation.Copy;

                // To show the user some information.
                e.DragUIOverride.Caption = "Drop Here to Add to Gallery";
                e.DragUIOverride.IsGlyphVisible = true;
                e.DragUIOverride.IsContentVisible = true;
                e.DragUIOverride.IsCaptionVisible = true;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Drop -= AssociatedObject_Drop;

            AssociatedObject.DragOver -= AssociatedObject_DragOver;
        }
    }
}
