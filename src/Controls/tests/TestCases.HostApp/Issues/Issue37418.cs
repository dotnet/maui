using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37418, "TranslationY outside the screen adds incorrect top padding", PlatformAffected.Android)]
public class Issue37418 : Shell
{
	public Issue37418()
	{
		Routing.RegisterRoute(nameof(Issue37418ModalPage), typeof(Issue37418ModalPage));

		Items.Add(new ShellContent
		{
			Title = "Home",
			Route = nameof(Issue37418HomePage),
			ContentTemplate = new DataTemplate(typeof(Issue37418HomePage))
		});
	}
}

public class Issue37418HomePage : ContentPage
{
	public Issue37418HomePage()
	{
		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(30, 0),
				Spacing = 25,
				Children =
				{
					new Button
					{
						AutomationId = "OpenBottomSheetButton",
						Text = "Open Bottom Sheet",
						Command = new Command(async () => await Shell.Current.GoToAsync(nameof(Issue37418ModalPage)))
					}
				}
			}
		};
	}
}

public class Issue37418ModalPage : ContentPage
{
	public Issue37418ModalPage()
	{
		Shell.SetPresentationMode(this, PresentationMode.ModalNotAnimated);
		SafeAreaEdges = SafeAreaEdges.None;
		Background = Colors.Transparent;
		Content = new Issue37418BottomSheet
		{
			Content = new VerticalStackLayout
			{
				Padding = 16,
				Spacing = 12,
				Children =
				{
					new Label
					{
						AutomationId = "BottomSheetTitle",
						Text = "Bottom Sheet Content",
						TextColor = Colors.White,
						FontSize = 24,
						HorizontalOptions = LayoutOptions.Center
					},
					new Button
					{
						AutomationId = "BottomSheetCustomButton",
						Text = "Custom button"
					}
				}
			}
		};
	}
}

class Issue37418BottomSheet : ContentView
{
	Border _border;

	public Issue37418BottomSheet()
	{
		ControlTemplate = new ControlTemplate(CreateTemplate);
	}

	View CreateTemplate()
	{
		var dismissArea = new Grid
		{
			Background = Colors.Black,
			Opacity = 0.5
		};
		dismissArea.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = new Command(async () => await CloseAsync())
		});

		var bottomBackdrop = new Grid
		{
			Background = Colors.Black,
			Opacity = 0.5
		};

		var closeButton = new Button
		{
			AutomationId = "CloseBottomSheetButton",
			HeightRequest = 48,
			Text = "Close"
		};
		closeButton.Clicked += async (_, _) => await CloseAsync();

		var sheetContent = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(48),
				new RowDefinition(GridLength.Auto)
			}
		};
		sheetContent.Add(closeButton, 0, 0);
		sheetContent.Add(new ContentPresenter(), 0, 1);

		_border = new Border
		{
			AutomationId = "BottomSheetBorder",
			SafeAreaEdges = SafeAreaEdges.None,
			StrokeShape = new RoundRectangle
			{
				CornerRadius = new CornerRadius(24, 24, 0, 0)
			},
			Background = Colors.Black,
			IsVisible = false,
			Content = sheetContent
		};

		var root = new Grid
		{
			SafeAreaEdges = SafeAreaEdges.None,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Auto)
			}
		};
		root.Add(dismissArea, 0, 0);
		root.Add(bottomBackdrop, 0, 1);
		root.Add(_border, 0, 1);

		return root;
	}

	async Task CloseAsync()
	{
		if (_border is not null)
		{
			await _border.TranslateToAsync(_border.X, _border.Height, 300, Easing.CubicIn);
		}

		await Shell.Current.GoToAsync("..");
	}

	protected override async void OnSizeAllocated(double width, double height)
	{
		base.OnSizeAllocated(width, height);

		if (_border is null)
		{
			return;
		}

		_border.TranslationY = _border.Measure(double.PositiveInfinity, double.PositiveInfinity).Height;
		_border.IsVisible = true;
		await _border.TranslateToAsync(_border.X, 0, 300, Easing.CubicIn);
	}
}
