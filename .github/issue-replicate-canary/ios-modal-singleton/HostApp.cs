#if IOS
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;
using Page = Microsoft.Maui.Controls.Page;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38361, "Shell singleton modal content remains visible after interactive dismissal", PlatformAffected.iOS)]
public class Issue38361 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;
		var routePrefix = $"Issue38361_{Guid.NewGuid():N}";
		var singletonRoute = $"{routePrefix}_Singleton";
		var transientRoute = $"{routePrefix}_Transient";
		var secondRoute = $"{routePrefix}_Second";
		var phase = new Label
		{
			AutomationId = "Issue38361Phase",
			Text = "Ready: transient control, then singleton",
			FontSize = 18
		};
		var openTransient = new Button
		{
			AutomationId = "Issue38361OpenTransient",
			Text = "Open transient control"
		};
		var openSingleton = new Button
		{
			AutomationId = "Issue38361OpenSingleton",
			Text = "Open singleton modal"
		};
		var root = new ContentPage
		{
			Title = "Home",
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 20,
				Children =
				{
					new Label
					{
						Text = "Open the transient control twice, then the singleton twice. Swipe each sheet down from its navigation bar. Reopened content must remain visible."
					},
					phase,
					openTransient,
					openSingleton
				}
			}
		};

		var registrations = new ServiceCollection();
		registrations.AddSingleton(_ => new SingletonPage(() => GoToAsync(secondRoute)));
		registrations.AddSingleton(provider => new GenericModalNavigationPage<SingletonPage>(provider.GetRequiredService<SingletonPage>()));
		registrations.AddSingleton(_ => new SecondPage());
		registrations.AddTransient(_ => new TransientPage(() => GoToAsync(secondRoute)));
		registrations.AddTransient(provider => new GenericModalNavigationPage<TransientPage>(provider.GetRequiredService<TransientPage>()));
		var services = registrations.BuildServiceProvider();

		GenericModalNavigationPage<SingletonPage>? firstSingleton = null;
		SingletonPage? firstSingletonPage = null;
		GenericModalNavigationPage<TransientPage>? previousTransient = null;
		var singletonOpens = 0;
		var transientOpens = 0;
		var activeStage = string.Empty;

		// Use real DI lifetimes without changing the HostApp's application-wide services.
		Routing.RegisterRoute(singletonRoute, new LocalRouteFactory(() =>
		{
			var modal = services.GetRequiredService<GenericModalNavigationPage<SingletonPage>>();
			var page = services.GetRequiredService<SingletonPage>();
			if (!ReferenceEquals(modal.CurrentPage, page) ||
				(firstSingleton is not null && !ReferenceEquals(firstSingleton, modal)) ||
				(firstSingletonPage is not null && !ReferenceEquals(firstSingletonPage, page)))
				throw new TimeoutException("The singleton route did not reuse the original wrapper and root page.");

			firstSingleton = modal;
			firstSingletonPage = page;
			activeStage = $"singleton open {++singletonOpens}";
			phase.Text = $"Opening {activeStage}";
			return modal;
		}));
		Routing.RegisterRoute(transientRoute, new LocalRouteFactory(() =>
		{
			var modal = services.GetRequiredService<GenericModalNavigationPage<TransientPage>>();
			if (previousTransient is not null &&
				(ReferenceEquals(previousTransient, modal) || ReferenceEquals(previousTransient.CurrentPage, modal.CurrentPage)))
				throw new TimeoutException("The transient control reused a wrapper or root page.");

			previousTransient = modal;
			activeStage = $"transient open {++transientOpens}";
			phase.Text = $"Opening {activeStage}";
			return modal;
		}));
		Routing.RegisterRoute(secondRoute, new LocalRouteFactory(() => services.GetRequiredService<SecondPage>()));

		root.Appearing += (_, _) =>
		{
			if (activeStage.Length > 0)
				phase.Text = $"Home after {activeStage}";
			openTransient.IsEnabled = true;
			openSingleton.IsEnabled = true;
		};
		openTransient.Clicked += async (_, _) =>
		{
			openTransient.IsEnabled = false;
			openSingleton.IsEnabled = false;
			await GoToAsync(transientRoute);
		};
		openSingleton.Clicked += async (_, _) =>
		{
			openTransient.IsEnabled = false;
			openSingleton.IsEnabled = false;
			await GoToAsync(singletonRoute);
		};
		HandlerChanging += (_, args) =>
		{
			if (args.OldHandler is not null && args.NewHandler is null)
			{
				Routing.UnRegisterRoute(singletonRoute);
				Routing.UnRegisterRoute(transientRoute);
				Routing.UnRegisterRoute(secondRoute);
				services.Dispose();
			}
		};
		AddContentPage(root, $"{routePrefix}_Home");
	}

	sealed class LocalRouteFactory : RouteFactory
	{
		readonly Func<Page> _create;

		public LocalRouteFactory(Func<Page> create) => _create = create;

		public override Element GetOrCreate() => _create();

		public override Element GetOrCreate(IServiceProvider services) => _create();
	}

	sealed class GenericModalNavigationPage<TPage> : NavigationPage where TPage : Page
	{
		public GenericModalNavigationPage(TPage page) : base(page)
		{
			Shell.SetPresentationMode(this, PresentationMode.Modal);
			On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.Automatic);
			HandlerProperties.SetDisconnectPolicy(this, HandlerDisconnectPolicy.Manual);
		}
	}

	class ModalContentPage : ContentPage
	{
		public ModalContentPage(string title, string contentId, string buttonId, Func<Task> navigateToSecond)
		{
			Title = title;
			Padding = 20;
			HandlerProperties.SetDisconnectPolicy(this, HandlerDisconnectPolicy.Manual);
			var secondButton = new Button
			{
				Text = "Navigate to 2nd page",
				AutomationId = buttonId,
				HorizontalOptions = LayoutOptions.Center
			};
			secondButton.Clicked += async (_, _) => await navigateToSecond();
			Content = new VerticalStackLayout
			{
				Spacing = 20,
				Children =
				{
					new Label
					{
						Text = "This content will only be shown the first time the user navigates to this page.",
						AutomationId = contentId,
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center
					},
					secondButton,
					new Label
					{
						Text = $"Instance {Guid.NewGuid():N}",
						AutomationId = "Issue38361Instance",
						FontSize = 12
					}
				}
			};
		}
	}

	sealed class SingletonPage : ModalContentPage
	{
		public SingletonPage(Func<Task> navigateToSecond)
			: base("SingletonPage", "Issue38361SingletonContent", "Issue38361SingletonSecond", navigateToSecond)
		{
		}
	}

	sealed class TransientPage : ModalContentPage
	{
		public TransientPage(Func<Task> navigateToSecond)
			: base("Transient control", "Issue38361TransientContent", "Issue38361TransientSecond", navigateToSecond)
		{
		}
	}

	sealed class SecondPage : ContentPage
	{
		public SecondPage()
		{
			Title = "SecondPage";
			HandlerProperties.SetDisconnectPolicy(this, HandlerDisconnectPolicy.Manual);
			Content = new VerticalStackLayout
			{
				Children =
				{
					new Label
					{
						Text = "Welcome to .NET MAUI!",
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center
					}
				}
			};
		}
	}
}
#endif
