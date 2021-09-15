using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IShadow
	{
		float Radius { get; }

		float Opacity { get; }

		Paint Paint { get; }

		Point Offset { get; }
	}
}
