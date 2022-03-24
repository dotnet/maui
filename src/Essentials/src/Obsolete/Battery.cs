#nullable enable
using System;

namespace Microsoft.Maui.Devices
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Battery']/Docs" />
	public static partial class Battery
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='ChargeLevel']/Docs" />
		[Obsolete($"Use {nameof(Battery)}.{nameof(Default)} instead.", true)]
		public static double ChargeLevel => Default.ChargeLevel;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='State']/Docs" />
		[Obsolete($"Use {nameof(Battery)}.{nameof(Default)} instead.", true)]
		public static BatteryState State => Default.State;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='PowerSource']/Docs" />
		[Obsolete($"Use {nameof(Battery)}.{nameof(Default)} instead.", true)]
		public static BatteryPowerSource PowerSource => Default.PowerSource;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='EnergySaverStatus']/Docs" />
		[Obsolete($"Use {nameof(Battery)}.{nameof(Default)} instead.", true)]
		public static EnergySaverStatus EnergySaverStatus => Default.EnergySaverStatus;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='BatteryInfoChanged']/Docs" />
		[Obsolete($"Use {nameof(Battery)}.{nameof(Default)} instead.", true)]
		public static event EventHandler<BatteryInfoChangedEventArgs> BatteryInfoChanged
		{
			add => Default.BatteryInfoChanged += value;
			remove => Default.BatteryInfoChanged -= value;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='EnergySaverStatusChanged']/Docs" />
		[Obsolete($"Use {nameof(Battery)}.{nameof(Default)} instead.", true)]
		public static event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChanged
		{
			add => Default.EnergySaverStatusChanged += value;
			remove => Default.EnergySaverStatusChanged -= value;
		}
	}
}
