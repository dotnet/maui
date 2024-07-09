using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 29453, "Navigation.PopAsync(false) in Entry.Completed handler => System.ArgumentException", PlatformAffected.Android)]
	public class Bugzilla29453 : NavigationPage
	{
		public Bugzilla29453() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				var page1Layout = new StackLayout
				{
					Children = {
					new Label {
						AutomationId = "Page1",
						HorizontalTextAlignment = TextAlignment.Center,
						Text = "Page 1"
					}
				}
				};

				var page2Layout = new StackLayout
				{
					Children = {
					new Label {
						HorizontalTextAlignment = TextAlignment.Center,
						Text = "Page 2"
					}
				}
				};

				var entry = new Entry { AutomationId = "entryText" };

				entry.Completed += async (sender, args) =>
				{
					await Navigation.PopAsync(false);
				};

				page2Layout.Children.Add(entry);

				var page2 = new ContentPage
				{
					Content = page2Layout
				};

				var button = new Button
				{
					Text = "Go to page 2",
					AutomationId = "btnGotoPage2",
					Command = new Command(async () => await Navigation.PushAsync(page2))
				};

				page1Layout.Children.Add(button);
				Content = page1Layout;
			}
		}
	}
}