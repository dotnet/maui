using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;

namespace Xamarin.Forms.Maps.UWP
{
	internal class GeocoderBackend
	{
		public static void Register()
		{
			Geocoder.GetPositionsForAddressAsyncFunc = GetPositionsForAddress;
			Geocoder.GetAddressesForPositionFuncAsync = GetAddressesForPositionAsync;
		}

		static string AddressToString(MapAddress address)
		{
			string building = "", house = "", city = "", country = "";

			var bldg = new List<string>();
			if (!"".Equals(address.BuildingRoom))
				bldg.Add(address.BuildingRoom);
			if (!"".Equals(address.BuildingFloor))
				bldg.Add(address.BuildingFloor);
			if (!"".Equals(address.BuildingName))
				bldg.Add(address.BuildingName);
			if (!"".Equals(address.BuildingWing))
				bldg.Add(address.BuildingWing);
			if (bldg.Count > 0)
				building = string.Join(" ", bldg) + Environment.NewLine;

			var hs = new List<string>();
			if (!"".Equals(address.StreetNumber))
				hs.Add(address.StreetNumber);
			if (!"".Equals(address.Street))
				hs.Add(address.Street);
			if (hs.Count > 0)
				house = string.Join(" ", hs) + Environment.NewLine;

			var cs = new List<string>();
			if (!"".Equals(address.Town))
				cs.Add(address.Town);
			if (!"".Equals(address.Neighborhood))
				cs.Add(address.Neighborhood);
			else if (!"".Equals(address.Region))
				cs.Add(address.Region);
			if (!"".Equals(address.PostCode))
				cs.Add(address.PostCode);
			if (cs.Count > 0)
				city = string.Join(" ", cs) + Environment.NewLine;

			if (!"".Equals(address.Country))
				country = address.Country;
			return building + house + city + country;
		}

		static async Task<IEnumerable<string>> GetAddressesForPositionAsync(Position position)
		{
			var queryResults =
				await
					MapLocationFinder.FindLocationsAtAsync(
						new Geopoint(new BasicGeoposition { Latitude = position.Latitude, Longitude = position.Longitude }));
			var addresses = new List<string>();
			foreach (var result in queryResults?.Locations)
				addresses.Add(AddressToString(result.Address));

			return addresses;
		}

		static async Task<IEnumerable<Position>> GetPositionsForAddress(string address)
		{
			var queryResults = await MapLocationFinder.FindLocationsAsync(address, null, 10);
			var positions = new List<Position>();
			foreach (var result in queryResults?.Locations)
				positions.Add(new Position(result.Point.Position.Latitude, result.Point.Position.Longitude));
			return positions;
		}
	}
}