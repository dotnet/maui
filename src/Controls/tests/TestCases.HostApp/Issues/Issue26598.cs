using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26598, "Tabbar disappears when navigating back from page with hidden TabBar in iOS", PlatformAffected.iOS)]
public class Issue26598 : TestShell
{
	TabBar tabBar = new TabBar();

	// Create first ShellContent
	ShellContent homeShellContent = new ShellContent
	{
		ContentTemplate = new DataTemplate(() => new Issue26598Home()),
		Title = "HomeTab",
		AutomationId = "Issue26598Home",
		Route = nameof(Issue26598Home)
	};

	// Create second ShellContent
	ShellContent recentShellContent = new ShellContent
	{
		ContentTemplate = new DataTemplate(() => new Issue26598Recent()),
		Title = "RecentTab",
		Route = nameof(Issue26598Recent)
	};

	protected override void Init()
	{
		Routing.RegisterRoute("Issue26598Inner", typeof(Issue26598Inner));
		Routing.RegisterRoute(nameof(Issue26589NonTab), typeof(Issue26589NonTab));
		tabBar.Items.Add(homeShellContent);
		tabBar.Items.Add(recentShellContent);
		Items.Add(tabBar);
	}

	public class Issue26598Home : ContentPage
	{
		VerticalStackLayout stackLayout;
		Button button;
		public Issue26598Home()
		{
			Title = "Home";
			HeightRequest = 200;
			stackLayout = new VerticalStackLayout();
			button = new Button()
			{
				Text = "Navigate to InnerTab",
				AutomationId = "NavigateToInnerTab",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
			};
			button.Clicked += Button_OnClicked;
			stackLayout.Add(button);
			Shell.SetTabBarIsVisible(this, false);
			this.Content = stackLayout;
		}

		private void Button_OnClicked(object sender, EventArgs e)
		{
			Shell.Current.GoToAsync(nameof(Issue26598Inner));
		}

	}

	public class Issue26598Inner : ContentPage
	{
		VerticalStackLayout stackLayout;
		Button button;
		public Issue26598Inner()
		{
			Title = "InnerTab";
			stackLayout = new VerticalStackLayout();
			button = new Button()
			{
				Text = "Navigate to TabBarPage",
				AutomationId = "NavigateToTabBarPage",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
			};
			button.Clicked += Button_OnClicked;
			stackLayout.Add(button);
			Shell.SetTabBarIsVisible(this, true);
			this.Content = stackLayout;
		}

		private void Button_OnClicked(object sender, EventArgs e)
		{
			Shell.Current.GoToAsync(nameof(Issue26589NonTab));
		}

	}

	public class Issue26598Recent : ContentPage
	{
		VerticalStackLayout stackLayout;
		Label label;
		public Issue26598Recent()
		{
			Title = "Recent";
			HeightRequest = 200;

			stackLayout = new VerticalStackLayout();
			label = new Label()
			{
				Text = "Page Loaded in Recent Tab",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,

			};
			stackLayout.Add(label);
			Shell.SetTabBarIsVisible(this, true);
			this.Content = stackLayout;
		}
	}
	public class Issue26589NonTab : ContentPage
	{
		VerticalStackLayout stackLayout;
		Label label1;
		public Issue26589NonTab()
		{
			Title = "NoTabBarPage";
			stackLayout = new VerticalStackLayout();
			label1 = new Label()
			{
				Text = "This is Non TabBarPage",
				AutomationId = "Issue26589NonTab",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
			};
			stackLayout.Add(label1);
			Shell.SetTabBarIsVisible(this, false);
			this.Content = stackLayout;
		}

	}
}
