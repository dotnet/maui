using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Preserve(AllMembers = true)]
[Issue(IssueTracker.Bugzilla, 32902, "[iOS | iPad] App Crashes (without debug log) when Flyout Detail isPresented and navigation being popped")]
public class Bugzilla32902 : TestContentPage
{
	ContentPage FirstContentPage { get; set; }

	FlyoutPage HomePage { get; set; }

	NavigationPage DetailPage { get; set; }

	ContentPage RootPage { get; set; }

	protected override void Init()
	{
		var rootContentPageLayout = new StackLayout();
		var rootContentPageButton = new Button()
		{
			Text = "PushAsync to next page",
			AutomationId = "btnNext",
			BackgroundColor = Color.FromArgb("#ecf0f1"),
			TextColor = Colors.Black
		};
		rootContentPageButton.Clicked += async (sender, args) =>
		{
			await Navigation.PushAsync(FirstContentPage);
		};

		rootContentPageLayout.Children.Add(rootContentPageButton);
		Content = rootContentPageLayout;

		Title = "RootPage";
		BackgroundColor = Color.FromArgb("#2c3e50");

		//ROOT PAGE
		RootPage = new ContentPage()
		{
			Title = "Flyout",
			BackgroundColor = Color.FromArgb("#1abc9c")
		};
		var rootPageLayout = new StackLayout();
		var rootPageButton = new Button()
		{
			Text = "Pop Modal and Pop Root",
			AutomationId = "btnPop",
			BackgroundColor = Color.FromArgb("#ecf0f1"),
			TextColor = Colors.Black
		};
		rootPageButton.Clicked += async (sender, args) =>
		{
			await Navigation.PopModalAsync();
			await Navigation.PopToRootAsync();
		};
		rootPageLayout.Children.Add(rootPageButton);
		RootPage.Content = rootPageLayout;


		//DETAIL PAGE
		DetailPage = new NavigationPage(new ContentPage()
		{
			Title = "RootNavigationDetailPage",
			BackgroundColor = Color.FromArgb("#2980b9"),
			Content = new Button
			{
				Text = "PopModal",
				TextColor = Colors.White,
				Command = new Command(async () =>
				{
					await Navigation.PopModalAsync();
				})
			}
		});

		//FLYOUTPAGE PAGE
		HomePage = new FlyoutPage()
		{
			Flyout = RootPage,
			Detail = DetailPage
		};

		//FIRST CONTENT PAGE
		FirstContentPage = new ContentPage()
		{
			Title = "First Content Page",
			BackgroundColor = Color.FromArgb("#e74c3c")
		};
		var firstContentPageLayout = new StackLayout();
		var firstContentPageButton = new Button()
		{
			Text = "Push Modal To Flyout-Detail Page",
			AutomationId = "btnPushModal",
			BackgroundColor = Color.FromArgb("#ecf0f1"),
			TextColor = Colors.Black
		};
		firstContentPageButton.Clicked += async (sender, args) =>
		{
			await Navigation.PushModalAsync(HomePage);
		};
		firstContentPageLayout.Children.Add(firstContentPageButton);
		FirstContentPage.Content = firstContentPageLayout;
	}
}
