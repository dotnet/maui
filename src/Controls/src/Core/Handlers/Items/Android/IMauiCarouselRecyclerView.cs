namespace Microsoft.Maui.Controls.Handlers.Items
{
	public interface IMauiCarouselRecyclerView
	{
		void UpdateFromCurrentItem();

		void UpdateFromPosition();

		void UpdateItemsLayout();

		bool IsSwipeEnabled { get; set; }
	}
}
