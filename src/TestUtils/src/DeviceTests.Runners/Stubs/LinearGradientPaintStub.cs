using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class LinearGradientPaintStub : LinearGradientPaint
	{
		public LinearGradientPaintStub(Color startColor, Color endColor)
		{
			StartColor = startColor;
			EndColor = endColor;
		}
	}
}
