using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IShadow
	{
		double Radius { get; }

		float Opacity { get; }

		Paint Paint { get; }

		Size Offset { get; }
	}
}
