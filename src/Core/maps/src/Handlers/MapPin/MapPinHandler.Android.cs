using Android.Gms.Maps.Model;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapPinHandler : ElementHandler<IMapPin, MarkerOptions>
	{
		/// <summary>
		/// Cache key of the image source whose icon is currently set on the handler's MarkerOptions,
		/// or <see langword="null"/> when none is. The MarkerOptions instance outlives the markers
		/// created from it, so this lets MapHandler skip a load it has already done.
		/// </summary>
		internal string? AppliedImageSourceKey { get; set; }

		protected override MarkerOptions CreatePlatformElement() => new MarkerOptions();

		public static void MapLocation(IMapPinHandler handler, IMapPin mapPin)
		{
			if (mapPin.Location is not null)
			{
				handler.PlatformView.SetPosition(new LatLng(mapPin.Location.Latitude, mapPin.Location.Longitude));
			}
		}

		public static void MapLabel(IMapPinHandler handler, IMapPin mapPin)
		{
			handler.PlatformView.SetTitle(mapPin.Label);
		}

		public static void MapAddress(IMapPinHandler handler, IMapPin mapPin)
		{
			handler.PlatformView.SetSnippet(mapPin.Address);
		}

		// Note: the icon itself is applied in MapHandler.AddPinAsync, because it must be set on
		// MarkerOptions BEFORE calling Map.AddMarker(). This mapper only invalidates the record of
		// which source the current icon came from: it runs whenever ImageSource is set and whenever
		// the handler is attached to a pin, so it is the one place that always sees the icon on the
		// MarkerOptions go stale - including a handler reconnected to a different pin, which keeps
		// its PlatformView and would otherwise inherit the previous pin's key.
		public static void MapImageSource(IMapPinHandler handler, IMapPin mapPin)
		{
			if (handler is MapPinHandler mapPinHandler)
				mapPinHandler.AppliedImageSourceKey = null;
		}
	}
}
