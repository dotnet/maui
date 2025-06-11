namespace Microsoft.Maui.Controls
{
	public partial class BoxView
	{
		internal static new void RemapForControls()
		{
			ShapeViewHandler.Mapper.ReplaceMapping<BoxView, IShapeViewHandler>(nameof(Background), MapBackground);
		}
	}
}