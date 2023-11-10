using System;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.VisualExamples.Models
{
    // Used in CollectionViewDemoPage and ListViewDemoPage
    public class Person
    {
        public Person()
        {
        }

        public Person(string name, DateTime birthday, Color favoriteColor)
        {
            Name = name;
            Birthday = birthday;
            FavoriteColor = favoriteColor;
        }

        public string Name { set; get; }

        public DateTime Birthday { set; get; }

        public Color FavoriteColor { set; get; }
    };
}
