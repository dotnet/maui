using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Essentials
{
	public static class EssentialsExtensions
	{
		public static IAppHostBuilder ConfigureEssentials(this IAppHostBuilder builder, Action<HostBuilderContext, IEssentialsBuilder> configureDelegate = null)
		{
			builder.ConfigureLifecycleEvents((ctx, life) =>
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
#endif
			});

			if (configureDelegate != null)
				builder.ConfigureServices<EssentialsBuilder>(configureDelegate);

			return builder;
		}

		public static IEssentialsBuilder AddAppAction(this IEssentialsBuilder essentials, string id, string title, string subtitle = null, string icon = null) =>
			essentials.AddAppAction(new AppAction(id, title, subtitle, icon));

		class EssentialsBuilder : IEssentialsBuilder, IServiceCollectionBuilder
		{
			readonly List<AppAction> _appActions = new List<AppAction>();
			Action<AppAction> _appActionHandlers;
			bool _trackVersions;

			bool _useLegaceSecureStorage;
			string _mapServiceToken;

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

			public void Build(IServiceCollection services)
			{
			}

			public async void Configure(IServiceProvider services)
			{
#if WINDOWS
				// Platform.MapServiceToken = _mapServiceToken;
#elif __ANDROID__
				SecureStorage.LegacyKeyHashFallback = _useLegaceSecureStorage;
#endif

				AppActions.OnAppAction += HandleOnAppAction;

				await AppActions.SetAsync(_appActions);

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