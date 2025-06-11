using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 11812, "Setting Content of ContentView through style would crash on parent change", PlatformAffected.Android)]
public class Issue11812 : ContentPage
{
	Label _countLabel;
	ContentView _mainContentView;
	Issue11812ViewModel _viewModel;
	public Issue11812()
	{
		this.BindingContext = _viewModel = new Issue11812ViewModel();

		var button = new Button
		{
			Text = "Add ContentView",
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "TestButton"
		};
		button.Clicked += OnButtonClicked;

		_countLabel = new Label();
		_countLabel.AutomationId = "TestLabel";
		_countLabel.SetBinding(Label.TextProperty, "Count");

		_mainContentView = new ContentView();
		_mainContentView.SetBinding(ContentView.ContentProperty, "Content.InnerContentView");

		var layout = new VerticalStackLayout
		{
			Spacing = 25,
			Padding = new Thickness(30, 0),
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				button,
				_countLabel,
				_mainContentView
			}
		};

		Content = layout;
	}
	private void OnButtonClicked(object sender, EventArgs e)
	{
		_viewModel.Count++;
		_viewModel.Content = new Issue11812Model()
		{
			InnerContentView = new Issue11812InnerContentView() { BackgroundColor = Colors.Red }
		};
	}
}

public class Issue11812InnerContentView : ContentView
{
	public Issue11812InnerContentView()
	{
		Application.Current.Resources ??= new ResourceDictionary();

		if (!Application.Current.Resources.ContainsKey("SubContentStyle"))
		{
			Application.Current.Resources["SubContentStyle"] = new Style(typeof(ContentView))
			{
				Setters =
			{
				new Setter
				{
					Property = ContentView.ContentProperty,
					Value = new Label { Text = "SubContent" }
				}
			}
			};
		}
		Style = (Style)Application.Current.Resources["SubContentStyle"];
	}
}

public class Issue11812Model
{
	public Issue11812InnerContentView InnerContentView { get; set; }
}

public class Issue11812ViewModel : INotifyPropertyChanged
{
	public Issue11812ViewModel()
	{
		Content = new Issue11812Model()
		{
			InnerContentView = new Issue11812InnerContentView()
		};
	}

	private Issue11812Model _content;
	public Issue11812Model Content
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
	private int _count;
	public int Count
	{
		get => _count;
		set
		{
			if (_count != value)
			{
				_count = value;
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
