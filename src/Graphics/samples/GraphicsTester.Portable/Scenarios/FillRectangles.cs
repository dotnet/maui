namespace GraphicsTester.Scenarios
{
	public class FillRectangles : AbstractFillScenario
	{
		public FillRectangles()
			: base((canvas, x, y, width, height) => canvas.FillRectangle(x, y, width, height))
		{
		}
	}
}
