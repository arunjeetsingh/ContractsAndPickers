using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace TumbleMe
{
    class PhotoItem : BaseViewModel
    {
        public static async Task<PhotoItem> GetPhotoItemAsync(StorageFile sf)
        {
            PhotoItem pi = new PhotoItem();
            pi.File = sf;
            pi._Stream = await sf.OpenAsync(FileAccessMode.Read);

            pi.Caption = string.Empty;
            return pi;
        }

        public PhotoItem()
        {
        }

        private IRandomAccessStream _Stream;

        public async Task BeginImageRetrieval()
        {
            if (_Stream != null)
            {
                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(_Stream);
                this.Image = bi;
            }
        }
        
        public PhotoItem(StorageFile sf)
        {
            File = sf;
            if (File != null)
            {
                var syncCtx = TaskScheduler.FromCurrentSynchronizationContext();
                File.OpenAsync(FileAccessMode.Read).AsTask().ContinueWith(async _ =>
                    {
                        var stream = _.Result;
                        if (stream != null)
                        {
                            BitmapImage bi = new BitmapImage();
                            await bi.SetSourceAsync(stream);
                            Image = bi;
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            Caption = string.Empty;
        }

        public StorageFile File { get; private set; }
        
        private BitmapImage _image;
        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                if (_image != value)
                {
                    _image = value;
                    OnPropertyChanged("Image");
                }
            }
        }

        private string _caption;
        public String Caption
        {
            get { return _caption; }
            set
            {
                if (_caption != value)
                {
                    _caption = value;
                    OnPropertyChanged("Caption");
                }
            }
        }
    }
    
    class PostPhotoModel : BaseViewModel
    {
        private TumblrApi.TumblrHelper _helper;

        public PostPhotoModel(TumblrApi.TumblrHelper helper)
        {
            _helper = helper;
            CanShare = _helper.SignedIn;
            FilesToPost.CollectionChanged += FilesToPost_CollectionChanged;
        }

        void FilesToPost_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("DisplayPickerPrompt");
        }

        private bool _CanShare;
        public bool CanShare
        {
            get { return _CanShare; }
            set 
            { 
                if (_CanShare != value)
                {
                    _CanShare = value;
                    OnPropertyChanged("CanShare");
                    OnPropertyChanged("DisplayPickerPrompt");
                }
            }
        }

        public bool DisplayPickerPrompt
        {
            get { return FilesToPost.Count == 0 && CanShare && !EnsureNoPicker; }
        }

        private bool _ensureNoPicker;
        public bool EnsureNoPicker
        {
            get { return _ensureNoPicker; }
            set 
            { 
                if (value != _ensureNoPicker)
                {
                    _ensureNoPicker = value;
                    OnPropertyChanged("DisplayPickerPrompt");
                }
            }
        }

        private bool _ShareInProgress;
        public bool ShareInProgress
        {
            get { return _ShareInProgress; }
            set
            {
                if (_ShareInProgress != value)
                {
                    _ShareInProgress = value;
                    OnPropertyChanged("ShareInProgress");
                }
            }
        }

        private ObservableCollection<PhotoItem> _filesToPost = new ObservableCollection<PhotoItem>();
        public ObservableCollection<PhotoItem> FilesToPost { get { return _filesToPost; } }

        public async Task PostToTumblrAsync()
        {
            if (ShareInProgress)
            {
                throw new InvalidOperationException("Post already in progress...");
            }
            ShareInProgress = true;

            try
            {
                if (FilesToPost.Count > 0 && _helper.SignedIn)
                {
                    foreach(var postable in FilesToPost)
                    {
                        await _helper.PostToBlog(postable.File, postable.Caption);
                    }
                    return;
                }
            }
            finally
            {
                ShareInProgress = false;
            }
        }
    }
}
