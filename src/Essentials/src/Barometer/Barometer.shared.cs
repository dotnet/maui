#nullable enable
using System;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	public interface IBarometer
	{
		bool IsSupported { get; }

		bool IsMonitoring { get; }

		void Start(SensorSpeed sensorSpeed);

		event EventHandler<BarometerChangedEventArgs>? ReadingChanged;

		void Stop();
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Barometer']/Docs" />
	public static class Barometer
	{
		public static event EventHandler<BarometerChangedEventArgs> ReadingChanged
		{
			add => Default.ReadingChanged += value;
			remove => Default.ReadingChanged -= value;
		}

		public static bool IsSupported => Default.IsSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="//Member[@MemberName='IsMonitoring']/Docs" />
		public static bool IsMonitoring
			=> Default.IsMonitoring;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="//Member[@MemberName='Start']/Docs" />
		public static void Start(SensorSpeed sensorSpeed)
			=> Default.Start(sensorSpeed);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="//Member[@MemberName='Stop']/Docs" />
		public static void Stop()
			=> Default.Stop();

		static IBarometer? defaultImplementation;

		public static IBarometer Default =>
			defaultImplementation ??= new BarometerImplementation();

		internal static void SetDefault(IBarometer? implementation) =>
			defaultImplementation = implementation;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/BarometerChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Essentials.BarometerChangedEventArgs']/Docs" />
	public class BarometerChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/BarometerChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public BarometerChangedEventArgs(BarometerData reading) =>
			Reading = reading;

		/// <include file="../../docs/Microsoft.Maui.Essentials/BarometerChangedEventArgs.xml" path="//Member[@MemberName='Reading']/Docs" />
		public BarometerData Reading { get; }
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/BarometerData.xml" path="Type[@FullName='Microsoft.Maui.Essentials.BarometerData']/Docs" />
	public readonly struct BarometerData : IEquatable<BarometerData>
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/BarometerData.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public BarometerData(double pressure) =>
			PressureInHectopascals = pressure;

		/// <include file="../../docs/Microsoft.Maui.Essentials/BarometerData.xml" path="//Member[@MemberName='PressureInHectopascals']/Docs" />
		public double PressureInHectopascals { get; }

		public static bool operator ==(BarometerData left, BarometerData right) =>
			left.Equals(right);

		public static bool operator !=(BarometerData left, BarometerData right) =>
			!left.Equals(right);

		/// <include file="../../docs/Microsoft.Maui.Essentials/BarometerData.xml" path="//Member[@MemberName='Equals'][1]/Docs" />
		public override bool Equals(object? obj) =>
			(obj is BarometerData data) && Equals(data);

		/// <include file="../../docs/Microsoft.Maui.Essentials/BarometerData.xml" path="//Member[@MemberName='Equals'][2]/Docs" />
		public bool Equals(BarometerData other) =>
			PressureInHectopascals.Equals(other.PressureInHectopascals);

		/// <include file="../../docs/Microsoft.Maui.Essentials/BarometerData.xml" path="//Member[@MemberName='GetHashCode']/Docs" />
		public override int GetHashCode() =>
			PressureInHectopascals.GetHashCode();

		/// <include file="../../docs/Microsoft.Maui.Essentials/BarometerData.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString() => $"{nameof(PressureInHectopascals)}: {PressureInHectopascals}";
	}

	partial class BarometerImplementation : IBarometer
	{
		bool UseSyncContext => SensorSpeed == SensorSpeed.Default || SensorSpeed == SensorSpeed.UI;

#pragma warning disable CS0067
		public event EventHandler<BarometerChangedEventArgs>? ReadingChanged;
#pragma warning restore CS0067

		public bool IsMonitoring { get; private set; }

		SensorSpeed SensorSpeed { get; set; } = SensorSpeed.Default;

		void RaiseReadingChanged(BarometerData reading)
		{
			var args = new BarometerChangedEventArgs(reading);

			if (UseSyncContext)
				MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(this, args));
			else
				ReadingChanged?.Invoke(this, args);
		}

		public void Start(SensorSpeed sensorSpeed)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			if (IsMonitoring)
				throw new InvalidOperationException("Barometer has already been started.");

			IsMonitoring = true;
			SensorSpeed = sensorSpeed;

			try
			{
				PlatformStart(sensorSpeed);
			}
			catch
			{
				IsMonitoring = false;
				throw;
			}
		}

		public void Stop()
		{
			if (!IsSupported)
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
	}
}
