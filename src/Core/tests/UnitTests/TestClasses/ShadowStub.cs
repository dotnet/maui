using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.UnitTests
{
	class ShadowStub : IShadow
	{
		public double Radius { get; set; }

		public double Opacity { get; set; }

		public Paint Paint { get; set; }

		public Size Offset { get; set; }
	}
}
