using System;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Battery']/Docs" />
	public static partial class Battery
	{
		static event EventHandler<BatteryInfoChangedEventArgs> BatteryInfoChangedInternal;

		static event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChangedInternal;

		// a cache so that events aren't fired unnecessarily
		// this is mainly an issue on Android, but we can stiil do this everywhere
		static double currentLevel;
		static BatteryPowerSource currentSource;
		static BatteryState currentState;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='ChargeLevel']/Docs" />
		public static double ChargeLevel => PlatformChargeLevel;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='State']/Docs" />
		public static BatteryState State => PlatformState;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='PowerSource']/Docs" />
		public static BatteryPowerSource PowerSource => PlatformPowerSource;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="//Member[@MemberName='EnergySaverStatus']/Docs" />
		public static EnergySaverStatus EnergySaverStatus => PlatformEnergySaverStatus;

		public static event EventHandler<BatteryInfoChangedEventArgs> BatteryInfoChanged
		{
			add
			{
				var wasRunning = BatteryInfoChangedInternal != null;

				BatteryInfoChangedInternal += value;

				if (!wasRunning && BatteryInfoChangedInternal != null)
				{
					SetCurrent();
					StartBatteryListeners();
				}
			}

			remove
			{
				var wasRunning = BatteryInfoChangedInternal != null;

				BatteryInfoChangedInternal -= value;

				if (wasRunning && BatteryInfoChangedInternal == null)
					StopBatteryListeners();
			}
		}

		public static event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChanged
		{
			add
			{
				var wasRunning = EnergySaverStatusChangedInternal != null;

				EnergySaverStatusChangedInternal += value;

				if (!wasRunning && EnergySaverStatusChangedInternal != null)
					StartEnergySaverListeners();
			}

			remove
			{
				var wasRunning = EnergySaverStatusChangedInternal != null;

				EnergySaverStatusChangedInternal -= value;

				if (wasRunning && EnergySaverStatusChangedInternal == null)
					StopEnergySaverListeners();
			}
		}

		static void SetCurrent()
		{
			currentLevel = Battery.ChargeLevel;
			currentSource = Battery.PowerSource;
			currentState = Battery.State;
		}

		static void OnBatteryInfoChanged(double level, BatteryState state, BatteryPowerSource source)
			=> OnBatteryInfoChanged(new BatteryInfoChangedEventArgs(level, state, source));

		static void OnBatteryInfoChanged()
			=> OnBatteryInfoChanged(ChargeLevel, State, PowerSource);

		static void OnBatteryInfoChanged(BatteryInfoChangedEventArgs e)
		{
			if (currentLevel != e.ChargeLevel || currentSource != e.PowerSource || currentState != e.State)
			{
				SetCurrent();
				BatteryInfoChangedInternal?.Invoke(null, e);
			}
		}

		static void OnEnergySaverChanged()
			=> OnEnergySaverChanged(EnergySaverStatus);

		static void OnEnergySaverChanged(EnergySaverStatus saverStatus)
			=> OnEnergySaverChanged(new EnergySaverStatusChangedEventArgs(saverStatus));

		static void OnEnergySaverChanged(EnergySaverStatusChangedEventArgs e)
			=> EnergySaverStatusChangedInternal?.Invoke(null, e);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryState.xml" path="Type[@FullName='Microsoft.Maui.Essentials.BatteryState']/Docs" />
	public enum BatteryState
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryState.xml" path="//Member[@MemberName='Unknown']/Docs" />
		Unknown = 0,
		/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryState.xml" path="//Member[@MemberName='Charging']/Docs" />
		Charging = 1,
		/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryState.xml" path="//Member[@MemberName='Discharging']/Docs" />
		Discharging = 2,
		/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryState.xml" path="//Member[@MemberName='Full']/Docs" />
		Full = 3,
		/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryState.xml" path="//Member[@MemberName='NotCharging']/Docs" />
		NotCharging = 4,
		/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryState.xml" path="//Member[@MemberName='NotPresent']/Docs" />
		NotPresent = 5
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryPowerSource.xml" path="Type[@FullName='Microsoft.Maui.Essentials.BatteryPowerSource']/Docs" />
	public enum BatteryPowerSource
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryPowerSource.xml" path="//Member[@MemberName='Unknown']/Docs" />
		Unknown = 0,
		/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryPowerSource.xml" path="//Member[@MemberName='Battery']/Docs" />
		Battery = 1,
		/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryPowerSource.xml" path="//Member[@MemberName='AC']/Docs" />
		AC = 2,
		/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryPowerSource.xml" path="//Member[@MemberName='Usb']/Docs" />
		Usb = 3,
		/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryPowerSource.xml" path="//Member[@MemberName='Wireless']/Docs" />
		Wireless = 4
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/EnergySaverStatus.xml" path="Type[@FullName='Microsoft.Maui.Essentials.EnergySaverStatus']/Docs" />
	public enum EnergySaverStatus
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/EnergySaverStatus.xml" path="//Member[@MemberName='Unknown']/Docs" />
		Unknown = 0,
		/// <include file="../../docs/Microsoft.Maui.Essentials/EnergySaverStatus.xml" path="//Member[@MemberName='On']/Docs" />
		On = 1,
		/// <include file="../../docs/Microsoft.Maui.Essentials/EnergySaverStatus.xml" path="//Member[@MemberName='Off']/Docs" />
		Off = 2
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryInfoChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Essentials.BatteryInfoChangedEventArgs']/Docs" />
	public class BatteryInfoChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryInfoChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public BatteryInfoChangedEventArgs(double level, BatteryState state, BatteryPowerSource source)
		{
			ChargeLevel = level;
			State = state;
			PowerSource = source;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryInfoChangedEventArgs.xml" path="//Member[@MemberName='ChargeLevel']/Docs" />
		public double ChargeLevel { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryInfoChangedEventArgs.xml" path="//Member[@MemberName='State']/Docs" />
		public BatteryState State { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryInfoChangedEventArgs.xml" path="//Member[@MemberName='PowerSource']/Docs" />
		public BatteryPowerSource PowerSource { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/BatteryInfoChangedEventArgs.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString() =>
			$"{nameof(ChargeLevel)}: {ChargeLevel.ToString()}, " +
			$"{nameof(State)}: {State}, " +
			$"{nameof(PowerSource)}: {PowerSource}";
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/EnergySaverStatusChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Essentials.EnergySaverStatusChangedEventArgs']/Docs" />
	public class EnergySaverStatusChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/EnergySaverStatusChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public EnergySaverStatusChangedEventArgs(EnergySaverStatus saverStatus)
		{
			EnergySaverStatus = saverStatus;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/EnergySaverStatusChangedEventArgs.xml" path="//Member[@MemberName='EnergySaverStatus']/Docs" />
		public EnergySaverStatus EnergySaverStatus { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/EnergySaverStatusChangedEventArgs.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString() =>
			$"{nameof(EnergySaverStatus)}: {EnergySaverStatus}";
	}
}
