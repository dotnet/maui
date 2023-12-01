using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 18457, "Adding/Removing Pages From Removed TabbedPage Causes Crash")]
public class Issue18457 : TestContentPage
{
	bool pagePushed = false;
	TabbedPage _tabbedPage = new TabbedPage();
	protected override void Init()
	{
		_tabbedPage.Children.Add(new ContentPage() { Title = "tab 1" });
		_tabbedPage.Children.Add(new ContentPage() { Title = "tab 2" });
		_tabbedPage.Children.Add(new ContentPage() { Title = "tab 3" });
	}

	protected override async void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);

		if (!pagePushed)
		{
			pagePushed = true;
			await Navigation.PushAsync(_tabbedPage);
			await Navigation.PopAsync();
			_tabbedPage.Children.Add(new ContentPage());
			_tabbedPage.Children[0].Background = SolidColorBrush.Purple;
			_tabbedPage.Children[0].Title = "update title";
			_tabbedPage.Children[0].IconImageSource = "dotnet_bot.png";
			await Task.Yield();
			Content = new VerticalStackLayout()
			{
				Children =
				{
					new Label() { Text = "This test pushes and pops a TabbedPage, and then modifies the Children on the popped page."},
					new Label() { Text = "If the app doesn't crash, this test has passed.", AutomationId = "Success" }
				}
			};
		}
	}
}
