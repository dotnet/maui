namespace Microsoft.Maui.Graphics
{
	public interface IDrawable
	{
		void Draw(ICanvas canvas, RectangleF dirtyRect);

		IImage ToImage(int width, int height, float scale = 1);
	}
}
