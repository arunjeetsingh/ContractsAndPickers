using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TumbleMe.TumblrApi
{
    [DataContract]
    public class Meta
    {
        [DataMember]
        public int status { get; set; }

        [DataMember]
        public string msg { get; set; }
    }

    [DataContract]
    public class Blog
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string url { get; set; }

        [DataMember]
        public int followers { get; set; }

        [DataMember]
        public bool primary { get; set; }

        [DataMember]
        public string title { get; set; }

        [DataMember]
        public string description { get; set; }

        [DataMember]
        public bool admin { get; set; }

        [DataMember]
        public int updated { get; set; }

        [DataMember]
        public int posts { get; set; }

        [DataMember]
        public int messages { get; set; }

        [DataMember]
        public int queue { get; set; }

        [DataMember]
        public int drafts { get; set; }

        [DataMember]
        public bool share_likes { get; set; }

        [DataMember]
        public bool ask { get; set; }

        [DataMember]
        public string tweet { get; set; }

        [DataMember]
        public string facebook { get; set; }

        [DataMember]
        public string facebook_opengraph_enabled { get; set; }

        [DataMember]
        public string type { get; set; }
    }

    [DataContract]
    public class User
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public int likes { get; set; }

        [DataMember]
        public int following { get; set; }

        [DataMember]
        public string default_post_format { get; set; }

        [DataMember]
        public List<Blog> blogs { get; set; }
    }

    [DataContract]
    public class Response
    {
        [DataMember]
        public User user { get; set; }

        [DataMember]
        public Blog blog { get; set; }

        [DataMember]
        public List<Post> posts { get; set; }

        [DataMember]
        public int total_posts { get; set; }
    }

    [DataContract]
    public class RootObject
    {
        [DataMember]
        public Meta meta { get; set; }

        [DataMember]
        public Response response { get; set; }        
    }

    [DataContract]
    public class AltSize
    {
        [DataMember]
        public int width { get; set; }

        [DataMember]
        public int height { get; set; }

        [DataMember]
        public string url { get; set; }
    }

    [DataContract]
    public class OriginalSize
    {
        [DataMember]
        public int width { get; set; }

        [DataMember]
        public int height { get; set; }

        [DataMember]
        public string url { get; set; }
    }

    [DataContract]
    public class Exif
    {
        [DataMember]
        public string Camera { get; set; }

        [DataMember]
        public int ISO { get; set; }

        [DataMember]
        public string Aperture { get; set; }

        [DataMember]
        public string Exposure { get; set; }

        [DataMember]
        public string FocalLength { get; set; }
    }

    [DataContract]
    public class Photo
    {
        [DataMember]
        public string caption { get; set; }

        [DataMember]
        public List<AltSize> alt_sizes { get; set; }

        [DataMember]
        public OriginalSize original_size { get; set; }

        [DataMember]
        public Exif exif { get; set; }
    }

    [DataContract]
    public class Post
    {
        [DataMember]
        public string blog_name { get; set; }

        [DataMember]
        public object id { get; set; }

        [DataMember]
        public string post_url { get; set; }

        [DataMember]
        public string slug { get; set; }

        [DataMember]
        public string type { get; set; }

        [DataMember]
        public string date { get; set; }

        [DataMember]
        public int timestamp { get; set; }

        [DataMember]
        public string state { get; set; }

        [DataMember]
        public string format { get; set; }

        [DataMember]
        public string reblog_key { get; set; }

        [DataMember]
        public List<object> tags { get; set; }

        [DataMember]
        public string short_url { get; set; }

        [DataMember]
        public List<object> highlighted { get; set; }

        [DataMember]
        public int note_count { get; set; }

        [DataMember]
        public string caption { get; set; }

        [DataMember]
        public string image_permalink { get; set; }

        [DataMember]
        public List<Photo> photos { get; set; }
    }

    [DataContract]
    public class AuthenticationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user was authenticated
        /// </summary>
        [DataMember]
        public bool UserAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is authorized to
        /// post to the blog
        /// </summary>
        [DataMember]
        public bool UserAuthorized { get; set; }

        /// <summary>
        /// Gets or sets details of the user if the user was authenticated
        /// </summary>
        [DataMember]
        public User User { get; set; }
    }
}
