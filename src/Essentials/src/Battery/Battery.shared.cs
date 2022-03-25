using System;
using System.ComponentModel;

namespace Microsoft.Maui.Devices
{
	public interface IBattery
	{
		double ChargeLevel { get; }

		BatteryState State { get; }

		BatteryPowerSource PowerSource { get; }

		EnergySaverStatus EnergySaverStatus { get; }

		void StartEnergySaverListeners();

		void StopEnergySaverListeners();

		void StartBatteryListeners();

		void StopBatteryListeners();
	}

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
				var wasRunning = BatteryInfoChangedInternal != null;

				BatteryInfoChangedInternal += value;

				if (!wasRunning && BatteryInfoChangedInternal != null)
				{
					SetCurrent();
					Current.StartBatteryListeners();
				}
			}

			remove
			{
				var wasRunning = BatteryInfoChangedInternal != null;

				BatteryInfoChangedInternal -= value;

				if (wasRunning && BatteryInfoChangedInternal == null)
					Current.StopBatteryListeners();
			}
		}

		public static event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChanged
		{
			add
			{
				var wasRunning = EnergySaverStatusChangedInternal != null;

				EnergySaverStatusChangedInternal += value;

				if (!wasRunning && EnergySaverStatusChangedInternal != null)
					Current.StartEnergySaverListeners();
			}

			remove
			{
				var wasRunning = EnergySaverStatusChangedInternal != null;

				EnergySaverStatusChangedInternal -= value;

				if (wasRunning && EnergySaverStatusChangedInternal == null)
					Current.StopEnergySaverListeners();
			}
		}

		static void SetCurrent()
		{
			currentLevel = Battery.ChargeLevel;
			currentSource = Battery.PowerSource;
			currentState = Battery.State;
		}

		internal static void OnBatteryInfoChanged(double level, BatteryState state, BatteryPowerSource source)
			=> OnBatteryInfoChanged(new BatteryInfoChangedEventArgs(level, state, source));

		internal static void OnBatteryInfoChanged()
			=> OnBatteryInfoChanged(ChargeLevel, State, PowerSource);

		internal static void OnBatteryInfoChanged(BatteryInfoChangedEventArgs e)
		{
			if (currentLevel != e.ChargeLevel || currentSource != e.PowerSource || currentState != e.State)
			{
				SetCurrent();
				BatteryInfoChangedInternal?.Invoke(null, e);
			}
		}

		internal static void OnEnergySaverChanged()
			=> OnEnergySaverChanged(EnergySaverStatus);

		internal static void OnEnergySaverChanged(EnergySaverStatus saverStatus)
			=> OnEnergySaverChanged(new EnergySaverStatusChangedEventArgs(saverStatus));

		internal static void OnEnergySaverChanged(EnergySaverStatusChangedEventArgs e)
			=> EnergySaverStatusChangedInternal?.Invoke(null, e);


#nullable enable
		static IBattery? currentImplementation;
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IBattery Current =>
			currentImplementation ??= new BatteryImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(IBattery? implementation) =>
			currentImplementation = implementation;
#nullable disable
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
