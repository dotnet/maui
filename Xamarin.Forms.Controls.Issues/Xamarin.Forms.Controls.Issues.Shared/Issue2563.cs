using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 2563, "NavigationPage should support queuing of navigation events", PlatformAffected.Android | PlatformAffected.WinPhone | PlatformAffected.iOS)]
	public class Issue2563 : ContentPage
	{
		public Issue2563 ()
		{
			var button = new Button {
				Text = "Click Me",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};

			Content = button;

			var random = new Random ();
			button.Clicked += (sender, args) => {
				for (int i = 0; i < 10; i++) {
					button.Navigation.PushAsync (new ContentPage {
						Title = "Page " + i,
						Content = new Label {
							Text = "Page " + i,
#pragma warning disable 618
							XAlign = TextAlignment.Center,
#pragma warning restore 618

#pragma warning disable 618
							YAlign = TextAlignment.Center
#pragma warning restore 618
						}
					}, random.NextDouble () > 0.5);
				}

				for (int i = 0; i < 6; i++) {
					button.Navigation.PopAsync (random.NextDouble () > 0.5);
				}

				button.Navigation.PopToRootAsync (random.NextDouble () > 0.5);
			};
		}
	}
}
