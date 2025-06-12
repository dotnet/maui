using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 11812, "Setting Content of ContentView through style would crash on parent change", PlatformAffected.Android)]
public class Issue11812 : ContentPage
{
	ContentView _mainContentView;
	public Issue11812()
	{
		this.BindingContext = new Issue11812ViewModel();

		var button = new Button
		{
			Text = "Add ContentView",
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "ChangeInnerContent"
		};
		button.Clicked += OnButtonClicked;

		_mainContentView = new ContentView();
		_mainContentView.SetBinding(ContentView.ContentProperty, "Content");

		var layout = new VerticalStackLayout
		{
			Spacing = 25,
			Padding = new Thickness(30, 0),
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				button,
				_mainContentView
			}
		};

		Content = layout;
	}
	private void OnButtonClicked(object sender, EventArgs e)
	{
		_mainContentView.Content = new Issue11812InnerContentView() { BackgroundColor = Colors.Red };
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
public class Issue11812ViewModel : INotifyPropertyChanged
{
	public Issue11812ViewModel()
	{
		Content = new Issue11812InnerContentView();
	}

	private Issue11812InnerContentView _content;
	public Issue11812InnerContentView Content
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

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
