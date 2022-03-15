using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Essentials
{
	public interface IEssentialsBuilder
	{
		IEssentialsBuilder UseMapServiceToken(string token);

		IEssentialsBuilder AddAppAction(AppAction appAction);

		IEssentialsBuilder OnAppAction(Action<AppAction> action);

		IEssentialsBuilder UseVersionTracking();
	}

	public static class EssentialsExtensions
	{
		internal static MauiAppBuilder UseEssentials(this MauiAppBuilder builder)
		{
			builder.ConfigureLifecycleEvents(life =>
			{
#if __ANDROID__
				Platform.Init(MauiApplication.Current);

				life.AddAndroid(android => android
					.OnCreate((activity, savedInstanceState) =>
					{
						Platform.Init(activity, savedInstanceState);
					})
					.OnRequestPermissionsResult((activity, requestCode, permissions, grantResults) =>
					{
						Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
					})
					.OnNewIntent((activity, intent) =>
					{
						Platform.OnNewIntent(intent);
					})
					.OnResume((activity) =>
					{
						Platform.OnResume();
					}));
#elif __IOS__
				life.AddiOS(ios => ios
					.ContinueUserActivity((application, userActivity, completionHandler) =>
					{
						return Platform.ContinueUserActivity(application, userActivity, completionHandler);
					})
					.OpenUrl((application, url, options) =>
					{
						return Platform.OpenUrl(application, url, options);
					})
					.PerformActionForShortcutItem((application, shortcutItem, completionHandler) =>
					{
						Platform.PerformActionForShortcutItem(application, shortcutItem, completionHandler);
					}));
#elif WINDOWS
				life.AddWindows(windows => windows
					.OnPlatformMessage((window, args) =>
					{
						Platform.NewWindowProc(args.Hwnd, args.MessageId, args.WParam, args.LParam);
					})
					.OnActivated((window, args) =>
					{
						Platform.OnActivated(window, args);
					})
					.OnLaunched((application, args) =>
					{
						Platform.OnLaunched(args);
					}));
#endif
			});

			return builder;
		}

		public static MauiAppBuilder ConfigureEssentials(this MauiAppBuilder builder, Action<IEssentialsBuilder>? configureDelegate = null)
		{
			if (configureDelegate != null)
			{
				builder.Services.AddSingleton<EssentialsRegistration>(new EssentialsRegistration(configureDelegate));
			}
			builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService, EssentialsInitializer>());

			return builder;
		}

		public static IEssentialsBuilder AddAppAction(this IEssentialsBuilder essentials, string id, string title, string? subtitle = null, string? icon = null) =>
			essentials.AddAppAction(new AppAction(id, title, subtitle, icon));

		internal class EssentialsRegistration
		{
			private readonly Action<IEssentialsBuilder> _registerEssentials;

			public EssentialsRegistration(Action<IEssentialsBuilder> registerEssentials)
			{
				_registerEssentials = registerEssentials;
			}

			internal void RegisterEssentialsOptions(IEssentialsBuilder essentials)
			{
				_registerEssentials(essentials);
			}
		}

		class EssentialsInitializer : IMauiInitializeService
		{
			private readonly IEnumerable<EssentialsRegistration> _essentialsRegistrations;
			private EssentialsBuilder? _essentialsBuilder;

			public EssentialsInitializer(IEnumerable<EssentialsRegistration> essentialsRegistrations)
			{
				_essentialsRegistrations = essentialsRegistrations;
			}

			public async void Initialize(IServiceProvider services)
			{
				_essentialsBuilder = new EssentialsBuilder();
				if (_essentialsRegistrations != null)
				{
					foreach (var essentialsRegistration in _essentialsRegistrations)
					{
						essentialsRegistration.RegisterEssentialsOptions(_essentialsBuilder);
					}
				}

#if WINDOWS
				Platform.MapServiceToken = _essentialsBuilder.MapServiceToken;
#endif

				AppActions.OnAppAction += HandleOnAppAction;

				try
				{
					await AppActions.SetAsync(_essentialsBuilder.AppActions);
				}
				catch (FeatureNotSupportedException ex)
				{
					services.GetService<ILoggerFactory>()?
						.CreateLogger<IEssentialsBuilder>()?
						.LogError(ex, "App Actions are not supported on this platform.");
				}

				if (_essentialsBuilder.TrackVersions)
					VersionTracking.Track();
			}

			void HandleOnAppAction(object? sender, AppActionEventArgs e)
			{
				_essentialsBuilder?.AppActionHandlers?.Invoke(e.AppAction);
			}
		}

		class EssentialsBuilder : IEssentialsBuilder
		{
			internal readonly List<AppAction> AppActions = new List<AppAction>();
			internal Action<AppAction>? AppActionHandlers;
			internal bool TrackVersions;

#pragma warning disable CS0414 // Remove unread private members
			internal string? MapServiceToken;
#pragma warning restore CS0414 // Remove unread private members

			public IEssentialsBuilder UseMapServiceToken(string token)
			{
				MapServiceToken = token;
				return this;
			}

			public IEssentialsBuilder AddAppAction(AppAction appAction)
			{
				AppActions.Add(appAction);
				return this;
			}

			public IEssentialsBuilder OnAppAction(Action<AppAction> action)
			{
				AppActionHandlers += action;
				return this;
			}

			public IEssentialsBuilder UseVersionTracking()
			{
				TrackVersions = true;
				return this;
			}
		}
	}
}