using System;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Placemark.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Placemark']/Docs" />
	public class Placemark
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Placemark.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public Placemark()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Placemark.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public Placemark(Placemark placemark)
		{
			if (placemark == null)
				throw new ArgumentNullException(nameof(placemark));

			if (placemark.Location == null)
				Location = new Location();
			else
				Location = new Location(placemark.Location);

			CountryCode = placemark.CountryCode;
			CountryName = placemark.CountryName;
			FeatureName = placemark.FeatureName;
			PostalCode = placemark.PostalCode;
			Locality = placemark.Locality;
			SubLocality = placemark.SubLocality;
			Thoroughfare = placemark.Thoroughfare;
			SubThoroughfare = placemark.SubThoroughfare;
			SubAdminArea = placemark.SubAdminArea;
			AdminArea = placemark.AdminArea;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Placemark.xml" path="//Member[@MemberName='Location']/Docs" />
		public Location Location { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Placemark.xml" path="//Member[@MemberName='CountryCode']/Docs" />
		public string CountryCode { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Placemark.xml" path="//Member[@MemberName='CountryName']/Docs" />
		public string CountryName { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Placemark.xml" path="//Member[@MemberName='FeatureName']/Docs" />
		public string FeatureName { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Placemark.xml" path="//Member[@MemberName='PostalCode']/Docs" />
		public string PostalCode { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Placemark.xml" path="//Member[@MemberName='SubLocality']/Docs" />
		public string SubLocality { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Placemark.xml" path="//Member[@MemberName='Thoroughfare']/Docs" />
		public string Thoroughfare { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Placemark.xml" path="//Member[@MemberName='SubThoroughfare']/Docs" />
		public string SubThoroughfare { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Placemark.xml" path="//Member[@MemberName='Locality']/Docs" />
		public string Locality { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Placemark.xml" path="//Member[@MemberName='AdminArea']/Docs" />
		public string AdminArea { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Placemark.xml" path="//Member[@MemberName='SubAdminArea']/Docs" />
		public string SubAdminArea { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Placemark.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString() =>
			$"{nameof(Location)}: {Location}, {nameof(CountryCode)}: {CountryCode}, " +
			$"{nameof(CountryName)}: {CountryName}, {nameof(FeatureName)}: {FeatureName}, " +
			$"{nameof(PostalCode)}: {PostalCode}, {nameof(SubLocality)}: {SubLocality}, " +
			$"{nameof(Thoroughfare)}: {Thoroughfare}, {nameof(SubThoroughfare)}: {SubThoroughfare}, " +
			$"{nameof(Locality)}: {Locality}, {nameof(AdminArea)}: {AdminArea}, " +
			$"{nameof(SubAdminArea)}: {SubAdminArea}";
	}
}
