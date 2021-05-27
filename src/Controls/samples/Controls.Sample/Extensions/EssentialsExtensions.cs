using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Essentials
{
	public static class EssentialsExtensions
	{
		public static IAppHostBuilder ConfigureEssentials(this IAppHostBuilder builder, Action<IEssentialsBuilder> configureDelegate = null)
		{
			if (configureDelegate == null)
				builder.ConfigureEssentials((Action<HostBuilderContext, IEssentialsBuilder>)null);
			else
				builder.ConfigureEssentials((_, essentials) => configureDelegate(essentials));

			return builder;
		}

		public static IAppHostBuilder ConfigureEssentials(this IAppHostBuilder builder, Action<HostBuilderContext, IEssentialsBuilder> configureDelegate = null)
		{
			builder.ConfigureLifecycleEvents(life =>
			{
#if __ANDROID__
				Platform.Init(MauiApplication.Current);

				life.AddAndroid(android => android
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
					.OnLaunched((application, args) =>
					{
						Platform.OnLaunched(args);
					}));
#endif
			});

			if (configureDelegate != null)
				builder.ConfigureServices<EssentialsBuilder>(configureDelegate);

			return builder;
		}

		public static IEssentialsBuilder AddAppAction(this IEssentialsBuilder essentials, string id, string title, string subtitle = null, string icon = null) =>
			essentials.AddAppAction(new AppAction(id, title, subtitle, icon));

		class EssentialsBuilder : IEssentialsBuilder, IMauiServiceBuilder
		{
			readonly List<AppAction> _appActions = new List<AppAction>();
			Action<AppAction> _appActionHandlers;
			bool _trackVersions;

#pragma warning disable CS0414 // Remove unread private members
			bool _useLegaceSecureStorage;
			string _mapServiceToken;
#pragma warning restore CS0414 // Remove unread private members

			public IEssentialsBuilder UseMapServiceToken(string token)
			{
				_mapServiceToken = token;
				return this;
			}

			public IEssentialsBuilder AddAppAction(AppAction appAction)
			{
				_appActions.Add(appAction);
				return this;
			}

			public IEssentialsBuilder OnAppAction(Action<AppAction> action)
			{
				_appActionHandlers += action;
				return this;
			}

			public IEssentialsBuilder UseVersionTracking()
			{
				_trackVersions = true;
				return this;
			}

			public IEssentialsBuilder UseLegacySecureStorage()
			{
				_useLegaceSecureStorage = true;
				return this;
			}

			public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
			{
			}

			public async void Configure(HostBuilderContext context, IServiceProvider services)
			{
#if WINDOWS
				Platform.MapServiceToken = _mapServiceToken;
#elif __ANDROID__
				SecureStorage.LegacyKeyHashFallback = _useLegaceSecureStorage;
#endif

				AppActions.OnAppAction += HandleOnAppAction;

				try
				{
					await AppActions.SetAsync(_appActions);
				}
				catch (FeatureNotSupportedException ex)
				{
					services.GetService<ILoggerFactory>()?
						.CreateLogger<IEssentialsBuilder>()?
						.LogError(ex, "App Actions are not supported on this platform.");
				}

				if (_trackVersions)
					VersionTracking.Track();
			}

			void HandleOnAppAction(object sender, AppActionEventArgs e)
			{
				_appActionHandlers?.Invoke(e.AppAction);
			}
		}
	}
}