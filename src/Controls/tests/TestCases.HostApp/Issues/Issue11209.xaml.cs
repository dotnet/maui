namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 11209, "[Bug] [iOS][SwipeView] Swipe view not handling tap gesture events until swiped", PlatformAffected.Android)]
	public class Issue11209NavigationPage : NavigationPage
	{
		public Issue11209NavigationPage() : base(new Issue11209())
		{
		}
	}

	public partial class Issue11209 : TestContentPage
	{
		const string SwipeViewContent = "SwipeViewContent";
		const string Success = "Success";

		public Issue11209()
		{
			InitializeComponent();
		}

		public List<string> Items => new List<string> { "short", "long word", "Extra long word", "word up" };

		protected override void Init()
		{

		}

		void SwipeItem_Invoked(object sender, EventArgs e)
		{
			Console.WriteLine("Hey i was invoked");
		}

		async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new Issue11209SecondPage());
		}


		public class Issue11209SecondPage : ContentPage
		{
			public Issue11209SecondPage()
			{
				Title = "Issue 11209";

				var layout = new StackLayout();

				var instructions = new Label
				{
					AutomationId = Success,
					Padding = 12,
					BackgroundColor = Colors.Black,
					TextColor = Colors.White,
					Text = "If navigated tapping an item from the CollectionView, the test has passed."
				};

				layout.Children.Add(instructions);

				Content = layout;
			}
		}
	}
}