namespace Microsoft.Maui.Graphics
{
	public abstract class Paint
	{
		public Color BackgroundColor { get; set; }

		public Color ForegroundColor { get; set; }

		public virtual bool IsTransparent { get; }
	}
}