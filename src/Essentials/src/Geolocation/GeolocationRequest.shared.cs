using System;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/GeolocationAccuracy.xml" path="Type[@FullName='Microsoft.Maui.Essentials.GeolocationAccuracy']/Docs" />
	public enum GeolocationAccuracy
	{
		// Default is Medium
		/// <include file="../../docs/Microsoft.Maui.Essentials/GeolocationAccuracy.xml" path="//Member[@MemberName='Default']/Docs" />
		Default = 0,

		// iOS:     ThreeKilometers         (3000m)
		// Android: ACCURACY_LOW, POWER_LOW (500m)
		// UWP:     3000                    (1000-5000m)
		/// <include file="../../docs/Microsoft.Maui.Essentials/GeolocationAccuracy.xml" path="//Member[@MemberName='Lowest']/Docs" />
		Lowest = 1,

		// iOS:     Kilometer               (1000m)
		// Android: ACCURACY_LOW, POWER_MED (500m)
		// UWP:     1000                    (300-3000m)
		/// <include file="../../docs/Microsoft.Maui.Essentials/GeolocationAccuracy.xml" path="//Member[@MemberName='Low']/Docs" />
		Low = 2,

		// iOS:     HundredMeters           (100m)
		// Android: ACCURACY_MED, POWER_MED (100-500m)
		// UWP:     100                     (30-500m)
		/// <include file="../../docs/Microsoft.Maui.Essentials/GeolocationAccuracy.xml" path="//Member[@MemberName='Medium']/Docs" />
		Medium = 3,

		// iOS:     NearestTenMeters        (10m)
		// Android: ACCURACY_HI, POWER_HI   (0-100m)
		// UWP:     High                    (<=10m)
		/// <include file="../../docs/Microsoft.Maui.Essentials/GeolocationAccuracy.xml" path="//Member[@MemberName='High']/Docs" />
		High = 4,

		// iOS:     Best                    (0m)
		// Android: ACCURACY_HI, POWER_HI   (0-100m)
		// UWP:     High                    (<=10m)
		/// <include file="../../docs/Microsoft.Maui.Essentials/GeolocationAccuracy.xml" path="//Member[@MemberName='Best']/Docs" />
		Best = 5
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/GeolocationRequest.xml" path="Type[@FullName='Microsoft.Maui.Essentials.GeolocationRequest']/Docs" />
	public partial class GeolocationRequest
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/GeolocationRequest.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public GeolocationRequest()
		{
			Timeout = TimeSpan.Zero;
			DesiredAccuracy = GeolocationAccuracy.Default;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/GeolocationRequest.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public GeolocationRequest(GeolocationAccuracy accuracy)
		{
			Timeout = TimeSpan.Zero;
			DesiredAccuracy = accuracy;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/GeolocationRequest.xml" path="//Member[@MemberName='.ctor'][3]/Docs" />
		public GeolocationRequest(GeolocationAccuracy accuracy, TimeSpan timeout)
		{
			Timeout = timeout;
			DesiredAccuracy = accuracy;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/GeolocationRequest.xml" path="//Member[@MemberName='Timeout']/Docs" />
		public TimeSpan Timeout { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/GeolocationRequest.xml" path="//Member[@MemberName='DesiredAccuracy']/Docs" />
		public GeolocationAccuracy DesiredAccuracy { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/GeolocationRequest.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString() =>
			$"{nameof(DesiredAccuracy)}: {DesiredAccuracy}, {nameof(Timeout)}: {Timeout}";
	}
}
