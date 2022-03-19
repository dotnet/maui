#nullable enable
using System;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Battery']/Docs" />
	public static partial class Battery
	{
		static event EventHandler<BatteryInfoChangedEventArgs>? BatteryInfoChangedInternal;
		static event EventHandler<EnergySaverStatusChangedEventArgs>? EnergySaverStatusChangedInternal;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='ChargeLevel']/Docs" />
		public static double ChargeLevel => Current.ChargeLevel;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='State']/Docs" />
		public static BatteryState State => Current.State;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='PowerSource']/Docs" />
		public static BatteryPowerSource PowerSource => Current.PowerSource;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='EnergySaverStatus']/Docs" />
		public static EnergySaverStatus EnergySaverStatus => Current.EnergySaverStatus;

		public static event EventHandler<BatteryInfoChangedEventArgs> BatteryInfoChanged
		{
			add
			{
				if (BatteryInfoChangedInternal == null)
					Current.BatteryInfoChanged += OnBatteryInfoChanged;
				BatteryInfoChangedInternal += value;
			}
			remove
			{
				BatteryInfoChangedInternal -= value;
				if (BatteryInfoChangedInternal == null)
					Current.BatteryInfoChanged -= OnBatteryInfoChanged;
			}
		}

		public static event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChanged
		{
			add
			{
				if (EnergySaverStatusChangedInternal == null)
					Current.EnergySaverStatusChanged += OnEnergySaverStatusChanged;
				EnergySaverStatusChangedInternal += value;
			}
			remove
			{
				EnergySaverStatusChangedInternal -= value;
				if (EnergySaverStatusChangedInternal == null)
					Current.EnergySaverStatusChanged -= OnEnergySaverStatusChanged;
			}
		}

		static void OnBatteryInfoChanged(object? sender, BatteryInfoChangedEventArgs e) =>
			BatteryInfoChangedInternal?.Invoke(sender, e);

		static void OnEnergySaverStatusChanged(object? sender, EnergySaverStatusChangedEventArgs e) =>
			EnergySaverStatusChangedInternal?.Invoke(sender, e);

		static IBattery Current => Devices.Battery.Default;
	}
}
