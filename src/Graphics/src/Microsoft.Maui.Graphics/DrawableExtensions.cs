namespace Microsoft.Maui.Graphics
{
	public static class DrawableExtensions
	{
		public static IImage ToImage(this IDrawable drawable, int width, int height, float scale = 1)
		{
			if (drawable == null) return null;

			using (var context = GraphicsPlatform.CurrentService.CreateBitmapExportContext(width, height))
			{
				context.Canvas.Scale(scale, scale);
				drawable.Draw(context.Canvas, new RectangleF(0, 0, (float) width / scale, (float) height / scale));
				return context.Image;
			}
		}
	}
}
