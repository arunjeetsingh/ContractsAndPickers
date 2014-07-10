using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TumbleMe.TumblrApi;
using Windows.UI.Xaml;

namespace TumbleMe
{
    [DataContract]
    public class PostsViewModel : BaseViewModel
    {
        const int pageSize = 20;

#if WINDOWS_PHONE_APP
        const int maxImageWidth = 480;
#else
        const int maxImageWidth = 1600;
#endif

        public PostsViewModel()
        {
        }

        public async Task Load()
        {
            var tumblr = (Application.Current as App).Tumblr;
            List<Post> posts = await tumblr.GetPhotoPosts(0, pageSize);
            Posts.Clear();
            AddPosts(posts);
        }

        private void AddPosts(List<Post> posts)
        {
            var epoch = new DateTime(1970, 1, 1);

            foreach (Post p in posts)
            {
                AltSize imageSize = (from size in p.photos[0].alt_sizes
                                     where size.width < maxImageWidth
                                     orderby size.width ascending
                                     select size).Last();

                Posts.Add(new PostViewModel
                {
                    Title = p.caption,
                    ImageSource = imageSize.url,
                    Width = imageSize.width,
                    Height = imageSize.height,
                    OriginalImageSource = p.photos[0].original_size.url,
                    TimestampText = "Posted on " + epoch.AddSeconds(p.timestamp).ToLocalTime().ToString()
                });
            }
        }

        ObservableCollection<PostViewModel> _Posts = new ObservableCollection<PostViewModel>();
        [DataMember]
        public ObservableCollection<PostViewModel> Posts 
        {
            get
            {
                return _Posts;
            }
            set
            {
                _Posts = value;
                OnPropertyChanged("Posts");
            }
        }
    }
}
