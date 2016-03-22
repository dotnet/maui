using System;

using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1583, "NavigationPage.TitleIcon broken", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public class Issue1583 : ContentPage
	{
		public Issue1583 ()
		{
			Title = "Test";
			BackgroundColor = Color.Pink;
			Content = new Label{Text = "Hello"};
			NavigationPage.SetTitleIcon(this, "bank.png");
		}
	}
}

