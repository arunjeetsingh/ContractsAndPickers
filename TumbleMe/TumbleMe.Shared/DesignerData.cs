using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace TumbleMe.DesignData
{
    public class DesignTimePostPhotoData
    {
        public DesignTimePostPhotoData()
        {
            CanShare = true;
            FilesToPost = new ObservableCollection<FakePhotoData>();
            FilesToPost.Add(new FakePhotoData { Caption="Photo1"});
            FilesToPost.Add(new FakePhotoData { Caption="Photo2"});
        }

        public bool CanShare { get; set; }

        public bool DisplayPickerPrompt { get { return FilesToPost.Count == 0; } }

        public bool ShareInProgress { get; set; }
        
        public ObservableCollection<FakePhotoData> FilesToPost {get; set;}
    }

    public class FakePhotoData
    {
        public string Caption { get; set; }

        public string Image { get; set; }
    }
    public class FakeData
    {
        public FakeData()
        {
            PostPhotoData = new DesignTimePostPhotoData();
        }
        public DesignTimePostPhotoData PostPhotoData { get; set; }
    }
}
