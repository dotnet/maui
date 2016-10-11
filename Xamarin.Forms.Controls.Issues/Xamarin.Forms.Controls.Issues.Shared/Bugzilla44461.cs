using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44461, "ScrollToPosition.Center works differently on Android and iOS", PlatformAffected.iOS)]
	public class Bugzilla44461 : TestContentPage
	{
		protected override void Init()
		{
			var grid = new Grid
			{
				RowSpacing = 0,
			};

			var scrollView = new ScrollView
			{
				Orientation = ScrollOrientation.Horizontal,
				VerticalOptions = LayoutOptions.Center,
				BackgroundColor = Color.Yellow,
				HeightRequest = 50
			};
			grid.Children.Add(scrollView);

			var stackLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Spacing = 20
			};

			for (var i = 0; i < 10; i++)
			{
				var button = new Button
				{
					Text = "Button" + i
				};
				button.Clicked += (sender, args) =>
				{
					scrollView.ScrollToAsync(sender as Button, ScrollToPosition.Center, true);
				};

				stackLayout.Children.Add(button);
			}
			scrollView.Content = stackLayout;
			Content = grid;
		}
	}
}