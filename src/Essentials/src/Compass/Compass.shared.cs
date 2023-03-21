#nullable enable
using System;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// Monitor changes to the orientation of the user's device.
	/// </summary>
	public interface ICompass
	{
		/// <summary>
		/// Gets a value indicating whether reading the compass is supported on this device.
		/// </summary>
		bool IsSupported { get; }

		/// <summary>
		/// Gets if compass is actively being monitored.
		/// </summary>
		bool IsMonitoring { get; }

		/// <summary>
		/// Start monitoring for changes to the compass.
		/// </summary>
		/// <param name="sensorSpeed">The speed to monitor for changes.</param>
		void Start(SensorSpeed sensorSpeed);

		/// <summary>
		/// Start monitoring for changes to the compass.
		/// </summary>
		/// <param name="sensorSpeed">The speed to monitor for changes.</param>
		/// <param name="applyLowPassFilter">Whether or not to apply a moving average filter (only used on Android).</param>
		void Start(SensorSpeed sensorSpeed, bool applyLowPassFilter);

		/// <summary>
		/// Stop monitoring for changes to the compass.
		/// </summary>
		void Stop();

		/// <summary>
		/// Occurs when compass reading changes.
		/// </summary>
		event EventHandler<CompassChangedEventArgs> ReadingChanged;
	}

	/// <summary>
	/// Platform-specific APIs for use with <see cref="ICompass"/>.
	/// </summary>
	public interface IPlatformCompass
	{
#if IOS || MACCATALYST
		/// <summary>
		/// Gets or sets if heading calibration should be shown.
		/// </summary>
		bool ShouldDisplayHeadingCalibration { get; set; }
#endif
	}

	/// <summary>
	/// Monitor changes to the orientation of the user's device.
	/// </summary>
	public static class Compass
	{
		/// <summary>
		/// Occurs when compass reading changes.
		/// </summary>
		public static event EventHandler<CompassChangedEventArgs> ReadingChanged
		{
			add => Current.ReadingChanged += value;
			remove => Current.ReadingChanged -= value;
		}

		/// <summary>
		/// Gets a value indicating whether reading the compass is supported on this device.
		/// </summary>
		public static bool IsSupported
			=> Current.IsSupported;

		/// <summary>
		/// Gets a value indicating whether the compass is actively being monitored.
		/// </summary>
		public static bool IsMonitoring
			=> Current.IsMonitoring;

		/// <summary>
		/// Start monitoring for changes to the compass.
		/// </summary>
		/// <remarks>
		/// Will throw <see cref="FeatureNotSupportedException"/> if not supported on device.
		/// Will throw <see cref="InvalidOperationException"/> if <see cref="IsMonitoring"/> is <see langword="true"/>.
		/// </remarks>
		/// <param name="sensorSpeed">The speed to monitor for changes.</param>
		public static void Start(SensorSpeed sensorSpeed)
			=> Start(sensorSpeed, true);

		/// <summary>
		/// Start monitoring for changes to the compass.
		/// </summary>
		/// <remarks>
		/// Will throw <see cref="FeatureNotSupportedException"/> if not supported on device.
		/// Will throw <see cref="InvalidOperationException"/> if <see cref="IsMonitoring"/> is <see langword="true"/>.
		/// </remarks>
		/// <param name="sensorSpeed">The speed to monitor for changes.</param>
		/// <param name="applyLowPassFilter">Whether or not to apply a moving average filter (only used on Android).</param>
		public static void Start(SensorSpeed sensorSpeed, bool applyLowPassFilter)
			=> Current.Start(sensorSpeed, applyLowPassFilter);

		/// <summary>
		/// Stop monitoring for changes to the compass.
		/// </summary>
		public static void Stop()
			=> Current.Stop();

#if IOS || MACCATALYST
		/// <summary>
		/// Gets or sets a value specifying whether the calibration screen should be displayed.
		/// </summary>
		/// <remarks>Only available on iOS.</remarks>
		public static bool ShouldDisplayHeadingCalibration
		{
			get
			{
				if (Current is IPlatformCompass c)
					return c.ShouldDisplayHeadingCalibration;
				return false;
			}
			set
			{
				if (Current is IPlatformCompass c)
					c.ShouldDisplayHeadingCalibration = value;
			}
		}
#endif

		static ICompass Current => Devices.Sensors.Compass.Default;

		static ICompass? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static ICompass Default =>
			defaultImplementation ??= new CompassImplementation();

		internal static void SetDefault(ICompass? implementation) =>
			defaultImplementation = implementation;
	}

	/// <summary>
	/// This class contains static extension methods for use with <see cref="ICompass"/>.
	/// </summary>
	public static class CompassExtensions
	{
#if IOS || MACCATALYST
		/// <summary>
		/// Gets or sets a value specifying whether the calibration screen should be displayed.
		/// </summary>
		/// <remarks>Only available on iOS.</remarks>
		public static void SetShouldDisplayHeadingCalibration(this ICompass compass, bool shouldDisplay)
		{
			if (compass is IPlatformCompass platform)
			{
				platform.ShouldDisplayHeadingCalibration = shouldDisplay;
			}
		}
#endif
	}

	/// <summary>
	/// Event arguments when compass reading changes.
	/// </summary>
	public class CompassChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CompassChangedEventArgs"/> class.
		/// </summary>
		/// <param name="reading">The compass data reading.</param>
		public CompassChangedEventArgs(CompassData reading) =>
			Reading = reading;

		/// <summary>
		/// The current values of the compass.
		/// </summary>
		public CompassData Reading { get; }
	}

	/// <summary>
	/// Contains the orientation of the user's device.
	/// </summary>
	public readonly struct CompassData : IEquatable<CompassData>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CompassData"/> class.
		/// </summary>
		/// <param name="headingMagneticNorth">A reading of compass data for heading magnetic north.</param>
		public CompassData(double headingMagneticNorth) =>
			HeadingMagneticNorth = headingMagneticNorth;

		/// <summary>
		/// The heading (measured in degrees) relative to magnetic north.
		/// </summary>
		public double HeadingMagneticNorth { get; }

		/// <summary>
		/// Compares the underlying <see cref="CompassData"/> instances.
		/// </summary>
		/// <param name="obj">Object to compare with.</param>
		/// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/>.</returns>
		public override bool Equals(object? obj) =>
			(obj is CompassData data) && Equals(data);

		/// <summary>
		/// Compares the underlying <see cref="CompassData.HeadingMagneticNorth"/> instances.
		/// </summary>
		/// <param name="other">Object to compare with.</param>
		/// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/>.</returns>
		public bool Equals(CompassData other) =>
			HeadingMagneticNorth.Equals(other.HeadingMagneticNorth);

		/// <summary>
		///	Equality operator for equals.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are equal, otherwise <see langword="false"/>.</returns>
		public static bool operator ==(CompassData left, CompassData right) =>
			left.Equals(right);

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are not equal, otherwise <see langword="false"/>.</returns>
		public static bool operator !=(CompassData left, CompassData right) =>
		   !left.Equals(right);

		/// <inheritdoc cref="object.GetHashCode"/>
		public override int GetHashCode() =>
			HeadingMagneticNorth.GetHashCode();

		/// <summary>
		/// Returns a string representation of the current values of <see cref="CompassData.HeadingMagneticNorth"/>.
		/// </summary>
		/// <returns>A string representation of this instance in the format of <c>HeadingMagneticNorth: {value}</c>.</returns>
		public override string ToString() =>
			$"{nameof(HeadingMagneticNorth)}: {HeadingMagneticNorth}";
	}

	partial class CompassImplementation : ICompass
	{
		bool UseSyncContext => SensorSpeed == SensorSpeed.Default || SensorSpeed == SensorSpeed.UI;

		public event EventHandler<CompassChangedEventArgs>? ReadingChanged;

		public bool IsSupported
			=> PlatformIsSupported;

		public bool IsMonitoring { get; private set; }

		SensorSpeed SensorSpeed { get; set; }

		public void Start(SensorSpeed sensorSpeed) => Start(sensorSpeed, true);

		public void Start(SensorSpeed sensorSpeed, bool applyLowPassFilter)
		{
			if (!PlatformIsSupported)
				throw new FeatureNotSupportedException();

			if (IsMonitoring)
				throw new InvalidOperationException("Compass has already been started.");

			IsMonitoring = true;


			try
			{
				PlatformStart(sensorSpeed, applyLowPassFilter);
			}
			catch
			{
				IsMonitoring = false;
				throw;
			}
		}

		public void Stop()
		{
			if (!PlatformIsSupported)
				throw new FeatureNotSupportedException();

			if (!IsMonitoring)
				return;

			IsMonitoring = false;

			try
			{
				PlatformStop();
			}
			catch
			{
				IsMonitoring = true;
				throw;
			}
		}

		internal void RaiseReadingChanged(CompassData data)
		{
			var args = new CompassChangedEventArgs(data);

			if (UseSyncContext)
				MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, args));
			else
				ReadingChanged?.Invoke(null, args);
		}
	}
}
