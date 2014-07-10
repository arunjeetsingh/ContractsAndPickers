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

            if (!_eventEnabled)
            {
                Debug.WriteLine("Set up DataTransferManager");
                DataTransferManager.GetForCurrentView().DataRequested += OnDataRequested;
                _eventEnabled = true;
            }
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

            if (!_eventEnabled)
            {
                Debug.WriteLine("Set up DataTransferManager");
                DataTransferManager.GetForCurrentView().DataRequested += OnDataRequested;
                _eventEnabled = true;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (_eventEnabled && !(e.NavigationMode == NavigationMode.Forward && e.Uri == null))
            {
                Debug.WriteLine("Tear down DataTransferManager");
                DataTransferManager.GetForCurrentView().DataRequested -= OnDataRequested;
                _eventEnabled = false;
            }
        }

        void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var linkToShare = new Uri(_model.OriginalImageSource, UriKind.Absolute);
            string extension = Path.GetExtension(linkToShare.AbsolutePath);

            DataPackage dp = args.Request.Data;
            dp.Properties.Title = "Build 2014";
            dp.Properties.FileTypes.Add(extension);

            dp.SetWebLink(linkToShare);
            dp.SetDataProvider(StandardDataFormats.StorageItems, new DataProviderHandler(async request =>
            {
                var deferral = request.GetDeferral();
                var file = await SaveUrlToDisk(linkToShare);
                request.SetData(new StorageFile[] { file });
                deferral.Complete();
            }));
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
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
