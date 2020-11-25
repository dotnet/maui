namespace GraphicsTester.Scenarios
{
    public class FillOvals : AbstractFillScenario
    {
        public FillOvals()
            : base((canvas, x, y, width, height) => canvas.FillOval(x, y, width, height))
        {
        }
    }
}