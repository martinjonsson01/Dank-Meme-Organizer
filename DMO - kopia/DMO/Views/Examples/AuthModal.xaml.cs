using System.Threading.Tasks;
using DMO.GoogleAPI;
using DMO.Utility.Logging;
using Template10.Common;
using Template10.Controls;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

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
                modal.ModalBackground = new SolidColorBrush(Colors.Black) { Opacity = .5 };
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
            var signInAuthResult = false;
            // Time and log signing in and authentication.
            using (new DisposableLogger(() => AuthLog.GetAccessTokenBegin("Google"), (sw) => AuthLog.GetAccessTokenEnd(sw, "Google", signInAuthResult)))
            {
                signInAuthResult = await GoogleClient.Client.SignInAuthenticate();
            }

            if (signInAuthResult)
            {
                if (_authCompletionSource.TrySetResult(GoogleClient.accessToken))
                    ShowAuth(false);
            }
            SignInButton.IsEnabled = true;
        }

    }
}
