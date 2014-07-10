using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TumbleMe.TumblrApi;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TumbleMe
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Posts : Page, IWebAuthenticationContinuable
    {
        TumblrHelper tumblr;
        App currentApp;

        public Posts()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            currentApp = Application.Current as App;
            tumblr = currentApp.Tumblr;

            this.Frame.BackStack.Clear();

            if (tumblr.SignedIn)
            {
                SignInButton.Visibility = Visibility.Collapsed;
                SignOutButton.Visibility = Visibility.Visible;
            }
            else
            {
                SignInButton.Visibility = Visibility.Visible;
                SignOutButton.Visibility = Visibility.Collapsed;
            }

            if (currentApp.PostsViewModel == null)
            {
                currentApp.PostsViewModel = new PostsViewModel();
            }

            WaitCursor.Visibility = Visibility.Visible;
            await currentApp.PostsViewModel.Load();
            WaitCursor.Visibility = Visibility.Collapsed;

            DataContext = currentApp.PostsViewModel;
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.Assert(!tumblr.SignedIn, "The user is logged in. The sign in button should not be available.");

            WaitCursor.Visibility = Visibility.Visible;

            await tumblr.StartAuthentication();

            WaitCursor.Visibility = Visibility.Collapsed;
        }

#if WINDOWS_PHONE_APP
        public async void ContinueWebAuthentication(WebAuthenticationBrokerContinuationEventArgs args)
        {
            WaitCursor.Visibility = Visibility.Visible;

            AuthenticationResult authResult = await tumblr.ContinueAuthentication(args);
            if(tumblr.SignedIn)
            {
                SignInButton.Visibility = Visibility.Collapsed;
                SignOutButton.Visibility = Visibility.Visible;
            }

            WaitCursor.Visibility = Visibility.Collapsed;
        }
#endif

        private void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.Assert(tumblr.SignedIn, "The user is not logged in. The sign out button should not be available.");

            tumblr.SignOut();

            SignInButton.Visibility = Visibility.Visible;
            SignOutButton.Visibility = Visibility.Collapsed;
        }

        private void PostButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PostPhoto));        
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            WaitCursor.Visibility = Visibility.Visible;

            var posts = DataContext as PostsViewModel;
            await posts.Load();

            WaitCursor.Visibility = Visibility.Collapsed;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var postIndex = currentApp.PostsViewModel.Posts.IndexOf(e.ClickedItem as PostViewModel);
            this.Frame.Navigate(typeof(PostDetailPage), postIndex);
        }
    }
}
