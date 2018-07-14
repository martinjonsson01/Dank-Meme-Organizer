using DMO.GoogleAPI;
using DMO.Services.SettingsServices;
using DMO.Utility;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Controls;
using Windows.Security.Authentication.Web;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace DMO.Views
{
    public sealed partial class AuthModal : UserControl
    {
        private TaskCompletionSource<string> _authCompletionSource;

        public AuthModal()
        {
            InitializeComponent();
        }
        
        // hide and show busy dialog
        public static void ShowAuth(bool show, TaskCompletionSource<string> taskCompletionSource = null)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modal = Window.Current.Content as ModalDialog;
                if (!(modal.ModalContent is AuthModal view))
                    modal.ModalContent = view = new AuthModal();
                modal.IsModal = show;
                view._authCompletionSource = taskCompletionSource;
            });
        }

        public static async Task<string> AuthorizeAndGetGoogleAccessTokenAsync()
        {
            var authCompletionSource = new TaskCompletionSource<string>();

            ShowAuth(true, authCompletionSource);

            return await authCompletionSource.Task;
        }

        private async void Auth_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SignInButton.IsEnabled = false;
            var sw = new Stopwatch();
            sw.Start();
            if (await GoogleClient.Client.SignInAuthenticate())
            {
                sw.Stop();
                Debug.WriteLine($"Google OAuth completed! Elapsed time: {sw.ElapsedMilliseconds} ms Token aquired: {FirebaseClient.accessToken}");
                
                if (_authCompletionSource.TrySetResult(GoogleClient.accessToken))
                    ShowAuth(false);
            }
            if (sw.IsRunning)
                sw.Stop();
            SignInButton.IsEnabled = true;
        }

    }
}
