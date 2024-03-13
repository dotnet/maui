using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapElementHandler : ElementHandler<IMapElement, object>
	{
		protected override object CreatePlatformElement() => new();

		[MissingMapper]
		public static void MapStroke(IMapElementHandler handler, IMapElement mapElement) { }

		[MissingMapper]
		public static void MapStrokeThickness(IMapElementHandler handler, IMapElement mapElement) { }

		[MissingMapper]
		public static void MapFill(IMapElementHandler handler, IMapElement mapElement) { }
	}
}