#nullable enable
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
	partial class DeviceDisplayImplementation : IDeviceDisplay
	{
		OrientationEventListener? orientationListener;

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

		DisplayInfo GetMainDisplayInfo()
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
				rotation: CalculateRotation(display),
				rate: display?.RefreshRate ?? 0);
		}

		void StartScreenMetricsListeners()
		{
			orientationListener = new Listener(Application.Context, OnMainDisplayInfoChanged);
			orientationListener.Enable();
		}

		void StopScreenMetricsListeners()
		{
			orientationListener?.Disable();
			orientationListener?.Dispose();
			orientationListener = null;
		}

		static DisplayRotation CalculateRotation(Display? display) =>
			display?.Rotation switch
			{
				SurfaceOrientation.Rotation270 => DisplayRotation.Rotation270,
				SurfaceOrientation.Rotation180 => DisplayRotation.Rotation180,
				SurfaceOrientation.Rotation90 => DisplayRotation.Rotation90,
				SurfaceOrientation.Rotation0 => DisplayRotation.Rotation0,
				_ => DisplayRotation.Unknown,
			};

		static DisplayOrientation CalculateOrientation() =>
			Application.Context.Resources?.Configuration?.Orientation switch
			{
				Orientation.Landscape => DisplayOrientation.Landscape,
				Orientation.Portrait => DisplayOrientation.Portrait,
				Orientation.Square => DisplayOrientation.Portrait,
				_ => DisplayOrientation.Unknown
			};

		static Display? GetDefaultDisplay()
		{
			try
			{
				using var service = Application.Context.GetSystemService(Context.WindowService);
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
