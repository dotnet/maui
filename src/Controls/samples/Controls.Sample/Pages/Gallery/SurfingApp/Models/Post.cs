using System;

namespace Maui.Controls.Sample.Gallery.SurfingApp
{
    public class Post
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Image { get; set; }
        public string Likes { get; set; }
        public User User { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}