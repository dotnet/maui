using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 59097, "[Android] Calling PopAsync via TapGestureRecognizer causes an application crash", PlatformAffected.Android)]
	public class Bugzilla59097 : NavigationPage
	{
		public Bugzilla59097() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				Navigation.PushAsync(new ContentPage { Content = new Label { Text = "previous page " } });
				Navigation.PushAsync(new ToPopPage());
			}

			public class ToPopPage : ContentPage
			{
				public ToPopPage()
				{
					var boxView = new BoxView { WidthRequest = 100, HeightRequest = 100, Color = Colors.Red, AutomationId = "boxView" };
					var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 1, Command = new Command(PopPageBack) };
					boxView.GestureRecognizers.Add(tapGesture);
					var layout = new StackLayout();
					layout.Children.Add(boxView);
					Content = layout;
				}

				async void PopPageBack(object obj)
				{
					await Navigation.PopAsync(true);
				}
			}
		}
	}
}