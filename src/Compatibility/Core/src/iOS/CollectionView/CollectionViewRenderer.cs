using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class CollectionViewRenderer : GroupableItemsViewRenderer<GroupableItemsView, GroupableItemsViewController<GroupableItemsView>>
	{
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public CollectionViewRenderer() { }
	}
}