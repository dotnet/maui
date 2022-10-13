using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class ShadowBenchmark : ContentPage
	{
		public ShadowBenchmark()
		{
			InitializeComponent();

			BindingContext = new ShadowBenchmarkViewModel();
		}
	}

	public class User
	{
		public string Name { get; set; }
		public string Image { get; set; }
		public Color Color { get; set; }
		public string From { get; set; }
	}

	public class Post
	{
		public string Title { get; set; }
		public string Content { get; set; }
		public string Image { get; set; }
		public string Likes { get; set; }
		public User User { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	public class ShadowBenchmarkViewModel : BindableObject
	{
		ObservableCollection<Post> _posts; 
		
		public ShadowBenchmarkViewModel()
		{
			LoadData();
		}

		public ObservableCollection<Post> Posts
		{
			get { return _posts; }
			set
			{
				_posts = value;
				OnPropertyChanged();
			}
		}

		void LoadData()
		{
			Posts = new ObservableCollection<Post>();

			User user = new User
			{
				Name = "Lorem ipsum",
				Image = "oasis.jpg",
				Color = Color.FromArgb("#62D7FB"),
				From = "Lorem ipsum"
			};

			for(int i = 0; i < 1000; i++)
			{
				Posts.Add(new Post { Title = $"Lorem ipsum dolor sit amet, consectetur adipiscing elit {1}", Image = "photo.jpg", Likes = "1k", User = user });
			}
		}
	}
}