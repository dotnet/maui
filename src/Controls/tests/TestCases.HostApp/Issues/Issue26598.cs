using Microsoft.Maui.Controls;
#if MACCATALYST
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Platform.Compatibility;
using System.Text;
using UIKit;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26598, "Tabbar disappears when navigating back from page with hidden TabBar in iOS", PlatformAffected.iOS)]
public class Issue26598 : TestShell
{
	TabBar tabBar = new TabBar();
#if MACCATALYST
	IDisposable tabBarHiddenObserver;
	UITabBar observedTabBar;
#endif

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
#if MACCATALYST
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MAUI_LOG_FILE")))
		{
			Loaded += (_, _) =>
			{
				ObserveNativeTabBar();
				LogNativeTabBarState("Loaded");
			};
			Unloaded += (_, _) =>
			{
				tabBarHiddenObserver?.Dispose();
				tabBarHiddenObserver = null;
				observedTabBar = null;
			};
			Navigated += (_, _) =>
			{
				ObserveNativeTabBar();
				LogNativeTabBarState("Navigated");
				Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500),
					() => LogNativeTabBarState("After navigation"));
			};
		}
#endif
	}

#if MACCATALYST
	void ObserveNativeTabBar()
	{
		var controller = (Handler as IShellContext)?.CurrentShellItemRenderer?.ViewController as UITabBarController;
		var nativeTabBar = controller?.IsViewLoaded == true ? controller.TabBar : null;
		if (observedTabBar == nativeTabBar)
			return;

		tabBarHiddenObserver?.Dispose();
		tabBarHiddenObserver = null;
		observedTabBar = nativeTabBar;
		if (nativeTabBar is null)
			return;

		// Capture the caller that re-hides the bar, without correcting native state.
		tabBarHiddenObserver = nativeTabBar.Layer.AddObserver("hidden", NSKeyValueObservingOptions.OldNew, change =>
		{
			if (change.OldValue?.Equals(change.NewValue) == true)
				return;

			LogNativeTabBarState($"Native hidden changed {change.OldValue} -> {change.NewValue}\n" +
				$"Native stack:\n{string.Join("\n", NSThread.NativeCallStack)}\nManaged stack:\n{Environment.StackTrace}");
		});
	}

	void LogNativeTabBarState(string stage)
	{
		var context = Handler?.MauiContext ?? Application.Current?.Handler?.MauiContext;
		if (context is null)
		{
			Console.Error.WriteLine($"Issue26598 {stage}: no native context is available for diagnostics.");
			return;
		}

		var logger = context.Services.GetRequiredService<ILogger<Issue26598>>();
		var controller = (Handler as IShellContext)?.CurrentShellItemRenderer?.ViewController as UITabBarController;
		if (controller?.IsViewLoaded != true)
		{
			logger.LogInformation("Issue26598 {Stage}: page={Page}, controller={Controller}, view not loaded",
				stage, CurrentPage?.Title, controller?.GetType().Name);
			return;
		}

		// Observe the existing native hierarchy without forcing layout or changing visibility.
		var nativeTabBar = controller.TabBar;
		var navigation = controller.SelectedViewController as UINavigationController;
		var state = new StringBuilder();
		state.Append($"Issue26598 {stage}: page={CurrentPage?.Title}, " +
			$"ShowTabs={(CurrentItem as IShellItemController)?.ShowTabs}, " +
			$"TabBarHidden={(OperatingSystem.IsMacCatalystVersionAtLeast(18) ? controller.TabBarHidden.ToString() : "unavailable")}, " +
			$"Mode={(OperatingSystem.IsMacCatalystVersionAtLeast(18) ? controller.Mode.ToString() : "unavailable")}, " +
			$"selectedIndex={controller.SelectedIndex}, selectedItem={nativeTabBar?.SelectedItem?.Title}, " +
			$"nativeStackDepth={navigation?.ViewControllers?.Length}, top={navigation?.TopViewController?.Title}, " +
			$"HidesBottomBarWhenPushed={navigation?.TopViewController?.HidesBottomBarWhenPushed}; navigation stack: ");

		if (navigation?.ViewControllers is { } viewControllers)
		{
			foreach (var viewController in viewControllers)
				state.Append($"{viewController.GetType().Name}[Title={viewController.Title}, HidesBottomBarWhenPushed={viewController.HidesBottomBarWhenPushed}] -> ");
		}

		state.Append("ancestry: ");
		for (UIView current = nativeTabBar; current is not null; current = current.Superview)
		{
			state.Append($"{current.GetType().Name}[Hidden={current.Hidden}, Alpha={current.Alpha}, " +
				$"Frame={current.Frame}, Bounds={current.Bounds}, ClipsToBounds={current.ClipsToBounds}, " +
				$"LayerHidden={current.Layer.Hidden}, Opacity={current.Layer.Opacity}, PresentationOpacity={current.Layer.PresentationLayer?.Opacity}, " +
				$"Window={current.Window?.Handle}] -> ");
		}

		logger.LogInformation("{NativeTabBarState}", state.ToString());
	}
#endif

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
				AutomationId = "RecentTabContent",
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
