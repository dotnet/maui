using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using Windows.Devices.Geolocation;

namespace Microsoft.Maui.Maps.Handlers
{
	/// <summary>
	/// Handler for map pins on Windows using the WinUI 3 MapIcon.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>Platform Limitations (Windows/WinUI 3):</b>
	/// <list type="bullet">
	/// <item><description><b>Label:</b> MapIcon does not have a built-in label property. 
	/// To display pin labels, use a custom XAML overlay or tooltip.</description></item>
	/// <item><description><b>Address:</b> MapIcon does not display address information. 
	/// Address display requires custom UI implementation.</description></item>
	/// <item><description><b>Custom Icons:</b> MapIcon has limited customization. 
	/// Custom pin images require additional platform-specific code.</description></item>
	/// <item><description><b>Info Windows:</b> Tap-to-show-info-window pattern is not built-in. 
	/// Implement custom flyouts or popups for pin information display.</description></item>
	/// </list>
	/// </para>
	/// </remarks>
	public partial class MapPinHandler : ElementHandler<IMapPin, object>
	{
		/// <inheritdoc/>
		protected override object CreatePlatformElement()
		{
			return new MapIcon
			{
				Location = new Geopoint(new BasicGeoposition
				{
					Latitude = VirtualView.Location.Latitude,
					Longitude = VirtualView.Location.Longitude
				})
			};
		}

		/// <summary>
		/// Maps the <see cref="IMapPin.Location"/> property to the platform element.
		/// </summary>
		public static void MapLocation(IMapPinHandler handler, IMapPin mapPin)
		{
			if (handler.PlatformView is MapIcon mapIcon)
			{
				mapIcon.Location = new Geopoint(new BasicGeoposition
				{
					Latitude = mapPin.Location.Latitude,
					Longitude = mapPin.Location.Longitude
				});
			}
		}

		/// <summary>
		/// Maps the <see cref="IMapPin.Label"/> property to the platform element.
		/// </summary>
		/// <remarks>
		/// <b>Windows Limitation:</b> The WinUI 3 MapIcon does not support labels directly.
		/// This method is a no-op on Windows. To display labels, implement a custom overlay.
		/// </remarks>
		public static void MapLabel(IMapPinHandler handler, IMapPin mapPin)
		{
			// The WinUI 3 MapIcon doesn't have a direct label property
			// Labels would need to be displayed via tooltips or custom UI overlay
		}

		/// <summary>
		/// Maps the <see cref="IMapPin.Address"/> property to the platform element.
		/// </summary>
		/// <remarks>
		/// <b>Windows Limitation:</b> The WinUI 3 MapIcon does not support address display.
		/// This method is a no-op on Windows. To display addresses, implement a custom overlay or info window.
		/// </remarks>
		public static void MapAddress(IMapPinHandler handler, IMapPin mapPin)
		{
			// The WinUI 3 MapIcon doesn't have an address property
			// Address display would need custom implementation
		}
	}
}
