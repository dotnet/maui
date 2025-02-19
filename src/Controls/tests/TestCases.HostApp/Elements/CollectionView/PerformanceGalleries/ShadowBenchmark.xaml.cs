namespace Maui.Controls.Sample.CollectionViewGalleries.PerformanceGalleries
{
	public class ShadowBenchmarkShell : Shell
	{
		public ShadowBenchmarkShell()
		{
			FlyoutBehavior = FlyoutBehavior.Flyout;
			Items.Add(new ShellContent
			{
				Content = new ShadowBenchmark(),
				Title = "Shadow Benchmark"
			});
		}
	}

	public partial class ShadowBenchmark : ContentPage
	{
		public ShadowBenchmark()
		{
			BindingContext = new ShadowBenchmarkViewModel();
			InitializeComponent();
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

	public class ShadowBenchmarkViewModel
	{
		public List<Post> Posts { get; } = GetData();

		static List<Post> GetData()
		{
			var posts = new List<Post>();

			User user = new User
			{
				Name = "Lorem ipsum",
				Image = "oasis.jpg",
				Color = Color.FromArgb("#62D7FB"),
				From = "Lorem ipsum"
			};

			for (int i = 0; i < 1000; i++)
			{
				posts.Add(new Post { Title = $"Lorem ipsum {i + 1} dolor sit amet, consectetur adipiscing elit", Image = "photo21314.jpg", Likes = "1k", User = user });
			}

			return posts;
		}
	}
}