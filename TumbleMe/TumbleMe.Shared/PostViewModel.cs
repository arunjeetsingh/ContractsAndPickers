using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TumbleMe
{
    [DataContract]
    public class PostViewModel : BaseViewModel
    {
        string _Title;
        [DataMember]
        public string Title
        { 
            get
            {
                return _Title;
            }
            set
            {
                _Title = value;
                OnPropertyChanged("Title");
            }
        }

        string _ImageSource;
        [DataMember]
        public string ImageSource
        {
            get
            {
                return _ImageSource;
            }
            set
            {
                _ImageSource = value;
                OnPropertyChanged("ImageSource");
            }
        }

        int _Width;
        [DataMember]
        public int Width
        {
            get
            {
                return _Width;
            }
            set
            {
                _Width = value;
                OnPropertyChanged("Width");
            }
        }

        int _Height;
        [DataMember]
        public int Height
        {
            get
            {
                return _Height;
            }
            set
            {
                _Height = value;
                OnPropertyChanged("Height");
            }
        }

        string _OriginalImageSource;
        [DataMember]
        public string OriginalImageSource
        {
            get
            {
                return _OriginalImageSource;
            }
            set
            {
                _OriginalImageSource = value;
                OnPropertyChanged("OriginalImageSource");
            }
        }

        string _Timestamp;
        [DataMember]
        public string TimestampText
        {
            get
            {
                return _Timestamp;
            }
            set
            {
                _Timestamp = value;
                OnPropertyChanged("Timestamp");
            }
        }
    }
}
