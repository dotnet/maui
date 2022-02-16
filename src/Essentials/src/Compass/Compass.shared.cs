using System;
using System.ComponentModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public interface ICompass
	{
		bool IsSupported { get; }

		bool IsMonitoring { get; set; }

		void Start(SensorSpeed sensorSpeed);

		void Start(SensorSpeed sensorSpeed, bool applyLowPassFilter);
		
		void Stop();
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Compass']/Docs" />
	public static partial class Compass
	{
		static bool useSyncContext;

		public static event EventHandler<CompassChangedEventArgs> ReadingChanged;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="//Member[@MemberName='IsMonitoring']/Docs" />
		public static bool IsSupported { get; private set; }

		public static bool IsMonitoring { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="//Member[@MemberName='Start'][0]/Docs" />
		public static void Start(SensorSpeed sensorSpeed) => Start(sensorSpeed, true);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="//Member[@MemberName='Start'][1]/Docs" />
		public static void Start(SensorSpeed sensorSpeed, bool applyLowPassFilter)
		{
			if (!Current.IsSupported)
				throw new FeatureNotSupportedException();

			if (Current.IsMonitoring)
				throw new InvalidOperationException("Compass has already been started.");

			Current.IsMonitoring = true;
			useSyncContext = sensorSpeed == SensorSpeed.Default || sensorSpeed == SensorSpeed.UI;

			try
			{
				Current.Start(sensorSpeed, applyLowPassFilter);
			}
			catch
			{
				Current.IsMonitoring = false;
				throw;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="//Member[@MemberName='Stop']/Docs" />
		public static void Stop()
		{
			if (!Current.IsSupported)
				throw new FeatureNotSupportedException();

			if (!Current.IsMonitoring)
				return;

			Current.IsMonitoring = false;

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

		internal static void OnChanged(CompassData reading) =>
			OnChanged(new CompassChangedEventArgs(reading));

		internal static void OnChanged(CompassChangedEventArgs e)
		{
			if (useSyncContext)
				MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, e));
			else
				ReadingChanged?.Invoke(null, e);
		}

#nullable enable
		static ICompass? currentImplementation;
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static ICompass Current =>
			currentImplementation ??= new CompassImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(ICompass? implementation) =>
			currentImplementation = implementation;
#nullable disable
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/CompassChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Essentials.CompassChangedEventArgs']/Docs" />
	public class CompassChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/CompassChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public CompassChangedEventArgs(CompassData reading) =>
			Reading = reading;

		/// <include file="../../docs/Microsoft.Maui.Essentials/CompassChangedEventArgs.xml" path="//Member[@MemberName='Reading']/Docs" />
		public CompassData Reading { get; }
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/CompassData.xml" path="Type[@FullName='Microsoft.Maui.Essentials.CompassData']/Docs" />
	public readonly struct CompassData : IEquatable<CompassData>
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/CompassData.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public CompassData(double headingMagneticNorth) =>
			HeadingMagneticNorth = headingMagneticNorth;

		/// <include file="../../docs/Microsoft.Maui.Essentials/CompassData.xml" path="//Member[@MemberName='HeadingMagneticNorth']/Docs" />
		public double HeadingMagneticNorth { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/CompassData.xml" path="//Member[@MemberName='Equals'][0]/Docs" />
		public override bool Equals(object obj) =>
			(obj is CompassData data) && Equals(data);

		/// <include file="../../docs/Microsoft.Maui.Essentials/CompassData.xml" path="//Member[@MemberName='Equals'][1]/Docs" />
		public bool Equals(CompassData other) =>
			HeadingMagneticNorth.Equals(other.HeadingMagneticNorth);

		public static bool operator ==(CompassData left, CompassData right) =>
			left.Equals(right);

		public static bool operator !=(CompassData left, CompassData right) =>
		   !left.Equals(right);

		/// <include file="../../docs/Microsoft.Maui.Essentials/CompassData.xml" path="//Member[@MemberName='GetHashCode']/Docs" />
		public override int GetHashCode() =>
			HeadingMagneticNorth.GetHashCode();

		/// <include file="../../docs/Microsoft.Maui.Essentials/CompassData.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString() =>
			$"{nameof(HeadingMagneticNorth)}: {HeadingMagneticNorth}";
	}
}
