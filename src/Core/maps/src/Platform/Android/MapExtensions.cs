using System;
using System.Threading.Tasks;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Maps.Handlers;

namespace Microsoft.Maui.Maps.Platform
{
	public static class MapExtensions
	{
		public static void UpdateMapType(this GoogleMap googleMap, IMap map)
		{
			if (googleMap == null)
				return;

			googleMap.MapType = map.MapType switch
			{
				MapType.Street => GoogleMap.MapTypeNormal,
				MapType.Satellite => GoogleMap.MapTypeSatellite,
				MapType.Hybrid => GoogleMap.MapTypeHybrid,
				_ => throw new ArgumentOutOfRangeException(),
			};
		}

		public static void UpdateIsShowingUser(this GoogleMap googleMap, IMap map, IMauiContext? mauiContext)
		{
			if (googleMap == null)
				return;

			if (mauiContext?.Context == null)
				return;

			googleMap.SetIsShowingUser(map, mauiContext).FireAndForget();
		}

		public static void UpdateIsScrollEnabled(this GoogleMap googleMap, IMap map)
		{
			if (googleMap == null)
				return;

			googleMap.UiSettings.ScrollGesturesEnabled = map.IsScrollEnabled;
		}

		public static void UpdateIsTrafficEnabled(this GoogleMap googleMap, IMap map)
		{
			if (googleMap == null)
				return;

			googleMap.TrafficEnabled = map.IsTrafficEnabled;
		}

		public static void UpdateIsZoomEnabled(this GoogleMap googleMap, IMap map)
		{
			if (googleMap == null)
				return;

			googleMap.UiSettings.ZoomControlsEnabled = map.IsZoomEnabled;
			googleMap.UiSettings.ZoomGesturesEnabled = map.IsZoomEnabled;
		}

		public static void UpdateMapStyle(this GoogleMap googleMap, IMap map)
		{
			if (googleMap == null)
				return;

			bool result;
			if (string.IsNullOrEmpty(map.MapStyle))
			{
				result = googleMap.SetMapStyle(null);
			}
			else
			{
				var styleOptions = new MapStyleOptions(map.MapStyle);
				result = googleMap.SetMapStyle(styleOptions);
			}

			if (!result)
			{
				System.Diagnostics.Debug.WriteLine("Failed to apply map style. The provided JSON style may be invalid.");
			}
		}

		internal static async Task SetIsShowingUser(this GoogleMap googleMap, IMap map, IMauiContext? mauiContext)
		{
			if (map.IsShowingUser)
			{
				var locationStatus = await ApplicationModel.Permissions.CheckStatusAsync<ApplicationModel.Permissions.LocationWhenInUse>();

				if (locationStatus == ApplicationModel.PermissionStatus.Granted)
				{
					googleMap.MyLocationEnabled = googleMap.UiSettings.MyLocationButtonEnabled = true;
				}
				else
				{
					var locationResult = await ApplicationModel.Permissions.RequestAsync<ApplicationModel.Permissions.LocationWhenInUse>();
					if (locationResult == ApplicationModel.PermissionStatus.Granted)
					{
						googleMap.MyLocationEnabled = googleMap.UiSettings.MyLocationButtonEnabled = true;
						return;
					}
					else
					{
						mauiContext?.Services.GetService<ILogger<MapHandler>>()?.LogWarning("Missing location permissions for IsShowingUser");
						googleMap.MyLocationEnabled = googleMap.UiSettings.MyLocationButtonEnabled = false;
					}
				}
			}
			else
			{
				googleMap.MyLocationEnabled = googleMap.UiSettings.MyLocationButtonEnabled = false;
			}
		}
	}
}
