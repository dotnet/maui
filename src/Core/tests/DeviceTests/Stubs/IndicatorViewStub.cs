namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class IndicatorViewStub : StubBase, IIndicatorView
	{
		public int Count { get; set; }

		public int Position { get; set; }

		public double IndicatorSize { get; set; } = 6.0d;

		public int MaximumVisible { get; set; } = int.MaxValue;

		public bool HideSingle { get; set; }

		public Paint IndicatorColor { get; set; } = new SolidPaint(Colors.Black);

		public Paint SelectedIndicatorColor { get; set; } = new SolidPaint(Colors.Grey);

		public IShape IndicatorsShape { get; set; }
	}
}