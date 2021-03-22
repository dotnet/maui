namespace Microsoft.Maui.Graphics
{
	public class PathGeometry : Geometry
	{
		public PathGeometry()
		{
			Figures = new PathFigureCollection();
		}

		public PathGeometry(PathFigureCollection figures)
		{
			Figures = figures;
		}

		public PathGeometry(PathFigureCollection figures, FillRule fillRule)
		{
			Figures = figures;
			FillRule = fillRule;
		}

		public PathFigureCollection Figures { get; set; }

		public FillRule FillRule { get; set; } = FillRule.EvenOdd;
	}
}