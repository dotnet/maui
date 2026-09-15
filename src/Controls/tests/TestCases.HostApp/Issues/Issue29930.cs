namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29930, "[Windows] Setting a ContentView with a content of StaticResource Style Causes a System.Runtime.InteropServices.COMException.", PlatformAffected.UWP)]
public class Issue29930 : ContentPage
{
	ContentView _mainContentView;
	public Issue29930()
	{
		var button = new Button
		{
			Text = "Add ContentView",
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "ChangeInnerContent"
		};
		button.Clicked += OnButtonClicked;

		_mainContentView = new ContentView();
		_mainContentView.Content = new Issue29930InnerContentView();

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
		_mainContentView.Content = new Issue29930InnerContentView() { BackgroundColor = Colors.Red };
	}
}

public class Issue29930InnerContentView : ContentView
{
	public Issue29930InnerContentView()
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