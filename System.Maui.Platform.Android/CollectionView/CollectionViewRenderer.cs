using Android.Content;

namespace System.Maui.Platform.Android
{
	public class CollectionViewRenderer : GroupableItemsViewRenderer<GroupableItemsView, GroupableItemsViewAdapter<GroupableItemsView, IGroupableItemsViewSource>, IGroupableItemsViewSource>
	{
		public CollectionViewRenderer(Context context) : base(context)
		{
		}
	}
}