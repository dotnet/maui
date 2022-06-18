namespace GraphicsTester.Scenarios
{
	public class FillEllipses : AbstractFillScenario
	{
		public FillEllipses()
			: base((canvas, x, y, width, height) => canvas.FillEllipse(x, y, width, height))
		{
		}
	}
}
