#if IOS
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using UIKit;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39210, "NavigationPage page view overlaps an opaque navigation bar in landscape", PlatformAffected.iOS)]
public class Issue39210 : TestNavigationPage
{
	ContentPage _pushedPage;
	Label _frameResult;

	protected override void Init()
	{
#pragma warning disable CS0618 // The iOS-specific translucency API is obsolete but is required for this compatibility behavior.
		On<iOS>().SetIsNavigationBarTranslucent(false);
#pragma warning restore CS0618
		BarBackgroundColor = Color.FromArgb("#042F42");
		BarTextColor = Colors.White;

		_pushedPage = new ContentPage
		{
			Title = "Pushed",
			SafeAreaEdges = SafeAreaEdges.None
		};

		var header = new Grid
		{
			BackgroundColor = Color.FromArgb("#EFEFEF"),
			SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container, SafeAreaRegions.None, SafeAreaRegions.Container, SafeAreaRegions.None),
			Children =
			{
				new Label
				{
					Text = "Header (45pt): this text should be fully visible",
					VerticalOptions = LayoutOptions.Center,
					Margin = new Thickness(8, 0)
				}
			}
		};

		_frameResult = new Label
		{
			Text = "Tap the button to check native frames",
			AutomationId = "FrameResult"
		};

		var checkFrameButton = new Button
		{
			Text = "Check child frame",
			AutomationId = "CheckChildFrameButton"
		};
		checkFrameButton.Clicked += (_, _) => CheckChildFrame();

		var content = new VerticalStackLayout
		{
			Padding = 8,
			Spacing = 8,
			SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container, SafeAreaRegions.None, SafeAreaRegions.Container, SafeAreaRegions.None),
			Children = { checkFrameButton, _frameResult }
		};

		var rootLayout = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(45),
				new RowDefinition(GridLength.Star)
			},
			SafeAreaEdges = SafeAreaEdges.None,
			Children = { header, content }
		};
		Grid.SetRow(content, 1);
		_pushedPage.Content = rootLayout;

		PushAsync(new InitialPage(_pushedPage), animated: false);
	}

	sealed class InitialPage : ContentPage
	{
		readonly Microsoft.Maui.Controls.Page _pageToPush;
		bool _pushed;

		public InitialPage(Microsoft.Maui.Controls.Page pageToPush)
		{
			_pageToPush = pageToPush;
			Content = new Label { Text = "Root page" };
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			if (_pushed)
				return;

			_pushed = true;
			await Navigation.PushAsync(_pageToPush, animated: false);
		}
	}

	void CheckChildFrame()
	{
		if (Handler?.PlatformView is not UINavigationController navigationController ||
			_pushedPage.Handler is not Microsoft.Maui.IPlatformViewHandler pageHandler ||
			pageHandler.ViewController?.View is not UIView pageView)
		{
			_frameResult.Text = "FAIL: Navigation or page view is unavailable";
			return;
		}

		var pageFrame = pageView.ConvertRectToView(pageView.Bounds, navigationController.View);
		var navigationBarFrame = navigationController.NavigationBar.ConvertRectToView(
			navigationController.NavigationBar.Bounds,
			navigationController.View);

		_frameResult.Text = pageFrame.Top >= navigationBarFrame.Bottom
			? $"PASS: child top {pageFrame.Top:0.##}, bar bottom {navigationBarFrame.Bottom:0.##}"
			: $"FAIL: child top {pageFrame.Top:0.##}, bar bottom {navigationBarFrame.Bottom:0.##}";
	}
}
#endif
