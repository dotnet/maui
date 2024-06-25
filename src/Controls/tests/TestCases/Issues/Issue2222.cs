using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2222, "NavigationBar.ToolbarItems.Add() crashes / breaks app in iOS7. works fine in iOS8", PlatformAffected.iOS)]
	public class Issue2222 : NavigationPage
	{
		public Issue2222() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				var tbItem = new ToolbarItem { Text = "hello", IconImageSource = "wrongName" };
				ToolbarItems.Add(tbItem);

				Navigation.PushAsync(new Issue22221());
			}

			[Preserve(AllMembers = true)]
			public class Issue22221 : ContentPage
			{
				public Issue22221()
				{
					Content = new StackLayout
					{
						Children = {
						new Label { AutomationId = "TestLabel", Text = "Hello Toolbaritem" }
					}
					};
				}
			}
		}
	}
}
