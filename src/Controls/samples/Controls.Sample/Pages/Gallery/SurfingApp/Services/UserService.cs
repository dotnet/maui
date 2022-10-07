using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Gallery.SurfingApp
{
    public class UserService
    {
        static UserService _instance;

        public static UserService Instance
        {
            get
            {
                _instance ??= new UserService();

                return _instance;
            }
        }

        public readonly User User1 = new User
        {
            Name = "Michael Scott",
            Image = "user01.jpg",
            Color = Color.FromArgb("#62D7FB"),
            From = "London, United Kingdom"
        };

        public readonly User User2 = new User
        {
            Name = "Emma Smith",
            Image = "user02.jpg",
            Color = Color.FromArgb("#9B4EC8"),
            From = "Berlin, Germany"
        };

        public readonly User User3 = new User
        {
            Name = "Pete Korando",
            Image = "user03.jpg",
            Color = Color.FromArgb("#CE4E8C"),
            From = "Paris, France"
        };

        public readonly User User4 = new User
        {
            Name = "Joseph Serio",
            Image = "user04.jpg",
            Color = Color.FromArgb("#4660C7"),
            From = "Madrid, Spain"
        };

        public readonly User User5 = new User
        {
            Name = "Stacie Miner",
            Image = "user05.jpg",
            Color = Color.FromArgb("#AF75CD"),
            From = "London, United Kingdom"
        };

        public readonly User User6 = new User
        {
            Name = "Carmela Delgado",
            Image = "user06.png",
            Color = Color.FromArgb("#C9E6F8"),
            From = "London, United Kingdom"
        }; 
              
        public List<User> GetUsers()
        {
            return new List<User>
            {
                User1, User2, User3, User4, User5, User6
            };
        }
    }
}