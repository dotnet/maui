using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui;

using AndroidX.Window.Layout;
using AndroidX.Window.Java.Layout;
using Java.Interop;

namespace Maui.Controls.Sample.Droid
{
	[Activity(
		Label = "@string/app_name",
		Theme = "@style/Maui.SplashTheme",
		MainLauncher = true,
		ConfigurationChanges = ConfigChanges.ScreenSize 
		| ConfigChanges.Orientation 
		| ConfigChanges.ScreenLayout 
		| ConfigChanges.UiMode
		| ConfigChanges.SmallestScreenSize
		| ConfigChanges.KeyboardHidden
		)]
	[IntentFilter(
		new[] { Microsoft.Maui.Essentials.Platform.Intent.ActionAppAction },
		Categories = new[] { Android.Content.Intent.CategoryDefault })]
	public class MainActivity : MauiAppCompatActivity, AndroidX.Core.Util.IConsumer
	{
		WindowInfoRepositoryCallbackAdapter wir;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			Android.Util.Log.Debug("JWM", "Activity.OnCreate - DualScreenService.Init");
			Microsoft.Maui.Controls.DualScreen.DualScreenService.Init(this);
			base.OnCreate(savedInstanceState);

			wir = new WindowInfoRepositoryCallbackAdapter(WindowInfoRepository.Companion.GetOrCreate(this));
		}

		protected override void OnStart()
		{
			base.OnStart();
			wir.AddWindowLayoutInfoListener(runOnUiThreadExecutor(), this); // `this` is the IConsumer implementation
		}

		protected override void OnStop()
		{
			base.OnStop();
			wir.RemoveWindowLayoutInfoListener(this);
		}

		#region Used by WindowInfoRepository callback
		Java.Util.Concurrent.IExecutor runOnUiThreadExecutor()
		{
			return new MyExecutor();
		}
		class MyExecutor : Java.Lang.Object, Java.Util.Concurrent.IExecutor
		{
			Handler handler = new Handler(Looper.MainLooper);
			public void Execute(Java.Lang.IRunnable r)
			{
				handler.Post(r);
			}
		}

		public void Accept(Java.Lang.Object windowLayoutInfo)  // Object will be WindowLayoutInfo
		{
			var newLayoutInfo = windowLayoutInfo as WindowLayoutInfo;

			Android.Util.Log.Info("JWM", "===LayoutStateChangeCallback.Accept");
			Android.Util.Log.Info("JWM", newLayoutInfo.ToString());

			foreach (var displayFeature in newLayoutInfo.DisplayFeatures)
			{
				var foldingFeature = displayFeature.JavaCast<IFoldingFeature>();

				if (foldingFeature != null) // HACK: requires JavaCast as shown above
				{
					Android.Util.Log.Info("JWM", "\nIsSeparating: " + foldingFeature.IsSeparating
							+ "\nOrientation: " + foldingFeature.Orientation  // FoldingFeature.OrientationVertical or Horizontal
							+ "\nState: " + foldingFeature.State // FoldingFeature.StateFlat or StateHalfOpened
					);
				}
				else
				{
					Android.Util.Log.Info("JWM", "DisplayFeature is not a fold or hinge (shouldn't happen currently)");
				}
			}
		}
		#endregion
	}
}