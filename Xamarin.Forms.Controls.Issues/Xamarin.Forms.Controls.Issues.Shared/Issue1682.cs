using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1682, "Software keyboard does not show up when we set Focus for Entry in Android", PlatformAffected.Android, NavigationBehavior.PushModalAsync)]
	public class Issue1682 : ContentPage
	{
		public Issue1682 ()
		{
			var entry = new Entry {
				WidthRequest = 300
			};

			var button = new Button { 
				Text = "Click"
			};

			button.Clicked += (sender, e) => entry.Focus ();

			Content = new StackLayout {
				Children = { entry, button },
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
			};
		}
	}
}

