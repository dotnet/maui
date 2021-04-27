namespace Microsoft.Maui.Graphics
{
	public class ImagePaint : Paint
	{
		public IImage Image { get; set; }

		public override bool IsTransparent => false;
	}
}