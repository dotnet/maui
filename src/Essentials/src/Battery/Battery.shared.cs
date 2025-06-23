#nullable enable
using System;

namespace Microsoft.Maui.Devices
{
	/// <summary>
	/// Methods and properties for battery and charging information of the device.
	/// </summary>
	/// <remarks>
	/// <para>Platform specific remarks:</para>
	/// <para>- Android: <c>Battery_Stats</c> permission must be set in manifest.</para>
	/// <para>- iOS: Simulator will not return battery information, must be run on device.</para>
	/// <para>- Windows: None.</para>
	/// </remarks>
	public interface IBattery
	{
		/// <summary>
		/// Gets the current charge level of the device from 0.0 to 1.0.
		/// </summary>
		/// <remarks>Returns -1 if no battery exists.</remarks>
		double ChargeLevel { get; }

		/// <summary>
		/// Gets the charging state of the device.
		/// </summary>
		BatteryState State { get; }

		/// <summary>
		/// Gets the current power source for the device.
		/// </summary>
		BatteryPowerSource PowerSource { get; }

		/// <summary>
		/// Gets the current energy saver status of the device.
		/// </summary>
		EnergySaverStatus EnergySaverStatus { get; }

		/// <summary>
		/// Occurs when battery properties change.
		/// </summary>
		event EventHandler<BatteryInfoChangedEventArgs> BatteryInfoChanged;

		/// <summary>
		/// Occurs when the energy saver status changes.
		/// </summary>
		event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChanged;
	}

	/// <summary>
	/// Methods and properties for battery and charging information of the device.
	/// </summary>
	public static class Battery
	{
		/// <summary>
		/// Gets the current charge level of the device from 0.0 to 1.0.
		/// </summary>
		/// <remarks>Returns -1 if no battery exists.</remarks>
		public static double ChargeLevel => Default.ChargeLevel;

		/// <summary>
		/// Gets the charging state of the device.
		/// </summary>
		public static BatteryState State => Default.State;

		/// <summary>
		/// Gets the current power source for the device.
		/// </summary>
		public static BatteryPowerSource PowerSource => Default.PowerSource;

		/// <summary>
		/// Gets the current energy saver status of the device.
		/// </summary>
		public static EnergySaverStatus EnergySaverStatus => Default.EnergySaverStatus;

		/// <summary>
		/// Event trigger when battery properties have changed.
		/// </summary>
		public static event EventHandler<BatteryInfoChangedEventArgs> BatteryInfoChanged
		{
			add => Default.BatteryInfoChanged += value;
			remove => Default.BatteryInfoChanged -= value;
		}

		/// <summary>
		/// Occurs when the energy saver status changes.
		/// </summary>
		public static event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChanged
		{
			add => Default.EnergySaverStatusChanged += value;
			remove => Default.EnergySaverStatusChanged -= value;
		}

		static IBattery? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>s
		public static IBattery Default =>
			defaultImplementation ??= new BatteryImplementation();

		internal static void SetDefault(IBattery? implementation) =>
			defaultImplementation = implementation;
	}

	partial class BatteryImplementation : IBattery
	{
		event EventHandler<BatteryInfoChangedEventArgs>? BatteryInfoChangedInternal;

		event EventHandler<EnergySaverStatusChangedEventArgs>? EnergySaverStatusChangedInternal;

		public event EventHandler<BatteryInfoChangedEventArgs> BatteryInfoChanged
		{
			add
			{
				if (BatteryInfoChangedInternal == null)
					StartBatteryListeners();
				BatteryInfoChangedInternal += value;
			}
			remove
			{
				BatteryInfoChangedInternal -= value;
				if (BatteryInfoChangedInternal == null)
					StopBatteryListeners();
			}
		}

		public event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChanged
		{
			add
			{
				if (EnergySaverStatusChangedInternal == null)
					StartEnergySaverListeners();
				EnergySaverStatusChangedInternal += value;
			}
			remove
			{
				EnergySaverStatusChangedInternal -= value;
				if (EnergySaverStatusChangedInternal == null)
					StopEnergySaverListeners();
			}
		}

		// a cache so that events aren't fired unnecessarily
		// this is mainly an issue on Android, but we can stiil do this everywhere
		static double currentLevel;
		static BatteryPowerSource currentSource;
		static BatteryState currentState;

		void SetCurrent()
		{
			currentLevel = ChargeLevel;
			currentSource = PowerSource;
			currentState = State;
		}

		void OnBatteryInfoChanged(double level, BatteryState state, BatteryPowerSource source)
			=> OnBatteryInfoChanged(new BatteryInfoChangedEventArgs(level, state, source));

		void OnBatteryInfoChanged()
			=> OnBatteryInfoChanged(ChargeLevel, State, PowerSource);

		void OnBatteryInfoChanged(BatteryInfoChangedEventArgs e)
		{
			if (currentLevel != e.ChargeLevel || currentSource != e.PowerSource || currentState != e.State)
			{
				SetCurrent();
				BatteryInfoChangedInternal?.Invoke(null, e);
			}
		}

		void OnEnergySaverChanged()
			=> OnEnergySaverChanged(EnergySaverStatus);

		void OnEnergySaverChanged(EnergySaverStatus saverStatus)
			=> OnEnergySaverChanged(new EnergySaverStatusChangedEventArgs(saverStatus));

		void OnEnergySaverChanged(EnergySaverStatusChangedEventArgs e)
			=> EnergySaverStatusChangedInternal?.Invoke(null, e);
	}

	/// <summary>
	/// Describes possible states of the battery.
	/// </summary>
	public enum BatteryState
	{
		/// <summary>Battery state could not be determined.</summary>
		Unknown = 0,

		/// <summary>Battery is actively being charged by a power source.</summary>
		Charging = 1,

		/// <summary>Battery is not plugged in and discharging.</summary>
		Discharging = 2,

		/// <summary>Battery is full.</summary>
		Full = 3,

		/// <summary>Battery is not charging or discharging, but in an inbetween state.</summary>
		NotCharging = 4,

		/// <summary>Battery does not exist on the device.</summary>
		NotPresent = 5
	}

	/// <summary>
	/// Enumerates power sources with which the device and battery can be powered or charged.
	/// </summary>
	public enum BatteryPowerSource
	{
		/// <summary>Power source cannot be determined.</summary>
		Unknown = 0,

		/// <summary>Power source is the battery and is currently not being charged.</summary>
		Battery = 1,

		/// <summary>Power source is an AC Charger.</summary>
		AC = 2,

		/// <summary>Power source is a USB port.</summary>
		Usb = 3,

		/// <summary>Power source is wireless.</summary>
		Wireless = 4
	}

	/// <summary>
	/// Enumerates states that the energy saver van have on the device.
	/// </summary>
	public enum EnergySaverStatus
	{
		/// <summary>Status of energy saving is unknown.</summary>
		Unknown = 0,

		/// <summary>Energy saving is on.</summary>
		On = 1,

		/// <summary>Energy saving is off.</summary>
		Off = 2
	}

	/// <summary>
	/// Event arguments containing the current reading of <see cref="IBattery"/>.
	/// </summary>
	public class BatteryInfoChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BatteryInfoChangedEventArgs"/> class.
		/// </summary>
		/// <param name="level">The current level of the battery.</param>
		/// <param name="state">The state of the battery.</param>
		/// <param name="source">The source of the battery.</param>
		public BatteryInfoChangedEventArgs(double level, BatteryState state, BatteryPowerSource source)
		{
			ChargeLevel = level;
			State = state;
			PowerSource = source;
		}

		/// <summary>
		/// Gets the current charge level of the device from 0.0 to 1.0.
		/// </summary>
		public double ChargeLevel { get; }

		/// <summary>
		/// Gets the charging state of the device.
		/// </summary>
		public BatteryState State { get; }

		/// <summary>
		/// Gets the current power source for the device.
		/// </summary>
		public BatteryPowerSource PowerSource { get; }

		/// <summary>
		/// Returns a string representation of this instance of <see cref="BatteryInfoChangedEventArgs"/>.
		/// </summary>
		/// <returns>A string representation of this instance in the format of <c>ChargeLevel: {value}, State: {value}, PowerSource: {value}</c>.</returns>
		public override string ToString() =>
			$"{nameof(ChargeLevel)}: {ChargeLevel.ToString()}, " +
			$"{nameof(State)}: {State}, " +
			$"{nameof(PowerSource)}: {PowerSource}";
	}

	/// <summary>
	/// Event arguments when the energy saver status changes.
	/// </summary>
	public class EnergySaverStatusChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EnergySaverStatusChangedEventArgs"/> class.
		/// </summary>
		/// <param name="saverStatus">The current status of the event.</param>
		public EnergySaverStatusChangedEventArgs(EnergySaverStatus saverStatus)
		{
			EnergySaverStatus = saverStatus;
		}

		/// <summary>
		/// Gets the current status of energy saver mode.
		/// </summary>
		public EnergySaverStatus EnergySaverStatus { get; }

		/// <summary>
		/// Returns a string representation of this instance of <see cref="EnergySaverStatusChangedEventArgs"/>.
		/// </summary>
		/// <returns>A string representation of this instance in the format of <c>EnergySaverStatus: {value}</c>.</returns>
		public override string ToString() =>
			$"{nameof(EnergySaverStatus)}: {EnergySaverStatus}";
	}
}
