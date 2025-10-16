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
			Content = new MainPage1(),
			Route = "MainPage"
		};

		var tabBar = new TabBar();
		tabBar.Items.Add(mainPage);
		Items.Add(tabBar);

		// Register routes for navigation
		Routing.RegisterRoute("Page1", typeof(Page1));
		Routing.RegisterRoute("Page2", typeof(Page2));
		Routing.RegisterRoute("Page3", typeof(Page3));
		Routing.RegisterRoute("ModalPage", typeof(ModalPage));
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

public partial class MainPage1 : ContentPage
{
	public Command NavigateToPage1Command { get; }

	public MainPage1()
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
		await Shell.Current.GoToAsync("Page1");
	}
}

public class Page1 : ContentPage, INavigationAware
{
	bool _wasModalShown = false;

	public Command OpenModalCommand { get; }
	public Command NavigateToPage2Command { get; }

	public Page1()
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
		await Shell.Current.GoToAsync("ModalPage");
	}

	async Task NavigateToPage2Async()
	{
		await Shell.Current.GoToAsync("Page2");
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

public class ModalPage : ContentPage
{
	public Command CloseModalCommand { get; }

	public ModalPage()
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

public class Page2 : ContentPage
{
	public Command NavigateToPage3Command { get; }

	public Page2()
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
		await Shell.Current.GoToAsync("Page3");
	}
}

public class Page3 : ContentPage
{
	public Page3()
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
