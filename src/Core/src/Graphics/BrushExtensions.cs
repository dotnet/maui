#nullable enable
namespace Microsoft.Maui.Graphics
{
	public static partial class BrushExtensions
	{
		public static bool IsNullOrEmpty(this IBrush? brush) =>
			brush == null || brush.IsEmpty;
	}
}