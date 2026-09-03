#if __IOS__ || MACCATALYST
using PlatformView = MapKit.MKOverlayRenderer;
#elif MONOANDROID
using PlatformView = Java.Lang.Object;
#elif WINDOWS
using PlatformView = System.Object;
#elif TIZEN
using PlatformView = System.Object;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0 && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Maps.Handlers
{
	/// <summary>
	/// Handler for <see cref="IMapElement"/> objects that manages the platform-specific implementation.
	/// </summary>
	public partial class MapElementHandler : IMapElementHandler
	{
		/// <summary>
		/// The property mapper that maps cross-platform properties to platform-specific methods.
		/// </summary>
		public static IPropertyMapper<IMapElement, IMapElementHandler> Mapper = new PropertyMapper<IMapElement, IMapElementHandler>(ElementMapper)
		{
			[nameof(IMapElement.Stroke)] = MapStroke,
			[nameof(IMapElement.StrokeThickness)] = MapStrokeThickness,
			[nameof(IFilledMapElement.Fill)] = MapFill,
#if MONOANDROID
			["Geopath"] = MapGeopath,
			[nameof(ICircleMapElement.Radius)] = MapRadius,
			[nameof(ICircleMapElement.Center)] = MapCenter,
#endif
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="MapElementHandler"/> class with the default mapper.
		/// </summary>
		public MapElementHandler() : base(Mapper)
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MapElementHandler"/> class with an optional custom mapper.
		/// </summary>
		/// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
		public MapElementHandler(IPropertyMapper? mapper = null)
		: base(mapper ?? Mapper)
		{
		}

		IMapElement IMapElementHandler.VirtualView => VirtualView;

		PlatformView IMapElementHandler.PlatformView => PlatformView;
	}
}
