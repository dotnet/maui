#if IOS_DUO_BINDINGS
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Foldable
{
	public static partial class HostBuilderExtensions
	{
		/// <summary>
		/// Configures the app to detect and respond to foldable device hinge positions and screen configurations.
		/// </summary>
		/// <param name="builder">The <see cref="MauiAppBuilder"/> to configure.</param>
		/// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
		public static MauiAppBuilder UseFoldable(this MauiAppBuilder builder)
		{
			builder.Services.AddScoped<IFoldableService, FoldableService>();
			builder.ConfigureLifecycleEvents(lifecycle =>
			{
				lifecycle.AddiOS(ios =>
				{
					ios.OnActivated(application =>
					{
						SetCurrentWindow(application.KeyWindow);
					});

					if (!OperatingSystem.IsIOSVersionAtLeast(13))
						return;

					ios.SceneOnActivated(scene =>
					{
						if (scene.Delegate is IUIWindowSceneDelegate sceneDelegate)
							SetCurrentWindow(sceneDelegate.GetWindow());
					});
				});
			});

			return builder;
		}

		static void SetCurrentWindow(UIWindow window)
		{
			var service = window?
				.GetWindow()?
				.Handler?
				.MauiContext?
				.Services?
				.GetService<IFoldableService>() as FoldableService;

			if (service == null)
				return;

			service.SetWindow(window);
			DualScreenInfo.Current.SetFoldableService(service);
		}
	}
}
#endif
