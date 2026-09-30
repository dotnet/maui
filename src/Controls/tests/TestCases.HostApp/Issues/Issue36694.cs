using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36694, "Setting BackgroundColor on Image via binding causes COMException after navigation", PlatformAffected.UWP)]
public class Issue36694 : NavigationPage
{
	public Issue36694() : base(new Issue36694MainPage())
	{
	}
}

public class Issue36694MainPage : ContentPage
{
	public Issue36694MainPage()
	{
		var viewModel = new Issue36694ViewModel();
		BindingContext = viewModel;

		var image = new Image
		{
			WidthRequest = 100,
			HeightRequest = 100,
			Source = "dotnet_bot.png",
			AutomationId = "TestImage"
		};
		image.SetBinding(Image.BackgroundColorProperty, nameof(Issue36694ViewModel.ImageBackgroundColor));

		var navigateButton = new Button
		{
			Text = "Navigate to Options",
			AutomationId = "NavigateButton"
		};
		navigateButton.Clicked += async (s, e) =>
		{
			await Navigation.PushAsync(new Issue36694OptionsPage(viewModel));
		};

		var resultLabel = new Label
		{
			Text = "No exception",
			AutomationId = "ResultLabel"
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 15,
			Children =
			{
				image,
				navigateButton,
				resultLabel
			}
		};
	}
}

public class Issue36694OptionsPage : ContentPage
{
	public Issue36694OptionsPage(Issue36694ViewModel viewModel)
	{
		Title = "Options";

		var changeColorButton = new Button
		{
			Text = "Change BackgroundColor to Red",
			AutomationId = "ChangeColorButton"
		};
		changeColorButton.Clicked += (s, e) => viewModel.ImageBackgroundColor = Colors.Red;

		var goBackButton = new Button
		{
			Text = "Go Back",
			AutomationId = "GoBackButton"
		};
		goBackButton.Clicked += async (s, e) => await Navigation.PopAsync();

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 15,
			Children =
			{
				new Label { Text = "Change background color while on a different page:" },
				changeColorButton,
				goBackButton
			}
		};
	}
}

public class Issue36694ViewModel : INotifyPropertyChanged
{
	// Must start as null to reproduce the bug. A non-null Background makes
	// ImageHandler.NeedsContainer true from the first render, so the container
	// already exists and the COMException never occurs.
	Color _imageBackgroundColor;
	public Color ImageBackgroundColor
	{
		get => _imageBackgroundColor;
		set
		{
			if (_imageBackgroundColor != value)
			{
				_imageBackgroundColor = value;
				OnPropertyChanged();
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
