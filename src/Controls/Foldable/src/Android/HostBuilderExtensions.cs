using Android.App;
using AndroidX.Window.Layout;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using static Microsoft.Maui.Foldable.FoldableService;

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

		/// <summary>
		/// Configure the .NET MAUI app to listen for fold-related events
		/// in the Android lifecycle. Ensures <see cref="Microsoft.Maui.Controls.Foldable.TwoPaneView"/>
		/// can detect and layout around a hinge or screen fold.
		/// </summary>
		/// <remarks>
		/// Relies on Jetpack Window Manager to detect and respond to
		/// foldable device features and capabilities.
		/// </remarks>
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