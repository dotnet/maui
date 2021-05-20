namespace Microsoft.Maui.Graphics
{
	public interface IPolyline : IShape
	{
		public PointCollection? Points { get; set; }
	}
}