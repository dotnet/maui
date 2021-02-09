using System;

namespace Xamarin.Essentials
{
	public class Placemark
	{
		public Placemark()
		{
		}

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

		public Location Location { get; set; }

		public string CountryCode { get; set; }

		public string CountryName { get; set; }

		public string FeatureName { get; set; }

		public string PostalCode { get; set; }

		public string SubLocality { get; set; }

		public string Thoroughfare { get; set; }

		public string SubThoroughfare { get; set; }

		public string Locality { get; set; }

		public string AdminArea { get; set; }

		public string SubAdminArea { get; set; }

		public override string ToString() =>
			$"{nameof(Location)}: {Location}, {nameof(CountryCode)}: {CountryCode}, " +
			$"{nameof(CountryName)}: {CountryName}, {nameof(FeatureName)}: {FeatureName}, " +
			$"{nameof(PostalCode)}: {PostalCode}, {nameof(SubLocality)}: {SubLocality}, " +
			$"{nameof(Thoroughfare)}: {Thoroughfare}, {nameof(SubThoroughfare)}: {SubThoroughfare}, " +
			$"{nameof(Locality)}: {Locality}, {nameof(AdminArea)}: {AdminArea}, " +
			$"{nameof(SubAdminArea)}: {SubAdminArea}";
	}
}
