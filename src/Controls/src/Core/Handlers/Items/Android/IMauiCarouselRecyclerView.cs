namespace Microsoft.Maui.Controls.Handlers.Items
{
	public interface IMauiCarouselRecyclerView
	{
		void UpdateFromCurrentItem();

		void UpdateFromPosition();

		bool IsSwipeEnabled { get; set; }
	}
}
