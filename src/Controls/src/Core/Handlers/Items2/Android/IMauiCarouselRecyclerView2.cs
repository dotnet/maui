namespace Microsoft.Maui.Controls.Handlers.Items2
{
    /// <summary>
    /// Interface for the Material Carousel-backed RecyclerView used by <see cref="CarouselViewHandler2"/> on Android.
    /// Mirrors <see cref="Items.IMauiCarouselRecyclerView"/> so handler map methods can call the same operations.
    /// </summary>
    public interface IMauiCarouselRecyclerView2
    {
        void UpdateFromCurrentItem();

        void UpdateFromPosition();

        bool IsSwipeEnabled { get; set; }
    }
}
