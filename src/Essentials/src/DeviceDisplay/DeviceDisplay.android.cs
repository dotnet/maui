#nullable enable
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class DeviceDisplayImplementation : IDeviceDisplay
	{
		OrientationEventListener? orientationListener;

		public event EventHandler<DisplayInfoChangedEventArgs>? MainDisplayInfoChanged;

		public bool KeepScreenOn
		{
			get
			{
				var window = Platform.GetCurrentActivity(true)?.Window;
				var flags = window?.Attributes?.Flags ?? 0;
				return flags.HasFlag(WindowManagerFlags.KeepScreenOn);
			}

			set
			{
				var window = Platform.GetCurrentActivity(true)?.Window;
				if (value)
					window?.AddFlags(WindowManagerFlags.KeepScreenOn);
				else
					window?.ClearFlags(WindowManagerFlags.KeepScreenOn);
			}
		}

		public DisplayInfo GetMainDisplayInfo()
		{
			using var displayMetrics = new DisplayMetrics();
			var display = GetDefaultDisplay();
#pragma warning disable CS0618 // Type or member is obsolete
			display?.GetRealMetrics(displayMetrics);
#pragma warning restore CS0618 // Type or member is obsolete

			return new DisplayInfo(
				width: displayMetrics?.WidthPixels ?? 0,
				height: displayMetrics?.HeightPixels ?? 0,
				density: displayMetrics?.Density ?? 0,
				orientation: CalculateOrientation(),
				rotation: CalculateRotation(),
				rate: display?.RefreshRate ?? 0);
		}

		public void StartScreenMetricsListeners()
		{
			orientationListener = new Listener(Platform.AppContext, OnScreenMetricsChanged);
			orientationListener.Enable();
		}

		public void StopScreenMetricsListeners()
		{
			orientationListener?.Disable();
			orientationListener?.Dispose();
			orientationListener = null;
		}

		void OnScreenMetricsChanged()
		{
			var metrics = GetMainDisplayInfo();
			MainDisplayInfoChanged?.Invoke(this, new DisplayInfoChangedEventArgs(metrics));
		}

		DisplayRotation CalculateRotation()
		{
			var display = GetDefaultDisplay();

			return display?.Rotation switch
			{
				SurfaceOrientation.Rotation270 => DisplayRotation.Rotation270,
				SurfaceOrientation.Rotation180 => DisplayRotation.Rotation180,
				SurfaceOrientation.Rotation90 => DisplayRotation.Rotation90,
				SurfaceOrientation.Rotation0 => DisplayRotation.Rotation0,
				_ => DisplayRotation.Unknown,
			};
		}

		DisplayOrientation CalculateOrientation()
		{
			return Platform.AppContext.Resources?.Configuration?.Orientation switch
			{
				Orientation.Landscape => DisplayOrientation.Landscape,
				Orientation.Portrait => DisplayOrientation.Portrait,
				Orientation.Square => DisplayOrientation.Portrait,
				_ => DisplayOrientation.Unknown
			};
		}

		Display? GetDefaultDisplay()
		{
			try
			{
				using var service = Platform.AppContext.GetSystemService(Context.WindowService);
				using var windowManager = service?.JavaCast<IWindowManager>();
				return windowManager?.DefaultDisplay;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to get default display: {ex}");
				return null;
			}
		}

		class Listener : OrientationEventListener
		{
			readonly Action onChanged;

			internal Listener(Context context, Action handler)
				: base(context) => onChanged = handler;

			public override async void OnOrientationChanged(int orientation)
			{
				await Task.Delay(500);
				onChanged();
			}
		}
	}
}
