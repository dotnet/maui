namespace Microsoft.Maui.Graphics
{
	public class RectangleGeometry : Geometry
	{
		public RectangleGeometry()
		{

		}

		public RectangleGeometry(Rect rect)
		{
			Rect = rect;
		}

		public Rect Rect { get; set; }
	}
}