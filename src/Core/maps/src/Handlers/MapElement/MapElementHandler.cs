#if __IOS__ || MACCATALYST
using PlatformView = MapKit.MKOverlayRenderer;
#elif MONOANDROID
using PlatformView = Android.Gms.Maps.Model.MarkerOptions;
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
	public partial class MapElementHandler : IMapElementHandler
	{
		public static IPropertyMapper<IMapElement, IMapElementHandler> Mapper = new PropertyMapper<IMapElement, IMapElementHandler>(ElementMapper)
		{
			[nameof(IMapElement.Stroke)] = MapStroke,
			[nameof(IMapElement.StrokeThickness)] = MapStrokeThickness,
			[nameof(IFilledMapElement.Fill)] = MapFill,
		};

		public static CommandMapper<IMapElement, IMapElementHandler> CommandMapper =
				new CommandMapper<IMapElement, IMapElementHandler>(ElementCommandMapper);

		public MapElementHandler() : base(Mapper)
		{

		}

		public MapElementHandler(IPropertyMapper? mapper = null)
		: base(mapper ?? Mapper)
		{
		}

		IMapElement IMapElementHandler.VirtualView => VirtualView;

		PlatformView IMapElementHandler.PlatformView => PlatformView;
	}
}
