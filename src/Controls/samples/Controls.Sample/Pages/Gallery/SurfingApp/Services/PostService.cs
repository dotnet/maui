using System.Collections.Generic;

namespace Maui.Controls.Sample.Gallery.SurfingApp
{
    public class PostService
    {
        static PostService _instance;

        public static PostService Instance
        {
            get
            {
                _instance ??= new PostService();

                return _instance;
            }
        }

        public List<Post> GetPosts()
        {
            return new List<Post>
            {
                new Post { Title = "Probably considered the forefather of pro surfing", Image = "post01.jpg", Likes = "1.2k", User = UserService.Instance.User1 },
                new Post { Title = "One of the most inspirational people in the public eye", Image = "post02.jpg", Likes = "225", User = UserService.Instance.User2 },
                new Post { Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit", Image = "post03.jpg", Likes = "111", User = UserService.Instance.User3 },
                new Post { Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit", Image = "post04.jpg", Likes = "988", User = UserService.Instance.User4 },
                new Post { Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit", Image = "post05.jpg", Likes = "210", User = UserService.Instance.User5 },
                new Post { Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit", Image = "post06.jpg", Likes = "334", User = UserService.Instance.User6 },
                new Post { Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit", Image = "post07.jpg", Likes = "122", User = UserService.Instance.User1 },
                new Post { Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit", Image = "post08.jpg", Likes = "1.4k", User = UserService.Instance.User2 }
            };
        }
    }
}