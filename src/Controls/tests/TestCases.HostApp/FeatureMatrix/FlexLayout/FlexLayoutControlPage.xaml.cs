using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample;

public class FlexLayoutControlPage : NavigationPage
{
	FlexLayoutViewModel _viewModel;
	public FlexLayoutControlPage()
	{
		_viewModel = new FlexLayoutViewModel();
		PushAsync(new FlexLayoutControlMainPage(_viewModel));
	}
}

public partial class FlexLayoutControlMainPage : ContentPage
{
	FlexLayoutViewModel _viewModel;
	private int _boxCounter = 6;
	private readonly List<Color> ExtraColors = new()
	{
		Color.FromArgb("#e15759"), Color.FromArgb("#FFDA8D00"), Color.FromArgb("#FF009DC0")
	};
	public FlexLayoutControlMainPage(FlexLayoutViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new FlexLayoutViewModel();
		await Navigation.PushAsync(new FlexLayoutOptionsPage(_viewModel));
	}

	private void AddChildButton_Clicked(object sender, EventArgs e)
	{
		int n;
#if MACCATALYST || WINDOWS
        n = 15;
#else
		n = 3;
#endif
		for (int i = 0; i < n; i++)
		{
			var newBorder = new Border
			{
				AutomationId = $"Child{_boxCounter}",
				BackgroundColor = ExtraColors[i % 3],
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle { CornerRadius = 8 }
			};
			newBorder.SetBinding(Border.WidthRequestProperty, new Binding("WidthRequest"));
			newBorder.SetBinding(Border.HeightRequestProperty, new Binding("SpecificHeightRequest"));

			var label = new Label
			{
				AutomationId = $"Child{_boxCounter}",
				Text = $"Child{_boxCounter}",
				TextColor = Colors.White,
				FontSize = 12,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			newBorder.Content = label;

			MainFlexLayout.Children.Add(newBorder);

			_boxCounter++;
		}
	}

	private void RemoveChildButton_Clicked(object sender, EventArgs e)
	{
		int n;
#if MACCATALYST || WINDOWS
        n = 15;
#else
		n = 3;
#endif
		for (int i = 0; i < n; i++)
		{
			if (MainFlexLayout.Children.Count > 0)
			{
				MainFlexLayout.Children.RemoveAt(MainFlexLayout.Children.Count - 1);
				_boxCounter--;
			}
		}
	}
}