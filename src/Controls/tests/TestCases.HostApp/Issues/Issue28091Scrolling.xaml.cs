using System.Collections.ObjectModel;
using Microsoft.Maui.Performance;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29065, "Implement scrolling performance metrics", PlatformAffected.All)]
public partial class Issue29065Scrolling : ContentPage
{
	public Issue29065Scrolling()
	{
		InitializeComponent();

		BindingContext = new Issue29065ScrollingViewModel();
		
		// Subscribe to real-time scrolling updates
		PerformanceProfiler.SubscribeToUpdates(null, OnScrollingUpdates);
	}

	void OnScrollingUpdates(ScrollingUpdate scrollingUpdate)
	{
		MainThread.BeginInvokeOnMainThread(() =>
		{
			var newEntry =
				$"FrameTime: {scrollingUpdate.FrameTime:0.00}, IsDroppedFrame: {scrollingUpdate.IsDroppedFrame}";
			PerformanceLabel.Text += newEntry + Environment.NewLine;
		});
	}

	public class Issue29065ScrollingViewModel : BindableObject
	{
		ObservableCollection<Issue29065ScrollingUser> _users;
		ObservableCollection<Issue29065ScrollingPost> _posts;

		public Issue29065ScrollingViewModel()
		{
			LoadData();
		}

		public ObservableCollection<Issue29065ScrollingUser> Users
		{
			get => _users;
			set
			{
				_users = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<Issue29065ScrollingPost> Posts
		{
			get => _posts;
			set
			{
				_posts = value;
				OnPropertyChanged();
			}
		}

		void LoadData()
		{
			var users = new List<Issue29065ScrollingUser>
			{
				new Issue29065ScrollingUser
				{
					Name = "Michael Scott",
					Image = "user01.jpg",
					Color = Color.FromArgb("#62D7FB"),
					From = "London, United Kingdom"
				},
				new Issue29065ScrollingUser
				{
					Name = "Emma Smith",
					Image = "user02.jpg",
					Color = Color.FromArgb("#9B4EC8"),
					From = "Berlin, Germany"
				},
				new Issue29065ScrollingUser
				{
					Name = "Pete Korando",
					Image = "user03.jpg",
					Color = Color.FromArgb("#CE4E8C"),
					From = "Paris, France"
				},
				new Issue29065ScrollingUser
				{
					Name = "Joseph Serio",
					Image = "user04.jpg",
					Color = Color.FromArgb("#4660C7"),
					From = "Madrid, Spain"
				},
				new Issue29065ScrollingUser
				{
					Name = "Stacie Miner",
					Image = "user05.jpg",
					Color = Color.FromArgb("#AF75CD"),
					From = "London, United Kingdom"
				},
				new Issue29065ScrollingUser
				{
					Name = "Carmela Delgado",
					Image = "user06.png",
					Color = Color.FromArgb("#C9E6F8"),
					From = "London, United Kingdom"
				}
			};

			Users = new ObservableCollection<Issue29065ScrollingUser>(users);

			var titles = new[]
			{
				"Probably considered the forefather of pro surfing",
				"One of the most inspirational people in the public eye", "Master of waves and saltwater wisdom",
				"Surfing like a true legend of the tides", "Paddling through history with every wave",
				"Redefining style on the surfboard", "Icon of beach culture and innovation",
				"Flowing like the ocean itself"
			};

			var posts = new List<Issue29065ScrollingPost>();

			for (int i = 0; i < 100; i++)
			{
				var user = users[i % users.Count];
				var title = titles[i % titles.Length];
				var image = $"post{(i % 8 + 1):D2}.jpg";
				var likes = $"{Random.Shared.Next(80, 1500)}{(Random.Shared.Next(2) == 0 ? "" : "k")}";

				posts.Add(new Issue29065ScrollingPost { Title = title, Image = image, Likes = likes, User = user });
			}

			Posts = new ObservableCollection<Issue29065ScrollingPost>(posts);
		}
	}

	public class Issue29065ScrollingUser
	{
		public string Name { get; set; }
		public string Image { get; set; }
		public Color Color { get; set; }
		public string From { get; set; }
	}

	public class Issue29065ScrollingPost
	{
		public string Title { get; set; }
		public string Image { get; set; }
		public string Likes { get; set; }
		public Issue29065ScrollingUser User { get; set; }
	}
}