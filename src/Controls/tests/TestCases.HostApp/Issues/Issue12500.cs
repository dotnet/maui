using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Maui.Controls.Sample.Issues;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Controls.TestCases.Issues;

[Issue(IssueTracker.Github, 12500, "Shell does not always raise Navigating event on Windows", PlatformAffected.UWP)]
public class Issue12500 : Shell
{
	private NavigationViewModel _viewModel;
	public Issue12500()
	{
		this.FlyoutBehavior = FlyoutBehavior.Disabled;

		_viewModel = new NavigationViewModel();

		// Create TabBar
		var tabBar = new TabBar();

		// Add ShellContent for MainPage
		tabBar.Items.Add(new ShellContent
		{
			Title = "Hello, World!",
			Route = "MainPage",
			ContentTemplate = new DataTemplate(() => new Issue12500Main { BindingContext = _viewModel })
		});
		// Add ShellContent for EventsPage
		tabBar.Items.Add(new ShellContent
		{
			Title = "Events",
			Route = "EventPage",
			ContentTemplate = new DataTemplate(() => new Issue12500EventPage { BindingContext = _viewModel })
		});

		// Add TabBar to Shell
		this.Items.Add(tabBar);
	}
	protected override void OnNavigating(ShellNavigatingEventArgs args)
	{
		base.OnNavigating(args);
		string targetPageRoute = args.Target.Location.ToString();

		// Update ViewModel with new navigation text
		_viewModel.LabelText = $"Navigating to {targetPageRoute}";

	}
}
public class Issue12500EventPage : ContentPage
{
	[RequiresUnreferencedCode("Issue12500EventPage may require unreferenced code for data binding")]
	public Issue12500EventPage()
	{
		var label = new Label
		{
			AutomationId = "Issue12500EventPage",
			FontSize = 24,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
		label.SetBinding(Label.TextProperty, nameof(NavigationViewModel.LabelText)); // Bind to ViewModel

		Content = new VerticalStackLayout
		{
			Children = { label }
		};
	}
}

public class Issue12500Main : ContentPage
{
	[RequiresUnreferencedCode("Issue12500Main may require unreferenced code for data binding")]
	public Issue12500Main()
	{
		var label = new Label
		{
			AutomationId = "Issue12500MainPage",
			FontSize = 24,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
		label.SetBinding(Label.TextProperty, nameof(NavigationViewModel.LabelText)); // Bind to ViewModel

		Content = new VerticalStackLayout
		{
			Children = { label }
		};
	}
}


public class NavigationViewModel : INotifyPropertyChanged
{
	private string _labelText;

	public string LabelText
	{
		get => _labelText;
		set
		{
			if (_labelText != value)
			{
				_labelText = value;
				OnPropertyChanged(nameof(LabelText));
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}