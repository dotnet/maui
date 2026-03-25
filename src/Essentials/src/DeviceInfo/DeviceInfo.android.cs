using System;
using Android.App;
using Android.Content.Res;
using Android.OS;
using Android.Provider;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
	class DeviceInfoImplementation : IDeviceInfo
	{
		const int tabletCrossover = 600;

		public string Model => Build.Model;

		public string Manufacturer => Build.Manufacturer;

		public string Name
		{
			get
			{
				// DEVICE_NAME added in System.Global in API level 25
				// https://developer.android.com/reference/android/provider/Settings.Global#DEVICE_NAME
				var name = GetSystemSetting("device_name", true);
				if (string.IsNullOrWhiteSpace(name))
					name = Model;
				return name;
			}
		}

		public string VersionString => Build.VERSION.Release;

		public Version Version => Utils.ParseVersion(VersionString);

		public DevicePlatform Platform => DevicePlatform.Android;

		public DeviceIdiom Idiom
		{
			get
			{
				var currentIdiom = DeviceIdiom.Unknown;

				// first try UIModeManager
				var uiModeManager = UiModeManager.FromContext(Application.Context);

				try
				{
					var uiMode = uiModeManager?.CurrentModeType ?? UiMode.TypeUndefined;
					currentIdiom = DetectIdiom(uiMode);
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Unable to detect using UiModeManager: {ex.Message}");
				}

				// then try Configuration
				if (currentIdiom == DeviceIdiom.Unknown)
				{
					var configuration = Application.Context.Resources?.Configuration;
					if (configuration != null)
					{
						var minWidth = configuration.SmallestScreenWidthDp;
						var isWide = minWidth >= tabletCrossover;
						currentIdiom = isWide ? DeviceIdiom.Tablet : DeviceIdiom.Phone;
					}
					else
					{
						// start clutching at straws
						var metrics = Application.Context.Resources?.DisplayMetrics;
						if (metrics != null)
						{
							var minSize = Math.Min(metrics.WidthPixels, metrics.HeightPixels);
							var isWide = minSize * metrics.Density >= tabletCrossover;
							currentIdiom = isWide ? DeviceIdiom.Tablet : DeviceIdiom.Phone;
						}
					}
				}

				// hope we got it somewhere
				return currentIdiom;
			}
		}

		static DeviceIdiom DetectIdiom(UiMode uiMode)
		{
			if (uiMode == UiMode.TypeNormal)
				return DeviceIdiom.Unknown;
			else if (uiMode == UiMode.TypeTelevision)
				return DeviceIdiom.TV;
			else if (uiMode == UiMode.TypeDesk)
				return DeviceIdiom.Desktop;
			else if (uiMode == UiMode.TypeWatch)
				return DeviceIdiom.Watch;

			return DeviceIdiom.Unknown;
		}

		public DeviceType DeviceType
		{
			get
			{
				var isEmulator =
					(Build.Brand.StartsWith("generic", StringComparison.Ordinal) && Build.Device.StartsWith("generic", StringComparison.Ordinal)) ||
					Build.Fingerprint.StartsWith("generic", StringComparison.Ordinal) ||
					Build.Fingerprint.StartsWith("unknown", StringComparison.Ordinal) ||
					Build.Hardware.Contains("goldfish", StringComparison.Ordinal) ||
					Build.Hardware.Contains("ranchu", StringComparison.Ordinal) ||
					Build.Model.Contains("google_sdk", StringComparison.Ordinal) ||
					Build.Model.Contains("Emulator", StringComparison.Ordinal) ||
					Build.Model.Contains("Android SDK built for x86", StringComparison.Ordinal) ||
					Build.Manufacturer.Contains("Genymotion", StringComparison.Ordinal) ||
					Build.Manufacturer.Contains("VS Emulator", StringComparison.Ordinal) ||
					Build.Product.Contains("emulator", StringComparison.Ordinal) ||
					Build.Product.Contains("google_sdk", StringComparison.Ordinal) ||
					Build.Product.Contains("sdk", StringComparison.Ordinal) ||
					Build.Product.Contains("sdk_google", StringComparison.Ordinal) ||
					Build.Product.Contains("sdk_x86", StringComparison.Ordinal) ||
					Build.Product.Contains("simulator", StringComparison.Ordinal) ||
					Build.Product.Contains("vbox86p", StringComparison.Ordinal);

				if (isEmulator)
					return DeviceType.Virtual;

				return DeviceType.Physical;
			}
		}

		static string GetSystemSetting(string name, bool isGlobal = false)
		{
			if (isGlobal && OperatingSystem.IsAndroidVersionAtLeast(25))
				return Settings.Global.GetString(Application.Context.ContentResolver, name);
			else
				return Settings.System.GetString(Application.Context.ContentResolver, name);
		}
	}
}
