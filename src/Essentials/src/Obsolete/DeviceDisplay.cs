#nullable enable
using System;

namespace Microsoft.Maui.Devices
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="Type[@FullName='Microsoft.Maui.Essentials.DeviceDisplay']/Docs" />
	public static partial class DeviceDisplay
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="//Member[@MemberName='KeepScreenOn']/Docs" />
		[Obsolete($"Use {nameof(DeviceDisplay)}.{nameof(Current)} instead.", true)]
		public static bool KeepScreenOn
		{
			get => Current.KeepScreenOn;
			set => Current.KeepScreenOn = value;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="//Member[@MemberName='MainDisplayInfo']/Docs" />
		[Obsolete($"Use {nameof(DeviceDisplay)}.{nameof(Current)} instead.", true)]
		public static DisplayInfo MainDisplayInfo => Current.MainDisplayInfo;

		/// <include file="../../docs/Microsoft.Maui.Essentials/DeviceDisplay.xml" path="//Member[@MemberName='MainDisplayInfoChanged']/Docs" />
		[Obsolete($"Use {nameof(DeviceDisplay)}.{nameof(Current)} instead.", true)]
		public static event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged
		{
			add => Current.MainDisplayInfoChanged += value;
			remove => Current.MainDisplayInfoChanged -= value;
		}
	}
}
