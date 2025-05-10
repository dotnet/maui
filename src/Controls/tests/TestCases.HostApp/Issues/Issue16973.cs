using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 16973, "RefreshView RefreshColor is not working", PlatformAffected.UWP)]
public class Issue16973 : ContentPage
{
	Label label;
	RefreshView refreshView;
	public Issue16973()
	{
		var grid = new Grid
		{
			RowDefinitions = new RowDefinitionCollection
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = new GridLength(500) }
			}
		};
		label = new Label
		{
			AutomationId = "label",
			Text = "Tap the button to change the refresh color."
		};
		Grid.SetRow(label, 0);

		var button = new Button
		{
			Text = "Change Refresh Color",
			AutomationId = "button",
			WidthRequest = 200,
		};
		Grid.SetRow(button, 1);

		refreshView = new RefreshView
		{
			RefreshColor = Colors.Red,
			IsRefreshing = true,
		};
		Grid.SetRow(refreshView, 2);

		var scrollView = new ScrollView();
		var label2 = new Label
		{
			Text = "If the color matches, the test has passed successfully.",
		};
		scrollView.Content = label2;
		refreshView.Content = scrollView;

		button.Clicked += (s, e) =>
		{
#if __IOS__ || __MACCATALYST__
			VerifyRefreshColorMatchIOS();
#elif WINDOWS
			VerifyRefreshColorMatchWindows();
#endif
		};
		grid.Children.Add(label);
		grid.Children.Add(button);
		grid.Children.Add(refreshView);
		Content = grid;
	}

	void VerifyRefreshColorMatchWindows()
	{
#if WINDOWS
			if (refreshView.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.RefreshContainer refreshContainer)
			{
				var refreshColor = refreshView.RefreshColor.ToWindowsColor();
				var visualizerForeground = (refreshContainer.Visualizer.Foreground as Microsoft.UI.Xaml.Media.SolidColorBrush)?.Color;

				label.Text = (refreshColor == visualizerForeground) ? "Color matches" : "Color does not match";
			}
#endif
	}

	void VerifyRefreshColorMatchIOS()
	{
#if __IOS__ || __MACCATALYST__
		if (refreshView.Handler?.PlatformView is MauiRefreshView refreshControl)
		{
			var refreshColor = refreshView.RefreshColor.ToPlatform();
			var visualizerForeground = refreshControl.RefreshControl.TintColor;

			bool AreColorsEqual(UIKit.UIColor color1, UIKit.UIColor color2)
			{
				if (color1 == null || color2 == null)
					return false;

				color1.GetRGBA(out nfloat r1, out nfloat g1, out nfloat b1, out nfloat a1);
				color2.GetRGBA(out nfloat r2, out nfloat g2, out nfloat b2, out nfloat a2);

				return (r1 == r2) && (g1 == g2) && (b1 == b2) && (a1 == a2);
			}

			label.Text = visualizerForeground != null && AreColorsEqual(refreshColor, visualizerForeground)
				? "Color matches"
				: "Color does not match";
		}
#endif
	}
}
