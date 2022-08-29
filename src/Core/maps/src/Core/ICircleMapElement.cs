using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Maps
{
	public interface ICircleMapElement : IMapElement, IFilledMapElement
	{
		Location Center { get; }
		Distance Radius { get; }
	}
}
