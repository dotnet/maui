
using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1329, "Losing root page with rapidly pushing and popping", PlatformAffected.Android)]
	public class Issue1329 : NavigationPage
	{
		public Issue1329 () : base (GetPage ("root page"))
		{
			var navigation = new NavigationPage (GetPage ("root page"));
			var pageNum = 0;

			MessagingCenter.Subscribe<Button> (
				navigation, 
				"PushPage", 
				(sender) => navigation.PushAsync (GetPage ("Page #: " + ++pageNum)) 
			);

			navigation.Popped += (sender, e) => {
				pageNum -= 1;
			};
		}
			
		public static Page GetPage (string name) {	

			var button = new Button {
				Text = name
			};

			button.Clicked += (sender, e) => MessagingCenter.Send<Button> ((Button)sender, "PushPage");

			var page = new ContentPage {
				Content = button
			};

			return page;
		}

	}

}

