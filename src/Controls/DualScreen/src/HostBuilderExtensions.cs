using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			AndroidX.Window.Java.Layout.WindowInfoTrackerCallbackAdapter wir = null;

			builder.ConfigureLifecycleEvents(lc =>
			{
				lc.AddAndroid(android =>
				{
					android.OnConfigurationChanged((activity, configuration) =>
					{
						global::Android.Util.Log.Debug("JWM", "~~~ HostBuilder.ConfigurationChanged");
					})
					.OnStart((activity) =>
					{
						consumer.SetFoldableContext(
							activity.GetWindow().Handler.MauiContext.Services.GetService(
								typeof(IFoldableContext)));

						// FUTURE USE
						wir.AddWindowLayoutInfoListener(activity, runOnUiThreadExecutor(), consumer); // `consumer` is the IConsumer implementation
					})
					.OnStop((activity) =>
					{
						// FUTURE USE
						wir.RemoveWindowLayoutInfoListener(consumer);
					})
					.OnCreate((activity, bundle) =>
					{
						// FUTURE USE
						wir = new AndroidX.Window.Java.Layout.WindowInfoTrackerCallbackAdapter(
						AndroidX.Window.Layout.WindowInfoTracker.Companion.GetOrCreate(
							activity));
					});
				});
			});


#endif

			return builder;
		}

#if ANDROID
		#region Used by WindowInfoRepository callback
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
	// FUTURE USE
	public class Consumer : Java.Lang.Object, AndroidX.Core.Util.IConsumer
	{
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
			var FoldingFeatureBounds = Rectangle.Zero;

			foreach (var displayFeature in newLayoutInfo.DisplayFeatures)
			{
				var foldingFeature = displayFeature.JavaCast<IFoldingFeature>();

				if (foldingFeature != null) // HACK: requires JavaCast as shown above
				{
					isSeparating = foldingFeature.IsSeparating;

					FoldingFeatureBounds = new Rectangle(foldingFeature.Bounds.Left, foldingFeature.Bounds.Top,
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
			global::Android.Util.Log.Info("JWM2", "=== FoldingFeatureChanged?.Invoke");
			
			_foldableContext.FoldingFeatureBounds = FoldingFeatureBounds;
			//_foldableContext.FoldingFeatureChanged?.Invoke(this, new Microsoft.Maui.Controls.DualScreen.FoldEventArgs()
			//{
			//	isSeparating = isSeparating,
			//	FoldingFeatureBounds = FoldingFeatureBounds,
			//	WindowBounds = WindowBounds
			//});
		}

		IFoldableContext _foldableContext;
		public void SetFoldableContext (object foldableContext) {
			_foldableContext = foldableContext as IFoldableContext;
			if (_foldableContext is null)
			{
				global::Android.Util.Log.Info("JWM2", "^^^ .SetFoldableContext foldableContext was NULL");
				throw new ArgumentNullException(nameof(foldableContext));
			}
		}
		
	}
#endif
}