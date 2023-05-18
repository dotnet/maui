using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1305, "ToolbarItems on NavigationPage broken", PlatformAffected.Android, NavigationBehavior.PushAsync)]
	public class Issue1305 : ContentPage
	{
		public Issue1305()
		{
			var settings = new ToolbarItem
			{
				IconImageSource = "bank.png",
				Text = "Settings",
				Command = new Command(ShowSettingsPage),
			};

			ToolbarItems.Add(settings);

			Content = new ContentView { Content = new Label { Text = "Testing..." } };

			Title = "Test Page";

			IconImageSource = "Icon.png";

		}

		async void ShowSettingsPage()
		{
			await Navigation.PushAsync(new Issue13052());
		}
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1305, "ToolbarItems on NavigationPage broken", PlatformAffected.Android)]
	public class Issue13052 : ContentPage
	{
		public Issue13052()
		{
			Content = new ContentView { Content = new Label { Text = "Settings..." } };

			Title = "Settings";

			IconImageSource = "bank.png";
		}
	}
}

