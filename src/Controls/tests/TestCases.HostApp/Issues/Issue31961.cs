using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

// Interface for navigation awareness
public interface INavigationAware
{
    void OnShellNavigated(ShellNavigatedEventArgs args);
}

[Issue(IssueTracker.Github, 31961, "[iOS] App crash with NullReferenceException in ShellSectionRenderer", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue31961 : Shell
{
	public Issue31961()
	{
		var mainPage = new ShellContent
		{
			Title = "Main",
			Content = new Issue31961MainPage(),
			Route = "MainPage"
		};

		var tabBar = new TabBar();
		tabBar.Items.Add(mainPage);
		Items.Add(tabBar);

		// Register routes for navigation
		Routing.RegisterRoute("Issue31961FirstPage", typeof(Issue31961FirstPage));
		Routing.RegisterRoute("Issue31961SecondPage", typeof(Issue31961SecondPage));
		Routing.RegisterRoute("Issue31961ThirdPage", typeof(Issue31961ThirdPage));
		Routing.RegisterRoute("Issue31961ModalPage", typeof(Issue31961ModalPage));
	}

	protected override void OnNavigated(ShellNavigatedEventArgs args)
	{
		if (Current.CurrentPage?.BindingContext is INavigationAware bindingContext)
		{
			bindingContext.OnShellNavigated(args);
		}

		base.OnNavigated(args);
	}
}

public partial class Issue31961MainPage : ContentPage
{
	public Command NavigateToPage1Command { get; }

	public Issue31961MainPage()
	{
		// Initialize command
		NavigateToPage1Command = new Command(async () => await NavigateToPage1Async());

		var btnNavigate = new Button
		{
			Text = "Go to Page 1",
			FontSize = 20,
			AutomationId = "MainPage",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Command = NavigateToPage1Command
		};

		Content = btnNavigate;
	}

	async Task NavigateToPage1Async()
	{
		await Shell.Current.GoToAsync("Issue31961FirstPage");
	}
}

public class Issue31961FirstPage : ContentPage, INavigationAware
{
	bool _wasModalShown = false;

	public Command OpenModalCommand { get; }
	public Command NavigateToPage2Command { get; }

	public Issue31961FirstPage()
	{
		Title = "Page 1";
		BindingContext = this; // Set binding context to enable INavigationAware

		// Initialize commands
		OpenModalCommand = new Command(async () => await OpenModalAsync());
		NavigateToPage2Command = new Command(async () => await NavigateToPage2Async());

		var btnOpenModal = new Button { Text = "Open Modal Page", AutomationId = "OpenModalButton", Command = OpenModalCommand };
		var btnGoToPage2 = new Button { Text = "Go to Page 2", Command = NavigateToPage2Command };

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children = { btnOpenModal, btnGoToPage2 }
		};
	}

	async Task OpenModalAsync()
	{
		_wasModalShown = true;
		await Shell.Current.GoToAsync("Issue31961ModalPage");
	}

	async Task NavigateToPage2Async()
	{
		await Shell.Current.GoToAsync("Issue31961SecondPage");
	}

	public void OnShellNavigated(ShellNavigatedEventArgs args)
	{
		// This will be called when Shell navigation occurs
		if (_wasModalShown)
		{
			_wasModalShown = false; // Reset the flag
			// Navigate to Page2 only after modal action is completed
			NavigateToPage2Command.Execute(null);
		}
	}
}

public class Issue31961ModalPage : ContentPage
{
	public Command CloseModalCommand { get; }

	public Issue31961ModalPage()
	{
		Title = "Modal Page";
		BackgroundColor = Colors.LightGray;

		// Initialize command
		CloseModalCommand = new Command(async () => await CloseModalAsync());

		var btnClose = new Button { Text = "Close Modal Page", AutomationId = "CloseModalButton", Command = CloseModalCommand };

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children = { new Label { Text = "This is a Modal Page" }, btnClose }
		};
	}

	async Task CloseModalAsync()
	{
		await Shell.Current.GoToAsync("..");
	}
}

public class Issue31961SecondPage : ContentPage
{
	public Command NavigateToPage3Command { get; }

	public Issue31961SecondPage()
	{
		Title = "Page 2";

		// Initialize command
		NavigateToPage3Command = new Command(async () => await NavigateToPage3Async());

		var btnGoToPage3 = new Button { Text = "Go to Page 3", AutomationId = "Page2" , Command = NavigateToPage3Command };

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children = { btnGoToPage3 }
		};
	}

	async Task NavigateToPage3Async()
	{
		await Shell.Current.GoToAsync("Issue31961ThirdPage");
	}
}

public class Issue31961ThirdPage : ContentPage
{
	public Issue31961ThirdPage()
	{
		Title = "Page 3";

		var label = new Label
		{
			Text = "Welcome to Page 3",
			AutomationId = "Page3",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		Content = label;
	}
}
