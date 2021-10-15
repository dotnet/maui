using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Controls.DualScreen
{
	public static class HostBuilderExtensions
	{
		public static MauiAppBuilder UseDualScreen(this MauiAppBuilder builder)
		{
			//builder.Services.AddSingleton<IHostedService>();
#if ANDROID
			var consumer = new Consumer();
			AndroidX.Window.Java.Layout.WindowInfoRepositoryCallbackAdapter wir = null;

			builder.ConfigureLifecycleEvents(lc =>
			{
				lc.AddAndroid(android =>
				{
					android.OnConfigurationChanged((activity, configuration) =>
					{
					
					})
					.OnStart((activity) =>
					{
						wir.AddWindowLayoutInfoListener(runOnUiThreadExecutor(), consumer); // `consumer` is the IConsumer implementation
					})
					.OnStop((activity) =>
					{
						wir.RemoveWindowLayoutInfoListener(consumer);
					})
					.OnCreate((activity, bundle) =>
					{
						wir = new AndroidX.Window.Java.Layout.WindowInfoRepositoryCallbackAdapter(
						AndroidX.Window.Layout.WindowInfoRepository.Companion.GetOrCreate(
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
	public class Consumer : Java.Lang.Object, AndroidX.Core.Util.IConsumer
	{
		public void Accept(Java.Lang.Object windowLayoutInfo)
		{
			var newLayoutInfo = windowLayoutInfo as AndroidX.Window.Layout.WindowLayoutInfo;

			global::Android.Util.Log.Info("JWM2", "%%% LayoutStateChangeCallback.Accept");
			global::Android.Util.Log.Info("JWM2", newLayoutInfo.ToString());
		}
	}
#endif
}