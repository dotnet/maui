using System;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapPinHandler : ElementHandler<IMapPin, MarkerOptions>
	{
		protected override MarkerOptions CreatePlatformElement() => new MarkerOptions();

		protected override void DisconnectHandler(MarkerOptions platformView)
		{
			if (VirtualView.MarkerId is Marker marker)
			{
				marker.Remove();
				VirtualView.MarkerId = null;
			}

			base.DisconnectHandler(platformView);
		}

		static Marker? EnsureMarker(IMapPinHandler handler, IMapPin mapPin)
		{
			if (mapPin.MarkerId is Marker existingMarker)
			{
				return existingMarker;
			}

			GoogleMap? googleMap = GetGoogleMap(mapPin);
			if (googleMap == null)
			{
				return null;
			}

			var marker = googleMap.AddMarker(handler.PlatformView) ?? throw new Exception("GoogleMap.AddMarker returned null");

			mapPin.MarkerId = marker;
			return marker;
		}

		static GoogleMap? GetGoogleMap(IMapPin mapPin)
		{
			if (mapPin.Parent is IMap map && map.Handler is MapHandler mapHandler)
			{
				return mapHandler.Map;
			}

			return null;
		}

		public static void MapLocation(IMapPinHandler handler, IMapPin mapPin)
		{
			if (mapPin.Location is null)
			{
				return;
			}

			var position = new LatLng(mapPin.Location.Latitude, mapPin.Location.Longitude);
			// Set position on MarkerOptions BEFORE creating marker
			handler.PlatformView.SetPosition(position);

			var marker = EnsureMarker(handler, mapPin);
			// Update marker position if it was successfully created
			if (marker is not null)
			{
				marker.Position = position;
			}
		}

		public static void MapLabel(IMapPinHandler handler, IMapPin mapPin)
		{
			var marker = EnsureMarker(handler, mapPin);
			if (marker is not null)
			{
				marker.Title = mapPin.Label;
			}
			else
			{
				handler.PlatformView.SetTitle(mapPin.Label);
			}
		}

		public static void MapAddress(IMapPinHandler handler, IMapPin mapPin)
		{
			var marker = EnsureMarker(handler, mapPin);
			if (marker is not null)
			{
				marker.Snippet = mapPin.Address;
			}
			else
			{
				handler.PlatformView.SetSnippet(mapPin.Address);
			}
		}
	}
}
