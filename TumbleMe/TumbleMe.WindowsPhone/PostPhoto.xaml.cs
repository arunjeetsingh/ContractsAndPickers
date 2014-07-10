using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TumbleMe.TumblrApi;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TumbleMe
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PostPhoto : Page
    {
        ShareOperation _shareOp;
        PostPhotoModel _postPhotoModel;

        public PostPhoto()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            _postPhotoModel = new PostPhotoModel((Application.Current as App).Tumblr);
            _shareOp = e.Parameter as ShareOperation;
            this.DataContext = _postPhotoModel;

            if (_shareOp != null)
            {
                var files = await _shareOp.Data.GetStorageItemsAsync();
                foreach (var file in files)
                {
                    _postPhotoModel.FilesToPost.Add(new PhotoItem(file as StorageFile));
                }
            }
        }
        
        private async void PostPhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _postPhotoModel.PostToTumblrAsync();

                if (_shareOp != null)
                {
                    _shareOp.ReportCompleted();
                }
                else
                {
                    this.Frame.GoBack();
                }
            }
            catch (Exception ex)
            {
                _shareOp.ReportError(ex.ToString());
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (_shareOp != null)
            {
                _shareOp.ReportCompleted();
            }
            else
            {
                this.Frame.GoBack();
            }
        }

        private void SelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.PickSingleFileAndContinue();
        }

        public void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            if (args.Files == null)
            {
                return;
            }

            foreach (var file in args.Files)
            {
                _postPhotoModel.FilesToPost.Add(new PhotoItem(file));
            }
        }
    }
}
