using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Phone.Maps.Services;

namespace Xamarin.Forms.Maps.WP8
{
	internal class GeocoderBackend
	{
		// Eventually this should be sanely set to match either where the map is centered or where the user is.
		internal static Position PositionForGeocoding { get; set; }

		public static void Register()
		{
			Geocoder.GetPositionsForAddressAsyncFunc = GetPositionsForAddress;
			Geocoder.GetAddressesForPositionFuncAsync = GetAddressesForPositionAsync;
		}

		// Thank you to craig dunn
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
			if (!"".Equals(address.BuildingZone))
				bldg.Add(address.BuildingZone);
			if (bldg.Count > 0)
				building = string.Join(" ", bldg) + Environment.NewLine;

			var hs = new List<string>();
			if (!"".Equals(address.HouseNumber))
				hs.Add(address.HouseNumber);
			if (!"".Equals(address.Street))
				hs.Add(address.Street);
			if (hs.Count > 0)
				house = string.Join(" ", hs) + Environment.NewLine;

			var cs = new List<string>();
			if (!"".Equals(address.City))
				cs.Add(address.City);
			if (!"".Equals(address.State))
				cs.Add(address.State);
			else if (!"".Equals(address.Province))
				cs.Add(address.Province);
			if (!"".Equals(address.PostalCode))
				cs.Add(address.PostalCode);
			if (cs.Count > 0)
				city = string.Join(" ", cs) + Environment.NewLine;

			if (!"".Equals(address.Country))
				country = address.Country;
			return building + house + city + country;
		}

		static Task<IEnumerable<string>> GetAddressesForPositionAsync(Position position)
		{
			var source = new TaskCompletionSource<IEnumerable<string>>();

			var query = new ReverseGeocodeQuery
			{
				GeoCoordinate = new GeoCoordinate(position.Latitude, position.Longitude)
			};
			query.QueryCompleted +=
				(sender, args) => source.SetResult(args.Result.Select(r => AddressToString(r.Information.Address)).ToArray());
			query.QueryAsync();

			return source.Task;
		}

		static Task<IEnumerable<Position>> GetPositionsForAddress(string s)
		{
			var source = new TaskCompletionSource<IEnumerable<Position>>();
			var query = new GeocodeQuery
			{
				SearchTerm = s,
				GeoCoordinate = new GeoCoordinate(PositionForGeocoding.Latitude, PositionForGeocoding.Longitude)
			};
			query.QueryCompleted +=
				(sender, args) =>
					source.SetResult(
						args.Result.Select(r => new Position(r.GeoCoordinate.Latitude, r.GeoCoordinate.Longitude)).ToArray());
			query.QueryAsync();

			return source.Task;
		}
	}
}