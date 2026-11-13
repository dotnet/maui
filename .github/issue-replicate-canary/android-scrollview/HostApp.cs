#if ANDROID
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37323, "ScrollView padding updates through binding", PlatformAffected.Android)]
public class Issue37323 : ContentPage
{
	Thickness _scrollPadding;

	public Issue37323()
	{
		var applyPaddingButton = new Button
		{
			AutomationId = "ApplyPadding",
			Text = "Apply padding"
		};
		var statusLabel = new Label
		{
			AutomationId = "Status",
			Text = "Padding: 0"
		};
		var contentLabel = new Label
		{
			AutomationId = "ScrollContent",
			Text = "ScrollView content"
		};
		var scrollView = new ScrollView
		{
			AutomationId = "TestScrollView",
			Content = contentLabel
		};
		scrollView.SetBinding(ScrollView.PaddingProperty, nameof(ScrollPadding));
		applyPaddingButton.Clicked += (_, _) =>
		{
			ScrollPadding = new Thickness(40);
			statusLabel.Text = "Padding: 40";
		};
		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			}
		};
		Grid.SetRow(applyPaddingButton, 0);
		grid.Add(applyPaddingButton);
		Grid.SetRow(statusLabel, 1);
		grid.Add(statusLabel);
		Grid.SetRow(scrollView, 2);
		grid.Add(scrollView);
		BindingContext = this;
		Content = grid;
	}

	public Thickness ScrollPadding
	{
		get => _scrollPadding;
		set
		{
			if (_scrollPadding == value)
				return;
			_scrollPadding = value;
			OnPropertyChanged();
		}
	}
}
#endif
