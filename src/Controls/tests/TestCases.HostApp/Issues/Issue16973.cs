using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 16973, "RefreshView RefreshColor is not working", PlatformAffected.UWP)]
public class Issue16973 : ContentPage
{
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
		var label = new Label
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

		var refreshView = new RefreshView
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
#if WINDOWS
			if (refreshView.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.RefreshContainer refreshContainer)
			{
				var refreshColor = refreshView.RefreshColor.ToWindowsColor();
				var visualizerForeground = (refreshContainer.Visualizer.Foreground as Microsoft.UI.Xaml.Media.SolidColorBrush)?.Color;

				label.Text = (refreshColor == visualizerForeground) ? "Color matches" : "Color does not match";
			}
#endif
		};
		grid.Children.Add(label);
		grid.Children.Add(button);
		grid.Children.Add(refreshView);
		Content = grid;
	}
}
