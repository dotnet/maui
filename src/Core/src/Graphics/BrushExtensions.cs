namespace Microsoft.Maui.Graphics
{
	public static class BrushExtensions
	{
		public static bool IsNullOrEmpty(this IBrush brush) =>
			brush == null || brush.IsEmpty;
	}
}