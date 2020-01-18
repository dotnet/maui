using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5596, "Previous page's toolbar item being grayed out when canceling swipe to previous page then navigating back", PlatformAffected.iOS)]
	public class Issue5596 : TestContentPage
	{

		protected override void Init()
		{
			ToolbarItems.Add(new ToolbarItem { Text = "Next Page", Command = new Command(async () => await Navigation.PushAsync(new MyTestPage())) });


			Content = new Label
			{
				Margin = 10,
				Text = "Click 'Next Page', start a swipe gesture to go back to the previous page but cancel it before it completes. Swipe again or click the back button on the upper left to go back. The toolbar item should not be grayed out."
			};
		}

		private class MyTestPage : ContentPage
		{

		}
	}
}
