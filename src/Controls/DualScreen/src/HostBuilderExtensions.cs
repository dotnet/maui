using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using System;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Graphics;

#if ANDROID
using Android.Runtime;
using AndroidX.Window.Layout;
using static Microsoft.Maui.Controls.DualScreen.DualScreenService;
#endif

namespace Microsoft.Maui.Controls.DualScreen
{
	public static class HostBuilderExtensions
	{
		public static MauiAppBuilder UseDualScreen(this MauiAppBuilder builder)
		{
#if ANDROID
			builder.Services.AddScoped(typeof(IFoldableContext), typeof(DualScreenServiceImpl));

			var consumer = new Consumer();
			AndroidX.Window.Java.Layout.WindowInfoTrackerCallbackAdapter wit = null;
			IWindowMetricsCalculator wmc = null;
			float screenDensity = 1f;

			builder.ConfigureLifecycleEvents(lc =>
			{
				lc.AddAndroid(android =>
				{
					android.OnConfigurationChanged((activity, configuration) =>
					{
						global::Android.Util.Log.Debug("JWM2", "~~~ HostBuilder.OnConfigurationChanged");
						// set window size after rotation
						var bounds = wmc.ComputeCurrentWindowMetrics(activity).Bounds;
						global::Android.Util.Log.Debug("JWM2", $"~~~                               bounds:{bounds}");
						var rect = new Rectangle(bounds.Left, bounds.Top, bounds.Width(), bounds.Height());
						consumer.SetWindowSize(rect);
					})
					.OnStart((activity) =>
					{
						global::Android.Util.Log.Debug("JWM2", "~~~ HostBuilder.OnStart");

						var foldContext = activity.GetWindow().Handler.MauiContext.Services.GetService(
								typeof(IFoldableContext)) as IFoldableContext;

						screenDensity = activity?.Resources?.DisplayMetrics?.Density ?? 1;
						foldContext.ScreenDensity = screenDensity;

						consumer.SetFoldableContext(foldContext); // so that we can update it on each message
																  
						// HACK: set window size first time - confirm if required
						var bounds = wmc.ComputeCurrentWindowMetrics(activity).Bounds;
						global::Android.Util.Log.Debug("JWM2", $"---                               bounds:{bounds}");
						var rect = new Rectangle(bounds.Left, bounds.Top, bounds.Width(), bounds.Height());
						consumer.SetWindowSize(rect);

						// HACK: Not sure this is the best way to pass info
						Microsoft.Maui.Controls.DualScreen.DualScreenService.Init(foldContext, activity);

						wit.AddWindowLayoutInfoListener(activity, runOnUiThreadExecutor(), consumer); // `consumer` is the IConsumer implementation

						global::Android.Util.Log.Debug("JWM2", $"~~~ HostBuilder.OnStart foldContext:{foldContext}");
					})
					.OnStop((activity) =>
					{
						wit.RemoveWindowLayoutInfoListener(consumer);
					})
					.OnCreate((activity, bundle) =>
					{
						wit = new AndroidX.Window.Java.Layout.WindowInfoTrackerCallbackAdapter(
						AndroidX.Window.Layout.WindowInfoTracker.Companion.GetOrCreate(
							activity));

						wmc = WindowMetricsCalculator.Companion.OrCreate; // source method `getOrCreate` is munged by auto-binding
					});
				});
			});
#endif

			return builder;
		}

#if ANDROID
		#region Used by WindowInfoTracker callback
		static Java.Util.Concurrent.IExecutor runOnUiThreadExecutor()
		{
			return new MyExecutor();
		}
		class MyExecutor : Java.Lang.Object, Java.Util.Concurrent.IExecutor
		{
			global::Android.OS.Handler handler = new global::Android.OS.Handler(global::Android.OS.Looper.MainLooper);
			public void Execute(Java.Lang.IRunnable r)
			{
				handler.Post(r);
			}
		}
		#endregion
#endif
	}
#if ANDROID
	public class Consumer : Java.Lang.Object, AndroidX.Core.Util.IConsumer
	{
		/// <summary>
		/// reference to context that is passed via dependencyservice...
		/// </summary>
		IFoldableContext foldableInfo;
		Rectangle WindowBounds;

		public void SetWindowSize(Rectangle size)
		{
			WindowBounds = size;
		}
		public void Accept(Java.Lang.Object windowLayoutInfo)
		{
			var newLayoutInfo = windowLayoutInfo as AndroidX.Window.Layout.WindowLayoutInfo;
			
			if (newLayoutInfo == null)
			{
				global::Android.Util.Log.Info("JWM2", "^^^ LayoutStateChangeCallback.Accept windowLayoutInfo was NULL");
				return;
			}

			global::Android.Util.Log.Info("JWM2", "%%% LayoutStateChangeCallback.Accept");
			global::Android.Util.Log.Info("JWM2", "%%% " + newLayoutInfo.ToString());

			var isSeparating = false; // we don't know if we'll find a displayFeature of not
			var foldingFeatureBounds = Rectangle.Zero;

			foreach (var displayFeature in newLayoutInfo.DisplayFeatures)
			{
				var foldingFeature = displayFeature.JavaCast<IFoldingFeature>();

				if (foldingFeature != null) // HACK: requires JavaCast as shown above
				{
					isSeparating = foldingFeature.IsSeparating;

					foldingFeatureBounds = new Rectangle(foldingFeature.Bounds.Left, foldingFeature.Bounds.Top,
														foldingFeature.Bounds.Width(), foldingFeature.Bounds.Height());

					global::Android.Util.Log.Info("JWM2", "\n    IsSeparating: " + foldingFeature.IsSeparating
							+ "\n    Orientation: " + foldingFeature.Orientation  // FoldingFeature.OrientationVertical or Horizontal
							+ "\n    State: " + foldingFeature.State // FoldingFeature.StateFlat or StateHalfOpened
					);
				}
				else
				{
					global::Android.Util.Log.Info("JWM2", "DisplayFeature is not a fold or hinge (shouldn't happen currently)");
				}
			}
			
			foldableInfo.isSeparating = isSeparating;
			foldableInfo.FoldingFeatureBounds = foldingFeatureBounds;
			foldableInfo.WindowBounds = WindowBounds; // HACK: also invokes FoldingFeatureChanged
		}

		/// <summary>
		/// Make the foldableContext available to receive data when fold/posture changes
		/// </summary>
		public void SetFoldableContext (object foldableContext) {
			foldableInfo = foldableContext as IFoldableContext;
			if (foldableInfo is null)
			{
				global::Android.Util.Log.Info("JWM2", "^^^ .SetFoldableContext foldableContext was NULL");
				throw new ArgumentNullException(nameof(foldableContext));
			}
		}
	}
#endif
}