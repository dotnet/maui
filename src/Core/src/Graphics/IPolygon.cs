namespace Microsoft.Maui.Graphics
{
	public interface IPolygon : IShape
	{
		public PointCollection? Points { get; set; }
	}
}