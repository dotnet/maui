using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Maps
{
	public interface IFilledMapElement : IMapElement
	{
		Paint? Fill { get; }
	}
}
