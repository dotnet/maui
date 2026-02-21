using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Maps.Handlers
{
	/// <summary>
	/// Handler for map elements (polylines, polygons, circles) on Windows.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>Platform Limitations (Windows/WinUI 3):</b>
	/// <list type="bullet">
	/// <item><description><b>Polylines:</b> The WinUI 3 MapElementsLayer does not support polylines directly.
	/// To render polylines, consider using Azure Maps REST API, Web SDK in a WebView, or custom XAML overlays.</description></item>
	/// <item><description><b>Polygons:</b> The WinUI 3 MapElementsLayer does not support polygons directly.
	/// To render polygons, consider using Azure Maps REST API, Web SDK in a WebView, or custom XAML overlays.</description></item>
	/// <item><description><b>Circles:</b> The WinUI 3 MapElementsLayer does not support circles directly.
	/// To render circles, approximate with a polygon or use Azure Maps REST API.</description></item>
	/// <item><description><b>Stroke/Fill:</b> MapIcon (the only fully supported element type) does not support
	/// stroke or fill properties. These properties are no-ops on Windows.</description></item>
	/// </list>
	/// </para>
	/// <para>
	/// The current implementation returns a MapIcon as a placeholder. For full shape support,
	/// consider integrating the Azure Maps Web SDK via a WebView control.
	/// </para>
	/// </remarks>
	public partial class MapElementHandler : ElementHandler<IMapElement, object>
	{
		/// <inheritdoc/>
		/// <remarks>
		/// <b>Windows Limitation:</b> Returns a MapIcon as a placeholder since the WinUI 3 MapControl
		/// does not support polylines, polygons, or circles in MapElementsLayer.
		/// </remarks>
		protected override object CreatePlatformElement()
		{
			// The WinUI 3 MapControl primarily supports MapIcon through MapElementsLayer
			// For polylines, polygons, and circles, we return a minimal MapElement
			// Full shape support would require additional Azure Maps integration
			return new MapIcon();
		}

		/// <summary>
		/// Maps the <see cref="IStroke.Stroke"/> property to the platform element.
		/// </summary>
		/// <remarks>
		/// <b>Windows Limitation:</b> Stroke is not supported on MapIcon.
		/// This method is a no-op on Windows. For shape rendering, use Azure Maps Web SDK.
		/// </remarks>
		public static void MapStroke(IMapElementHandler handler, IMapElement mapElement)
		{
			// Stroke is not directly supported on MapIcon
			// Would need custom shape implementation for polylines/polygons
		}

		/// <summary>
		/// Maps the <see cref="IStroke.StrokeThickness"/> property to the platform element.
		/// </summary>
		/// <remarks>
		/// <b>Windows Limitation:</b> Stroke thickness is not supported on MapIcon.
		/// This method is a no-op on Windows. For shape rendering, use Azure Maps Web SDK.
		/// </remarks>
		public static void MapStrokeThickness(IMapElementHandler handler, IMapElement mapElement)
		{
			// Stroke thickness is not directly supported on MapIcon
			// Would need custom shape implementation for polylines/polygons
		}

		/// <summary>
		/// Maps the <see cref="IFilledMapElement.Fill"/> property to the platform element.
		/// </summary>
		/// <remarks>
		/// <b>Windows Limitation:</b> Fill is not supported on MapIcon.
		/// This method is a no-op on Windows. For shape rendering, use Azure Maps Web SDK.
		/// </remarks>
		public static void MapFill(IMapElementHandler handler, IMapElement mapElement)
		{
			// Fill is not directly supported on MapIcon
			// Would need custom shape implementation for polygons/circles
		}
	}
}
