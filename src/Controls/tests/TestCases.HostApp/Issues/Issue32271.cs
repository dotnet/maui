using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32271, "ScrollView with RTL FlowDirection and Horizontal Orientation scrolls in the wrong direction on iOS", PlatformAffected.iOS)]

public class Issue32271 : ContentPage
{
	Issue32271ScrollViewViewModel _viewModel;

	public Issue32271()
	{
		_viewModel = new Issue32271ScrollViewViewModel();
		BindingContext = _viewModel;

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Star },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto }
			}
		};

		var scrollViewContent = new ContentView
		{
			AutomationId = "ScrollViewContent"
		};
		scrollViewContent.SetBinding(ContentView.ContentProperty, static (Issue32271ScrollViewViewModel vm) => vm.Content);

		var scrollView = new ScrollView
		{
			Content = scrollViewContent,
			FlowDirection = FlowDirection.RightToLeft,
		};
		scrollView.SetBinding(ScrollView.OrientationProperty, static (Issue32271ScrollViewViewModel vm) => vm.Orientation);

		grid.Add(scrollView);
		Grid.SetRow(scrollView, 0);
		
		var toggleButton = new Button
		{
			Text = "Toggle Orientation",
			AutomationId = "ToggleOrientationButton"
		};
		toggleButton.Clicked += Button_Clicked;

		var scrollToEndButton = new Button
		{
			Text = "Scroll",
			AutomationId = "ScrollToEndButton"
		};
		scrollToEndButton.Clicked += async (s, e) =>
		{
			if (_viewModel.Content != null)
				await scrollView.ScrollToAsync(_viewModel.Content, ScrollToPosition.Center, true);
		};

		var buttonLayout = new HorizontalStackLayout
		{
			HorizontalOptions = LayoutOptions.Center,
			Spacing = 20,
			Children = { toggleButton, scrollToEndButton }
		};

		grid.Add(buttonLayout);
		Grid.SetRow(buttonLayout, 1);

		Content = grid;
	}

	void Button_Clicked(object sender, EventArgs e)
	{
		if (_viewModel.Orientation == ScrollOrientation.Vertical)
			_viewModel.Orientation = ScrollOrientation.Horizontal;
		else
			_viewModel.Orientation = ScrollOrientation.Vertical;
	}
}


public class Issue32271ScrollViewViewModel : INotifyPropertyChanged
{
	string _contentText;
	ScrollOrientation _orientation = ScrollOrientation.Vertical;
	View _content;

	public ScrollOrientation Orientation
	{
		get => _orientation;
		set
		{
			if (_orientation != value)
			{
				_orientation = value;
				OnPropertyChanged();
			}
		}
	}

	public Issue32271ScrollViewViewModel()
	{
		_contentText = string.Empty;
		Content = new Label
		{
			Text = string.Join(Environment.NewLine, Enumerable.Range(1, 100).Select(i =>
				$"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim, eget facilisis enim nisl nec elit . Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim Eget facilisis enim nisl nec elit Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae. Nullam ac erat at dui laoreet aliquet. Praesent euismod, justo at dictum facilisis, urna erat dictum enim. {i}")),
			FontSize = 18,
			Padding = 10
		};
	}

	public string ContentText
	{
		get => _contentText;
		set
		{
			if (_contentText != value)
			{
				_contentText = value;
				Content = new Label { Text = _contentText };
				OnPropertyChanged();
			}
		}
	}

	public View Content
	{
		get => _content;
		set
		{
			if (_content != value)
			{
				_content = value;
				OnPropertyChanged();
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
