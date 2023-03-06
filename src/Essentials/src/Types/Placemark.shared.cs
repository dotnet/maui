using System;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// Represents a user-friendly description of a geographic coordinate. This contains information such as the name of the place, its address, and other information.
	/// </summary>
	public class Placemark
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Placemark"/> class.
		/// </summary>
		public Placemark()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Placemark"/> class.
		/// </summary>
		/// <param name="placemark">An instance of <see cref="Placemark"/> that will be used to clone into this instance.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="placemark"/> is <see langword="null"/>.</exception>
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

		/// <summary>
		/// Gets or sets the location of the placemark.
		/// </summary>
		public Location Location { get; set; }

		/// <summary>
		/// Gets or sets the country ISO standard code.
		/// </summary>
		public string CountryCode { get; set; }

		/// <summary>
		/// Gets or sets the country name.
		/// </summary>
		public string CountryName { get; set; }

		/// <summary>
		/// Gets or sets the feature name.
		/// </summary>
		public string FeatureName { get; set; }

		/// <summary>
		/// Gets or sets the postal code.
		/// </summary>
		public string PostalCode { get; set; }

		/// <summary>
		/// Gets or sets the sub locality.
		/// </summary>
		public string SubLocality { get; set; }

		/// <summary>
		/// Gets or sets the street name.
		/// </summary>
		public string Thoroughfare { get; set; }

		/// <summary>
		/// Gets or sets optional info: sub street or region.
		/// </summary>
		public string SubThoroughfare { get; set; }

		/// <summary>
		/// Gets or sets the city or town.
		/// </summary>
		public string Locality { get; set; }

		/// <summary>
		/// Gets or sets the administrative area name of the address, for example, "CA", or <see langword="null"/> if it is unknown.
		/// </summary>
		public string AdminArea { get; set; }

		/// <summary>
		/// Gets or sets the sub-administrative area name of the address, for example, "Santa Clara County", or <see langword="null"/> if it is unknown.
		/// </summary>
		public string SubAdminArea { get; set; }

		/// <summary>
		/// Returns a string representation of the current values of <see cref="Placemark"/>.
		/// </summary>
		/// <returns>A string representation of this instance in the format of <c>Location: {value}, CountryCode: {value}, CountryName: {value}, FeatureName: {value}, PostalCode: {value}, SubLocality: {value}, Thoroughfare: {value}, SubThoroughfare: {value}, Locality: {value}, AdminArea: {value}, SubAdminArea: {value}</c>.</returns>
		public override string ToString() =>
			$"{nameof(Location)}: {Location}, {nameof(CountryCode)}: {CountryCode}, " +
			$"{nameof(CountryName)}: {CountryName}, {nameof(FeatureName)}: {FeatureName}, " +
			$"{nameof(PostalCode)}: {PostalCode}, {nameof(SubLocality)}: {SubLocality}, " +
			$"{nameof(Thoroughfare)}: {Thoroughfare}, {nameof(SubThoroughfare)}: {SubThoroughfare}, " +
			$"{nameof(Locality)}: {Locality}, {nameof(AdminArea)}: {AdminArea}, " +
			$"{nameof(SubAdminArea)}: {SubAdminArea}";
	}
}
