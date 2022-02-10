using System;
using System.ComponentModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public interface IBarometer
	{
		bool IsSupported { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="//Member[@MemberName='IsMonitoring']/Docs" />
		bool IsMonitoring { get; set; }

		void Start(SensorSpeed sensorSpeed);

		void Stop();
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Barometer']/Docs" />
	public static partial class Barometer
	{
		static bool useSyncContext;

		public static event EventHandler<BarometerChangedEventArgs> ReadingChanged;

		public static bool IsSupported => Current.IsSupported; 
		
		public static bool IsMonitoring 
		{
			get
			{
				return Current.IsMonitoring;
			}

 			set
			{
				Current.IsMonitoring = value;
			}
		}
		
		/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="//Member[@MemberName='Start']/Docs" />
		public static void Start(SensorSpeed sensorSpeed)
		{
			if (!Current.IsSupported)
				throw new FeatureNotSupportedException();

			if (Current.IsMonitoring)
				throw new InvalidOperationException("Barometer has already been started.");

			IsMonitoring = true;
			useSyncContext = sensorSpeed == SensorSpeed.Default || sensorSpeed == SensorSpeed.UI;

			try
			{
				Current.Start(sensorSpeed);
			}
			catch
			{
				Current.IsMonitoring = false;
				throw;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="//Member[@MemberName='Stop']/Docs" />
		public static void Stop()
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			if (!IsMonitoring)
				return;

			IsMonitoring = false;

			try
			{
				Current.Stop();
			}
			catch
			{
				Current.IsMonitoring = true;
				throw;
			}
		}

		internal static void OnChanged(BarometerData reading) =>
			OnChanged(new BarometerChangedEventArgs(reading));

		static void OnChanged(BarometerChangedEventArgs e)
		{
			if (useSyncContext)
				MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, e));
			else
				ReadingChanged?.Invoke(null, e);
		}

#nullable enable
		static IBarometer? currentImplementation;
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IBarometer Current =>
			currentImplementation ??= new BarometerImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(IBarometer? implementation) =>
			currentImplementation = implementation;
#nullable disable
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

		/// <include file="../../docs/Microsoft.Maui.Essentials/BarometerData.xml" path="//Member[@MemberName='Equals'][0]/Docs" />
		public override bool Equals(object obj) =>
			(obj is BarometerData data) && Equals(data);

		/// <include file="../../docs/Microsoft.Maui.Essentials/BarometerData.xml" path="//Member[@MemberName='Equals'][1]/Docs" />
		public bool Equals(BarometerData other) =>
			PressureInHectopascals.Equals(other.PressureInHectopascals);

		/// <include file="../../docs/Microsoft.Maui.Essentials/BarometerData.xml" path="//Member[@MemberName='GetHashCode']/Docs" />
		public override int GetHashCode() =>
			PressureInHectopascals.GetHashCode();

		/// <include file="../../docs/Microsoft.Maui.Essentials/BarometerData.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString() => $"{nameof(PressureInHectopascals)}: {PressureInHectopascals}";
	}
}
