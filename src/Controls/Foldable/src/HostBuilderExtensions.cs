using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using System;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Graphics;

#if ANDROID
using Android.Runtime;
using AndroidX.Window.Layout;
using Microsoft.Maui.Foldable;
using static Microsoft.Maui.Foldable.FoldableService;
#endif

namespace Microsoft.Maui.Foldable
{
	public static class HostBuilderExtensions
	{
		public static MauiAppBuilder UseFoldable(this MauiAppBuilder builder)
		{
#if ANDROID
			builder.Services.AddScoped(typeof(IFoldableContext), typeof(FoldableServiceImpl));

			var consumer = new Consumer();
			AndroidX.Window.Java.Layout.WindowInfoTrackerCallbackAdapter wit = null; // for hinge/fold
			IWindowMetricsCalculator wmc = null; // for window dimensions
			IFoldableContext foldContext; // DualScreenServiceImpl instance
			float screenDensity = 1f; // for converting px to dp

			builder.ConfigureLifecycleEvents(lc =>
			{
				lc.AddAndroid(android =>
				{
					android.OnConfigurationChanged((activity, configuration) =>
					{
						// set window size after rotation
						var bounds = wmc.ComputeCurrentWindowMetrics(activity).Bounds;
						var rect = new Rect(bounds.Left, bounds.Top, bounds.Width(), bounds.Height());
						consumer.SetWindowSize(rect);
					})
					.OnStart((activity) =>
					{
						foldContext = activity.GetWindow().Handler.MauiContext.Services.GetService(
								typeof(IFoldableContext)) as IFoldableContext; // DualScreenServiceImpl instance

						screenDensity = activity?.Resources?.DisplayMetrics?.Density ?? 1;
						foldContext.ScreenDensity = screenDensity; // assuming this never changes

						consumer.SetFoldableContext(foldContext); // so that we can update it on each message

						// set window size first time - sets WindowBounds
						var bounds = wmc.ComputeCurrentWindowMetrics(activity).Bounds;
						var rect = new Rect(bounds.Left, bounds.Top, bounds.Width(), bounds.Height());
						consumer.SetWindowSize(rect);


						// Adds to DependencyService here in the Init too
						Microsoft.Maui.Foldable.FoldableService.Init(foldContext, activity);

						wit.AddWindowLayoutInfoListener(activity, runOnUiThreadExecutor(), consumer); // `consumer` is the IConsumer implementation declared below
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

						wmc = WindowMetricsCalculator.Companion.OrCreate; // source method `getOrCreate` is munged by NuGet auto-binding
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
		Rect WindowBounds;

		public void SetWindowSize(Rect size)
		{
			WindowBounds = size;
			foldableInfo.WindowBounds = size;
		}
		public void Accept(Java.Lang.Object windowLayoutInfo)
		{
			var newLayoutInfo = windowLayoutInfo as AndroidX.Window.Layout.WindowLayoutInfo;
			
			if (newLayoutInfo == null)
			{
				global::Android.Util.Log.Info("JWM", "LayoutStateChangeCallback.Accept windowLayoutInfo was NULL");
				return;
			}

			var isSeparating = false; // we don't know if we'll find a displayFeature of not
			var foldingFeatureBounds = Rect.Zero;

			foreach (var displayFeature in newLayoutInfo.DisplayFeatures)
			{
				var foldingFeature = displayFeature.JavaCast<IFoldingFeature>();

				if (foldingFeature != null) // requires JavaCast as shown above, since DisplayFeatures collection might have mulitple types
				{
					isSeparating = foldingFeature.IsSeparating;

					foldingFeatureBounds = new Rect(foldingFeature.Bounds.Left, foldingFeature.Bounds.Top,
														foldingFeature.Bounds.Width(), foldingFeature.Bounds.Height());

					global::Android.Util.Log.Info("JWM2", "\n    IsSeparating: " + foldingFeature.IsSeparating
							+ "\n    Orientation: " + foldingFeature.Orientation  // FoldingFeature.OrientationVertical or Horizontal
							+ "\n    State: " + foldingFeature.State // FoldingFeature.StateFlat or StateHalfOpened
					);
				}
				else
				{
					global::Android.Util.Log.Info("JWM2", "DisplayFeature is not a fold or hinge (could be a cut-out)");
				}
			}
			
			foldableInfo.IsSeparating = isSeparating;// also invokes FoldingFeatureChanged
			foldableInfo.FoldingFeatureBounds = foldingFeatureBounds;// also invokes FoldingFeatureChanged
			foldableInfo.WindowBounds = WindowBounds; // also invokes FoldingFeatureChanged
		}

		/// <summary>
		/// Make the foldableContext available to receive data when fold/posture changes
		/// </summary>
		public void SetFoldableContext (object foldableContext) {
			foldableInfo = foldableContext as IFoldableContext;
			if (foldableInfo is null)
			{
				throw new ArgumentNullException(nameof(foldableContext));
			}
		}
	}
#endif
}