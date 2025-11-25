#nullable enable
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6994, "[iOS] Reusing the same page for formsheet Modal causes measuring issues", PlatformAffected.iOS)]
public class Issue6994FormSheet : ContentPage
{
	// Reuse the same modal page instance
	readonly Issue6994FormSheetModalPage _modalPage;
	readonly Label _statusLabel;
	int _openCount;

	public Issue6994FormSheet()
	{
		_modalPage = new Issue6994FormSheetModalPage();
		_openCount = 0;

		var openButton = new Button
		{
			Text = "Open FormSheet Modal",
			AutomationId = "OpenFormSheetButton"
		};
		openButton.Clicked += OnOpenModalClicked;

		_statusLabel = new Label
		{
			Text = "Tap 'Open FormSheet Modal' to begin test",
			AutomationId = "StatusLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 20,
			Children =
			{
				new Label
				{
					Text = "Issue 6994 - FormSheet Modal Reuse Test",
					FontAttributes = FontAttributes.Bold,
					HorizontalOptions = LayoutOptions.Center
				},
				new Label
				{
					Text = "This test verifies that reusing the same page for FormSheet modals maintains consistent dimensions. The modal dimensions should remain the same across multiple open/close cycles.",
					HorizontalOptions = LayoutOptions.Center
				},
				openButton,
				_statusLabel
			}
		};
	}

	async void OnOpenModalClicked(object? sender, EventArgs e)
	{
		_openCount++;
		_statusLabel.Text = $"Opening modal (attempt #{_openCount})...";

#if IOS
		var navigationPage = new NavigationPage(_modalPage);
		navigationPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
		await Navigation.PushModalAsync(navigationPage);
#else
		// On non-iOS platforms, just show the modal normally for testing purposes
		await Navigation.PushModalAsync(new NavigationPage(_modalPage));
#endif
	}
}

public class Issue6994FormSheetModalPage : ContentPage
{
	readonly Label _dimensionsLabel;

	public Issue6994FormSheetModalPage()
	{
		Title = "FormSheet Modal";

		_dimensionsLabel = new Label
		{
			AutomationId = "ModalDimensionsLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			FontSize = 24
		};

		var closeButton = new Button
		{
			Text = "Close Modal",
			AutomationId = "CloseModalButton"
		};
		closeButton.Clicked += OnCloseClicked;

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 20,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = "FormSheet Modal Page",
					FontAttributes = FontAttributes.Bold,
					HorizontalOptions = LayoutOptions.Center
				},
				new Label
				{
					Text = "Modal Dimensions:",
					HorizontalOptions = LayoutOptions.Center
				},
				_dimensionsLabel,
				closeButton
			}
		};
	}

	protected override void OnSizeAllocated(double width, double height)
	{
		base.OnSizeAllocated(width, height);

		// Update the dimensions label whenever size changes
		if (width > 0 && height > 0)
		{
			_dimensionsLabel.Text = $"{width:F0} x {height:F0}";
		}
	}

	async void OnCloseClicked(object? sender, EventArgs e)
	{
		await Navigation.PopModalAsync();
	}
}
