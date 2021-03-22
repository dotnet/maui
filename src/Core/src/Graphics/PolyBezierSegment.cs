namespace Microsoft.Maui.Graphics
{
	public class PolyBezierSegment : PathSegment
	{
		public PolyBezierSegment()
		{
			Points = new PointCollection();
		}

		public PolyBezierSegment(PointCollection points)
		{
			Points = points;
		}

		public PointCollection Points { get; set; }
	}
}