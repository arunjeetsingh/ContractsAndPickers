using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TumbleMe.TumblrApi;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
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
    public sealed partial class PostDetailPage : Page
    {
        bool _eventEnabled;
        Uri _linkToShare;
        TumblrHelper _tumblr;
        App _currentApp;
        PostViewModel _model;

        public PostDetailPage()
        {
            this.InitializeComponent();
        }

        void PostDetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("PostDetailPage_Loaded");            

            // OnNavigatedTo
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("OnNavigatedTo"); 
            _currentApp = Application.Current as App;
            _tumblr = _currentApp.Tumblr;

            if (e != null)
            {
                DataContext = _model = _currentApp.PostsViewModel.Posts[(int)e.Parameter];
            }

            // OnNavigatedTo
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // OnNavigatedFrom
        }

        // OnDataRequested

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            // ShareButtonClick
        }

        static private async Task<StorageFile> SaveUrlToDisk(Uri url)
        {   
            string filename = Path.GetFileName(url.AbsolutePath);
            StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
            StorageFile file = await tempFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            using (var httpClient = new HttpClient())
            using (var httpResponse = await httpClient.GetAsync(url))
            {
                await FileIO.WriteBufferAsync(file, await httpResponse.Content.ReadAsBufferAsync());
            }

            return file;
        }
    }
}
