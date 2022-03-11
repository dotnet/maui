using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.UnitTests
{
	class ShadowStub : IShadow
	{
		public float Radius { get; set; }

		public float Opacity { get; set; }

		public Paint Paint { get; set; }

		public Point Offset { get; set; }
	}
}
