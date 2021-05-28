using Microsoft.Maui.Layouts;


namespace Microsoft.Maui.Controls
{
	// Don't panic, Layout2.StackLayout is the temporary name for the abstract base class until
	// we rename everything and move the legacy layout
	[ContentProperty(nameof(Children))]
	public class VerticalStackLayout : Layout2.StackLayout
	{
		protected override ILayoutManager CreateLayoutManager() => new VerticalStackLayoutManager(this);
	}
}
