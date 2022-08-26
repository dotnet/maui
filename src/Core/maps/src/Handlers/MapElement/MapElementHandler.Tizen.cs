using Microsoft.Maui.Handlers;
namespace Microsoft.Maui.Maps.Handlers
{
	public partial class MapElementHandler : ElementHandler<IMapElement, ElmSharp.EvasObject>
	{
		protected override ElmSharp.EvasObject CreatePlatformElement() => throw new System.NotImplementedException();
		public static void MapStroke(IMapElementHandler handler, IMapElement mapElement) => throw new System.NotImplementedException();
		public static void MapStrokeThickness(IMapElementHandler handler, IMapElement mapElement) => throw new System.NotImplementedException();
		public static void MapFill(IMapElementHandler handler, IMapElement mapElement) => throw new System.NotImplementedException();
	}
}
