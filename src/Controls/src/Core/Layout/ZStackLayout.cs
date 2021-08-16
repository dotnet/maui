using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Children))]
	public class ZStackLayout : StackBase
	{
		protected override ILayoutManager CreateLayoutManager() => new ZStackLayoutManager(this);
	}
}
