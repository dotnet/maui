using Android.Content;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public class CollectionViewRenderer : GroupableItemsViewRenderer<GroupableItemsView, GroupableItemsViewAdapter<GroupableItemsView, IGroupableItemsViewSource>, IGroupableItemsViewSource>
	{
		public CollectionViewRenderer(Context context) : base(context)
		{
		}
	}
}