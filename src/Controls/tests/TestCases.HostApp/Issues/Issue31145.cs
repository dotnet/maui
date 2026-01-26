namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31145, "MaximumVisible Property Not Working with IndicatorTemplate in IndicatorView", PlatformAffected.All)]
public class Issue31145 : ContentPage
{
	public Issue31145()
	{
		Label descriptionLabel = new Label
		{
			Text = "The test passes if the IndicatorView with an IndicatorTemplate respects the MaximumVisible property; otherwise, it fails.",
			FontSize = 18,
			HorizontalOptions = LayoutOptions.Center,
		};

		IndicatorView indicatorView = new IndicatorView
		{
			ItemsSource = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" },
			IndicatorColor = Colors.Yellow,
			SelectedIndicatorColor = Colors.Gray,
			HorizontalOptions = LayoutOptions.Center
		};

		indicatorView.IndicatorTemplate = new DataTemplate(() =>
		{
			return new Label
			{
				Text = "\uf30c",
				FontFamily = "ionicons",
				FontSize = 12
			};
		});

		Button updateMaximumVisibleBtn = new Button
		{
			AutomationId = "UpdateMaximumVisibleBtn",
			Text = "Set MaximumVisible Property to 2"
		};

		updateMaximumVisibleBtn.Clicked += (s, e) =>
		{
			indicatorView.MaximumVisible = 2;
		};

		Content = new StackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				descriptionLabel,
				indicatorView,
				updateMaximumVisibleBtn
			}
		};
	}
}