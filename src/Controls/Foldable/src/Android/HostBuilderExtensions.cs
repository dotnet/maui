using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Graphics;
using AndroidX.Window.Layout;
using static Microsoft.Maui.Foldable.FoldableService;
using Android.App;

namespace Microsoft.Maui.Foldable
{
	public static partial class HostBuilderExtensions
	{
		static FoldableService GetFoldableService(Activity activity)
		{
			return activity
					.GetWindow()?
					.Handler?
					.MauiContext?
					.Services?.GetService<IFoldableContext>() as FoldableService;

		}

		public static MauiAppBuilder UseFoldable(this MauiAppBuilder builder)
		{
			builder.Services.AddScoped(typeof(IFoldableContext), (sp) => new FoldableService());
			builder.Services.AddScoped(typeof(IFoldableService),
				(sp) => sp.GetService<IFoldableContext>() as IFoldableService ?? new FoldableService());

			builder.ConfigureLifecycleEvents(lc =>
			{
				lc.AddAndroid(android =>
				{
					android.OnConfigurationChanged((activity, configuration) =>
					{
						GetFoldableService(activity)
							?.OnConfigurationChanged(activity, configuration);
					})
					.OnStart((activity) =>
					{
						GetFoldableService(activity)
							?.OnStart(activity);

					})
					.OnStop((activity) =>
					{
						GetFoldableService(activity)
							?.OnStop(activity);
					})
					.OnMauiContextCreated((context) =>
					{
						var activity = context.Context.GetActivity();
						var foldableService =
						(context.Services.GetService<IFoldableContext>()
							as FoldableService);

						if (activity != null && foldableService != null)
							foldableService.OnCreate(activity);
					})
					.OnResume((activity) =>
					{
						var foldableService =
							activity
								.GetWindow()?
								.Handler?
								.MauiContext?
								.Services?.GetService<IFoldableService>();

						DualScreenInfo.Current.SetFoldableService(foldableService);
					});
				});
			});

			return builder;
		}
	}
}