#nullable enable
using System;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Battery']/Docs" />
	public static partial class Battery
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='ChargeLevel']/Docs" />
		public static double ChargeLevel => Current.ChargeLevel;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='State']/Docs" />
		public static BatteryState State => Current.State;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='PowerSource']/Docs" />
		public static BatteryPowerSource PowerSource => Current.PowerSource;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='EnergySaverStatus']/Docs" />
		public static EnergySaverStatus EnergySaverStatus => Current.EnergySaverStatus;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='BatteryInfoChanged']/Docs" />
		public static event EventHandler<BatteryInfoChangedEventArgs> BatteryInfoChanged
		{
			add => Current.BatteryInfoChanged += value;
			remove => Current.BatteryInfoChanged -= value;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='EnergySaverStatusChanged']/Docs" />
		public static event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChanged
		{
			add => Current.EnergySaverStatusChanged += value;
			remove => Current.EnergySaverStatusChanged -= value;
		}

		static IBattery Current => Devices.Battery.Default;
	}
}
