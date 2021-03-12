using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "NavigationPage override UWP push pop", PlatformAffected.UWP, NavigationBehavior.Default)]
	public class NavPageOverrideUWP : ContentPage
	{
		public NavPageOverrideUWP()
		{
			var pushModalButton = new Button() { Text = "Open NavPage with overrides", BackgroundColor = Color.Blue, TextColor = Color.White };
			pushModalButton.Clicked += (s, a) => Navigation.PushModalAsync(new CustomNavPageForOverride(new Page1()));

			Content = new StackLayout { Children = { pushModalButton } };
		}

		public class Page1 : ContentPage
		{
			public Page1()
			{
				var pushButton = new Button() { Text = "Push with custom animation", BackgroundColor = Color.Blue, TextColor = Color.White };
				pushButton.Clicked += (s, a) => Navigation.PushAsync(new Page2());

				var popModalButton = new Button() { Text = "Pop modal", BackgroundColor = Color.Blue, TextColor = Color.White };
				popModalButton.Clicked += (s, a) => Navigation.PopModalAsync();

				Content = new StackLayout()
				{
					Padding = 40,
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					BackgroundColor = Color.Red,
					Children = { pushButton, popModalButton }
				};
			}
		}

		public class Page2 : ContentPage
		{
			public Page2()
			{
				var popButton = new Button() { Text = "Pop with custom animation", BackgroundColor = Color.Blue, TextColor = Color.White, HeightRequest = 50 };
				popButton.Clicked += (s, a) => Navigation.PopAsync();

				Content = new StackLayout
				{
					Padding = 40,
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					BackgroundColor = Color.Yellow,
					Children = { popButton }
				};
			}
		}


		public class CustomNavPageForOverride : NavigationPage
		{
			public CustomNavPageForOverride(Page page) : base(page)
			{

			}
		}
	}
}