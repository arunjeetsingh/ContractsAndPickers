using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Text;
using TumbleMe.Common;
using TumbleMe.TumblrApi;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
using Windows.ApplicationModel.Email;
#endif
// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace TumbleMe
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        TumblrHelper _Tumblr;
        public TumblrHelper Tumblr
        {
            get
            { 
                if(_Tumblr == null)
                {
                    _Tumblr = new TumblrHelper();
                }

                return _Tumblr;
            }
        }

        PostsViewModel _PostsViewModel;
        public PostsViewModel PostsViewModel 
        {
            get
            {
                if (_PostsViewModel == null && SuspensionManager.SessionState.ContainsKey("Posts"))
                {
                    _PostsViewModel = Deserialize<PostsViewModel>(SuspensionManager.SessionState["Posts"] as string);
                }

                return _PostsViewModel;
            }
            set
            {
                _PostsViewModel = value;
            }
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
            this.UnhandledException += App_UnhandledException;

#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += this.HardwareButtons_BackPressed;
#endif
        }

        async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;

#if WINDOWS_PHONE_APP
            var dialog = new MessageDialog("Looks like an unhandled exception just happened. We didn't expect that! :( Say Yes if you'd like to send us a technical report on what happened.", "Oops!");
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
                accessRequest.Subject = "Exception in the Build 2014 Tumblr app";
                accessRequest.Body = e.Exception.ToString();

                await EmailManager.ShowComposeNewEmailAsync(accessRequest);
            }
#else
            var dialog = new MessageDialog("Looks like an unhandled exception just happened. We didn't expect that! :( Say Yes if you'd like to send us a technical report on what happened.", "Oops!");
            await dialog.ShowAsync();
#endif
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// This event wraps HardwareButtons.BackPressed to ensure that any pages that
        /// want to override the default behavior can subscribe to this event to potentially
        /// handle the back button press a different way (e.g. dismissing dialogs).
        /// </summary>
        public event EventHandler<BackPressedEventArgs> BackPressed;
#endif

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = CreateRootFrame();
            RestoreStatus(e.PreviousExecutionState);

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                //Go to the MainPage is the user isn't signed in
                //Go straight to the Posts page is the user is signed in
                Type firstPage = typeof(MainPage);
                if(Tumblr.SignedIn)
                {
                    firstPage = typeof(Posts);
                }

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(firstPage, e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        protected override void OnActivated(IActivatedEventArgs e)
        {
            Frame rootFrame = CreateRootFrame();
            RestoreStatus(e.PreviousExecutionState);

            // Ensure the current window is active
            Window.Current.Activate();

			if (e.Kind == ActivationKind.Protocol)
			{
				if (Tumblr.SignedIn)
				{
					rootFrame.Navigate(typeof(Posts));
				}
				else
				{
					rootFrame.Navigate(typeof(MainPage));
				}

				return;
			}

            #region authentication
#if WINDOWS_PHONE_APP
            var webAuthContinuePage = rootFrame.Content as IWebAuthenticationContinuable;
            if (webAuthContinuePage != null && e is WebAuthenticationBrokerContinuationEventArgs)
            {
                webAuthContinuePage.ContinueWebAuthentication(e as WebAuthenticationBrokerContinuationEventArgs);
            }
#endif
            #endregion

#if WINDOWS_PHONE_APP
            var postPhotoPage = rootFrame.Content as PostPhoto;
            if (postPhotoPage != null && e is FileOpenPickerContinuationEventArgs)
            {
                postPhotoPage.ContinueFileOpenPicker(e as FileOpenPickerContinuationEventArgs);
            }
#endif
        }

        protected override void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            rootFrame.Navigate(typeof(PostPhoto), args.ShareOperation);

            Window.Current.Activate();
        }

        private Frame CreateRootFrame()
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            return rootFrame;
        }

        private async void RestoreStatus(ApplicationExecutionState previousExecutionState)
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (previousExecutionState == ApplicationExecutionState.Terminated)
            {
                // Restore the saved session state only when appropriate
                try
                {
                    await SuspensionManager.RestoreAsync();                    
                }
                catch (SuspensionManagerException)
                {
                    //Something went wrong restoring state.
                    //Assume there is no state and continue
                }
            }
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        /// <summary>
        /// Handles the back button press and navigates through the history of the root frame.
        /// </summary>
        /// <param name="sender">The source of the event. <see cref="HardwareButtons"/></param>
        /// <param name="e">Details about the back button press.</param>
        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            if (frame == null)
            {
                return;
            }

            var handler = this.BackPressed;
            if (handler != null)
            {
                handler(sender, e);
            }

            if (frame.CanGoBack && !e.Handled)
            {
                frame.GoBack();
                e.Handled = true;
            }
        }
#endif

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            if (PostsViewModel != null)
            {
                SuspensionManager.SessionState["Posts"] = Serialize(PostsViewModel);
            }

            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }


        private string Serialize(object o)
        {
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(o.GetType());
            var stream = new MemoryStream();
            jsonSerializer.WriteObject(stream, o);
            byte[] buffer = stream.ToArray();
            string json = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            return json;
        }

        private T Deserialize<T>(string json)
        {
            DataContractJsonSerializer jsonDeserializer = new DataContractJsonSerializer(typeof(T));
            T instance = (T)jsonDeserializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(json)));
            return instance;
        }
    }
}