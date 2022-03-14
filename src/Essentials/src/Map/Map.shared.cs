#nullable enable
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public interface IMap
	{
		Task OpenMapsAsync(double latitude, double longitude, MapLaunchOptions options);
		Task OpenMapsAsync(Placemark placemark, MapLaunchOptions options);
	}
	/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Map']/Docs" />
	public static class Map
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][1]/Docs" />
		public static Task OpenAsync(Location location) =>
			OpenAsync(location, new MapLaunchOptions());

		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][4]/Docs" />
		public static Task OpenAsync(Location location, MapLaunchOptions options)
		{
			if (location == null)
				throw new ArgumentNullException(nameof(location));

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return Current.OpenMapsAsync(location.Latitude, location.Longitude, options);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][3]/Docs" />
		public static Task OpenAsync(double latitude, double longitude) =>
			OpenAsync(latitude, longitude, new MapLaunchOptions());

		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][6]/Docs" />
		public static Task OpenAsync(double latitude, double longitude, MapLaunchOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return Current.OpenMapsAsync(latitude, longitude, options);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][2]/Docs" />
		public static Task OpenAsync(Placemark placemark) =>
			OpenAsync(placemark, new MapLaunchOptions());

		/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="//Member[@MemberName='OpenAsync'][5]/Docs" />
		public static Task OpenAsync(Placemark placemark, MapLaunchOptions options)
		{
			if (placemark == null)
				throw new ArgumentNullException(nameof(placemark));

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return Current.OpenMapsAsync(placemark, options);
		}
		static IMap? currentImplementation;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IMap Current =>
			currentImplementation ??= new MapImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetCurrent(IMap? implementation) =>
			currentImplementation = implementation;
	}
}
