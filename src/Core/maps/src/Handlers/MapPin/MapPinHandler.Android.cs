using System;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapPinHandler : ElementHandler<IMapPin, MarkerOptions>
	{
		// Keep track of the actual marker associated with this handler using a weak reference
		// to avoid potential memory leaks (the Marker is owned by the Google Maps view)
		WeakReference<Marker>? _markerWeakReference;

		internal Marker? Marker
		{
			get => _markerWeakReference?.TryGetTarget(out var marker) == true ? marker : null;
			set => _markerWeakReference = value is not null ? new WeakReference<Marker>(value) : null;
		}

		protected override MarkerOptions CreatePlatformElement() => new MarkerOptions();

		protected override void DisconnectHandler(MarkerOptions platformView)
		{
			// Clean up the weak reference to avoid potential memory leaks
			_markerWeakReference = null;
			base.DisconnectHandler(platformView);
		}

		public static void MapLocation(IMapPinHandler handler, IMapPin mapPin)
		{
			if (mapPin.Location is null)
			{
				return;
			}

			// Always update the MarkerOptions
			var position = new LatLng(mapPin.Location.Latitude, mapPin.Location.Longitude);
			handler.PlatformView.SetPosition(position);

			// Update the actual marker if available
			UpdateMarker(handler, marker => marker.Position = position);
		}

		public static void MapLabel(IMapPinHandler handler, IMapPin mapPin)
		{
			handler.PlatformView.SetTitle(mapPin.Label);

			// Update the actual marker if available
			UpdateMarker(handler, marker => marker.Title = mapPin.Label);
		}

		public static void MapAddress(IMapPinHandler handler, IMapPin mapPin)
		{
			handler.PlatformView.SetSnippet(mapPin.Address);

			// Update the actual marker if available
			UpdateMarker(handler, marker => marker.Snippet = mapPin.Address);
		}

		static void UpdateMarker(IMapPinHandler handler, Action<Marker> updateAction)
		{
			if (handler is MapPinHandler mapPinHandler && mapPinHandler.Marker is Marker marker)
			{
				updateAction(marker);
			}
		}
	}
}
