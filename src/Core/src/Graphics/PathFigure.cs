namespace Microsoft.Maui.Graphics
{
	public sealed class PathFigure 
	{
		public PathFigure()
		{
			Segments = new PathSegmentCollection();
		}

		public PathSegmentCollection Segments { get; set; }

		public Point StartPoint { get; set; } = new Point(0, 0);

		public bool IsClosed { get; set; } = false;

		public bool IsFilled { get; set; } = true;
	}
}