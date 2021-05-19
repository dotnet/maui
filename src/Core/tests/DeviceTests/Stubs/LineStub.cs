using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class LineStub : ILine
	{
		public LineStub()
		{

		}

		public LineStub(double x1, double y1, double x2, double y2)
		{
			X1 = x1;
			Y1 = y1;
			X2 = x2;
			Y2 = y2;
		}

		public double X1 { get; set; }

		public double Y1 { get; set; }

		public double X2 { get; set; }

		public double Y2 { get; set; }
	}
}