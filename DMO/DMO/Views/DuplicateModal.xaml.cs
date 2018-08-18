using DMO.Extensions;
using DMO.Models;
using DMO.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Controls;
using Template10.Services.SerializationService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace DMO.Views
{
    public sealed partial class DuplicateModal : UserControl
    {
        public static Queue<List<MediaData>> DuplicateQueue = new Queue<List<MediaData>>();

        public static bool ProcessQueueRunning;

        public DuplicateModal()
        {
            InitializeComponent();
        }

        // hide and show duplicat dialog
        public static void ShowDuplicateModal(bool show, List<MediaData> duplicates, TaskCompletionSource<string> taskCompletionSource = null)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modal = Window.Current.Content as ModalDialog;
                modal.ModalBackground = new SolidColorBrush(Colors.Black) { Opacity = .5 };
                DuplicateModal view = null;
                if (show)
                {
                    modal.ModalContent = view = new DuplicateModal();
                    modal.IsModal = show;
                }
                modal.IsModal = show;

                if (view?.DataContext is DuplicatePageViewModel vm)
                {
                    vm.DuplicateCompletionSource = taskCompletionSource;

                    if (duplicates == null) return;

                    var duplicateEntries = new ObservableCollection<DuplicateMediaEntry>();
                    foreach (var duplicate in duplicates)
                    {
                        var duplicateEntry = new DuplicateMediaEntry(duplicate);
                        duplicateEntries.Add(duplicateEntry);
                    }
                    vm.DuplicateMediaEntries = duplicateEntries;
                }
            });
        }

        public static async Task ProcessQueueAsync()
        {
            if (ProcessQueueRunning)
                return;

            ProcessQueueRunning = true;
            try
            {
                while (DuplicateQueue.TryDequeue(out List<MediaData> duplicates))
                {
                    await HandleDuplicatesAsync(duplicates);
                }
            }
            finally
            {
                ProcessQueueRunning = false;
            }
        }

        private static async Task HandleDuplicatesAsync(List<MediaData> duplicates)
        {
            var authCompletionSource = new TaskCompletionSource<string>();

            ShowDuplicateModal(true, duplicates, authCompletionSource);

            // Wait for user to choose one of the duplicates.
            await authCompletionSource.Task;

            // Close modal.
            ShowDuplicateModal(false, null);
        }
    }
}
