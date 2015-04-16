using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TumbleMe.TumblrApi;
using Windows.ApplicationModel.Activation;
#if WINDOWS_PHONE_APP
using Windows.ApplicationModel.Email;
#endif
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TumbleMe
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IWebAuthenticationContinuable
    {
        TumblrHelper tumblr;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.tumblr = (Application.Current as App).Tumblr;
        }        

        private async void SignIn_Click(object sender, RoutedEventArgs e)
        {
            WaitCursor.Visibility = Visibility.Visible;

            await tumblr.StartAuthentication();

            WaitCursor.Visibility = Visibility.Collapsed;

            await ForwardNavOnSignin();
        }

        private async Task ForwardNavOnSignin()
        {
            if (tumblr.SignedIn)
            {
                this.Frame.Navigate(typeof(Posts));
            }
#if WINDOWS_PHONE_APP
            else
            {
                var dialog = new MessageDialog("It looks like you can't post to the Build 2014 tumblr. You can still browse though! Would you like to request permission to post?", "Whoops!");
                IUICommand yes = new UICommand("Yes");
                dialog.Commands.Add(yes);
                IUICommand no = new UICommand("No");
                dialog.Commands.Add(no);
                dialog.CancelCommandIndex = 1;

                IUICommand selected = await dialog.ShowAsync();
                if (selected == yes)
                {
                    EmailMessage accessRequest = new EmailMessage();
                    accessRequest.To.Add(new EmailRecipient("arunjeet.singh@live.com", "Arun"));
                    accessRequest.Subject = "Access to the Build 2014 unofficial tumblr";
                    accessRequest.Body = "Hi! Can I have access to the Build 2014 unofficial tumblr? Thanks!";

                    await EmailManager.ShowComposeNewEmailAsync(accessRequest);
                }
            }
#endif
        }

#if WINDOWS_PHONE_APP
        public async void ContinueWebAuthentication(WebAuthenticationBrokerContinuationEventArgs args)
        {
            WaitCursor.Visibility = Visibility.Visible;

            await tumblr.ContinueAuthentication(args);

            await ForwardNavOnSignin();

            WaitCursor.Visibility = Visibility.Collapsed;
        }
#endif

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Posts));
        }

		private void About_Click(object sender, RoutedEventArgs e)
		{
			this.Frame.Navigate(typeof(AboutPage));
		}
	}
}
