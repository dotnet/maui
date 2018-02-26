using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Caboodle
{
    public class Location
	{
        public Location()
        {

        }

        public Location(Location location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

			if (location.Point == null)
				Point = new GeoPoint();
			else
				Point = new GeoPoint(location.Point);

			CountryCode = location.CountryCode;
			CountryName = location.CountryName;
			FeatureName = location.FeatureName;
			PostalCode = location.PostalCode;
			Locality = location.Locality;
			SubLocality = location.SubLocality;
			Thoroughfare = location.Thoroughfare;
			SubThoroughfare = location.SubThoroughfare;
			SubAdminArea = location.SubAdminArea;
			AdminArea = location.AdminArea;
		}

		public GeoPoint Point { get; set; }

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
		
    }
}
