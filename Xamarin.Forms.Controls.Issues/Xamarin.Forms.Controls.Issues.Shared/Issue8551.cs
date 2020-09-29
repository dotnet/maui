using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8551, "Modal Page with transparent background", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.UWP)]
	public class Issue8551 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Issue 8551";

			var layout = new StackLayout();

			var instructions = new Label
			{
				Text = "Press the button below to navigate to a new page. The new page background must have transparency.",
				BackgroundColor = Color.Black,
				TextColor = Color.White
			};

			var navigateButton = new Button
			{
				Text = "Open Modal Transparent Page"
			};

			navigateButton.Clicked += (sender, args) =>
			{
				Navigation.PushModalAsync(new Issue8551ModalPage(), true);
			};

			layout.Children.Add(instructions);
			layout.Children.Add(navigateButton);

			Content = layout;
		}
	}

	public class Issue8551ModalPage : ContentPage
	{
		public Issue8551ModalPage()
		{
			BackgroundColor = Color.FromHex("700000FF");

			var layout = new Grid();

			var frame = new Frame
			{
				HeightRequest = 200,
				WidthRequest = 200,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				BackgroundColor = Color.White,
				CornerRadius = 12
			};

			var modalLayout = new StackLayout();

			var backButton = new Button
			{
				VerticalOptions = LayoutOptions.Center,
				Text = "Close"
			};

			backButton.Clicked += (sender, args) =>
			{
				Navigation.PopModalAsync(true);
			};

			modalLayout.Children.Add(backButton);

			frame.Content = modalLayout;

			layout.Children.Add(frame);

			Content = layout;
		}
	}
}