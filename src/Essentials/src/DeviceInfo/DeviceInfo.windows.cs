using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Maui.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;

namespace Microsoft.Maui.Devices
{
	class DeviceInfoImplementation : IDeviceInfo
	{
		readonly EasClientDeviceInformation deviceInfo;
		DeviceIdiom currentIdiom;
		DeviceType currentType = DeviceType.Unknown;
		string systemProductName;

		/// <summary>
		/// Initializes a new instance of the <see cref="DeviceInfoImplementation"/> class.
		/// </summary>
		public DeviceInfoImplementation()
		{
			deviceInfo = new EasClientDeviceInformation();
			currentIdiom = DeviceIdiom.Unknown;
			try
			{
				systemProductName = deviceInfo.SystemProductName;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to get system product name. {ex.Message}");
			}
		}

		public string Model => deviceInfo.SystemProductName;

		public string Manufacturer => deviceInfo.SystemManufacturer;

		public string Name => deviceInfo.FriendlyName;

		public string VersionString
		{
			get
			{
				var version = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;

				if (ulong.TryParse(version, out var v))
				{
					var v1 = (v & 0xFFFF000000000000L) >> 48;
					var v2 = (v & 0x0000FFFF00000000L) >> 32;
					var v3 = (v & 0x00000000FFFF0000L) >> 16;
					var v4 = v & 0x000000000000FFFFL;
					return $"{v1}.{v2}.{v3}.{v4}";
				}

				return version;
			}
		}

		public Version Version => Utils.ParseVersion(VersionString);

		public DevicePlatform Platform => DevicePlatform.WinUI;

		public DeviceIdiom Idiom
		{
			get
			{
				switch (AnalyticsInfo.VersionInfo.DeviceFamily)
				{
					case "Windows.Mobile":
						currentIdiom = DeviceIdiom.Phone;
						break;
					case "Windows.Universal":
					case "Windows.Desktop":
						currentIdiom = GetIsInTabletMode()
							? DeviceIdiom.Tablet
							: DeviceIdiom.Desktop;
						break;
					case "Windows.Xbox":
					case "Windows.Team":
						currentIdiom = DeviceIdiom.TV;
						break;
					case "Windows.IoT":
					default:
						currentIdiom = DeviceIdiom.Unknown;
						break;
				}

				return currentIdiom;
			}
		}

		public DeviceType DeviceType
		{
			get
			{
				if (currentType != DeviceType.Unknown)
					return currentType;

				try
				{
					if (string.IsNullOrWhiteSpace(systemProductName))
						systemProductName = deviceInfo.SystemProductName;

					var isVirtual = systemProductName.Contains("Virtual", StringComparison.Ordinal) || systemProductName == "HMV domU";

					currentType = isVirtual ? DeviceType.Virtual : DeviceType.Physical;
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Unable to get device type. {ex.Message}");
				}
				return currentType;
			}
		}

		/// <summary>
		/// Whether or not the device is in "tablet mode" or not. This
		/// has to be implemented by the device manufacturer.
		/// </summary>
		const int SM_CONVERTIBLESLATEMODE = 0x2003;

		/// <summary>
		/// How many fingers (aka touches) are supported for touch control
		/// </summary>
		const int SM_MAXIMUMTOUCHES = 95;

		/// <summary>
		/// Whether a physical keyboard is attached or not.
		/// Manufacturers have to remember to set this.
		/// Defaults to not-attached.
		/// </summary>
		const int SM_ISDOCKED = 0x2004;


		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern int GetSystemMetrics(int nIndex);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern bool GetAutoRotationState(ref AutoRotationState pState);

		[DllImport("Powrprof.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern PowerPlatformRole PowerDeterminePlatformRoleEx(ulong Version);

		static bool GetIsInTabletMode()
		{
			// Adopt Chromium's methodology for determining tablet mode
			// https://source.chromium.org/chromium/chromium/src/+/main:base/win/win_util.cc;l=537;drc=ac83a5a2d3c04763d86ce16d92f3904cc9566d3a;bpv=0;bpt=1
			// Device does not have a touchscreen
			if (GetSystemMetrics(SM_MAXIMUMTOUCHES) == 0)
			{
				return false;
			}

			// If the device is docked, user is treating as a PC
			if (GetSystemMetrics(SM_ISDOCKED) != 0)
			{
				return false;
			}

			// Fetch device rotation. Possible for this to fail.
			AutoRotationState rotationState = AutoRotationState.AR_ENABLED;
			bool success = GetAutoRotationState(ref rotationState);

			// Fetch succeeded and device does not support rotation
			if (success && (rotationState & (AutoRotationState.AR_NOT_SUPPORTED | AutoRotationState.AR_LAPTOP | AutoRotationState.AR_NOSENSOR)) != 0)
			{
				return false;
			}

			// Check if power management says we are mobile (laptop) or a tablet
			if ((PowerDeterminePlatformRoleEx(2) & (PowerPlatformRole.PlatformRoleMobile | PowerPlatformRole.PlatformRoleSlate)) != 0)
			{
				// Check if tablet mode is 0. 0 is default value.
				return GetSystemMetrics(SM_CONVERTIBLESLATEMODE) == 0;
			}

			return false;
		}
	}

	/// <summary>
	/// Represents the OEM's preferred power management profile,
	/// Useful in-case OEM implements one but not the other
	/// </summary>
	enum PowerPlatformRole
	{
		PlatformRoleMobile = 2,
		PlatformRoleSlate = 8,
	}

	/// <summary>
	/// Whether rotation is supported or not.
	/// Rotation is only supported if AR_ENABLED is true
	/// </summary>
	enum AutoRotationState
	{
		AR_ENABLED = 0x0,
		AR_NOT_SUPPORTED = 0x20,
		AR_LAPTOP = 0x80,
		AR_NOSENSOR = 0x10,
	}
}
