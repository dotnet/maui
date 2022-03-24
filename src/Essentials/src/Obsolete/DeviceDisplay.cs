#nullable enable
using System;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="Type[@FullName='Microsoft.Maui.Essentials.DeviceDisplay']/Docs" />
	public static class DeviceDisplay
	{
		static IDeviceDisplay Current => Devices.DeviceDisplay.Current;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="//Member[@MemberName='KeepScreenOn']/Docs" />
		public static bool KeepScreenOn
		{
			get => Current.KeepScreenOn;
			set => Current.KeepScreenOn = value;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="//Member[@MemberName='MainDisplayInfo']/Docs" />
		public static DisplayInfo MainDisplayInfo => Current.MainDisplayInfo;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="//Member[@MemberName='MainDisplayInfoChanged']/Docs" />
		public static event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged
		{
			add => Current.MainDisplayInfoChanged += value;
			remove => Current.MainDisplayInfoChanged -= value;
		}
	}
}
