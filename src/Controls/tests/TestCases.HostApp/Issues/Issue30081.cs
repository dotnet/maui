using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30081, "[Android] ScrollView scroll position changes unexpectedly when Orientation is set to Horizontal and FlowDirection is RTL at runtime", PlatformAffected.Android)]

public class Issue30081 : ContentPage
{
    readonly ScrollViewViewModel _viewModel;

    public Issue30081()
    {
        _viewModel = new ScrollViewViewModel();
        BindingContext = _viewModel;

        // Create Grid (same as XAML structure)
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        // Create ContentView and bind its Content
        var scrollViewContent = new ContentView
        {
            AutomationId = "ScrollViewContent"
        };
        scrollViewContent.SetBinding(ContentView.ContentProperty, nameof(ScrollViewViewModel.Content));

        // Create ScrollView and bind its Orientation
        var scrollView = new ScrollView
		{
			Content = scrollViewContent,
			FlowDirection = FlowDirection.RightToLeft
		};
        scrollView.SetBinding(ScrollView.OrientationProperty, nameof(ScrollViewViewModel.Orientation));

        // Add ScrollView to Grid row 0
        grid.Add(scrollView);
        Grid.SetRow(scrollView, 0);

        // Create Button
        var button = new Button
		{
			Text = "Toggle Orientation",
			AutomationId = "ToggleOrientationButton"
		};
        button.Clicked += Button_Clicked;

        // Add Button to Grid row 1
        grid.Add(button);
        Grid.SetRow(button, 1);

        // Set grid as the page content
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

public class ScrollViewViewModel : INotifyPropertyChanged
{
	string _contentText;
	ScrollOrientation _orientation = ScrollOrientation.Vertical;

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

	View _content;

	public ScrollViewViewModel()
	{
		_contentText = string.Empty;
		Content = new Label
		{
			Text = string.Join(Environment.NewLine, Enumerable.Range(1, 100).Select(i => $"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim, eget facilisis enim nisl nec elit . Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim Eget facilisis enim nisl nec elit Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae. Nullam ac erat at dui laoreet aliquet. Praesent euismod, justo at dictum facilisis, urna erat dictum enim. {i}")),
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
				Content = new Label { Text = _contentText }; // Update Content when ContentText changes
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