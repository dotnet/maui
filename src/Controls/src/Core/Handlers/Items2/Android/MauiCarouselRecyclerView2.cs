#nullable disable
using System;
using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Carousel;

namespace Microsoft.Maui.Controls.Handlers.Items2;

/// <summary>
/// A <see cref="Items.MauiCarouselRecyclerView"/> variant that uses the Material Design
/// <see cref="CarouselLayoutManager"/> (with <see cref="FullScreenCarouselStrategy"/>) instead of
/// <see cref="LinearLayoutManager"/>.
///
/// All MAUI CarouselView API surface (Position, CurrentItem, IsSwipeEnabled, IsBounceEnabled,
/// PeekAreaInsets, ItemsLayout) is preserved by inheriting the existing scroll and visual-state
/// machinery from <see cref="Items.MauiCarouselRecyclerView"/>.
///
/// <para>
/// <b>Looping is not supported on this handler.</b> Material's <see cref="CarouselLayoutManager"/>
/// has no concept of a virtual range, so the LoopScale (≈16384) trick used by MAUI's
/// LinearLayoutManager-based implementation does not work. Callers must keep
/// <see cref="CarouselView.Loop"/> set to <c>false</c>; <see cref="CarouselViewAdapter2.ItemCount"/>
/// is locked to <c>ItemsSource.Count</c> to guard the adapter side.
/// </para>
/// </summary>
internal class MauiCarouselRecyclerView2 :
    Items.MauiCarouselRecyclerView,
    IMauiCarouselRecyclerView2
{
    CarouselSnapHelper _carouselSnapHelper;
    bool _disposed;

    public MauiCarouselRecyclerView2(
        Context context,
        Func<IItemsLayout> getItemsLayout,
        Func<Items.ItemsViewAdapter<CarouselView, Items.IItemsViewSource>> getAdapter)
        : base(context, getItemsLayout, getAdapter)
    {
    }

    // Material's CarouselLayoutManager has no virtual-range concept, so the
    // LoopScale (16384) trick used by the legacy LinearLayoutManager path does
    // not apply here. Force every Loop-aware code path in the base class to
    // take the non-loop branch regardless of CarouselView.Loop.
    // TODO: Remove this override once a true looping mechanism is implemented
    // for the Material3 handler (e.g. edge-jump strategy, duplicate-buffer
    // adapter, or a CarouselLayoutManager fork with virtual-range support).
    protected override bool IsLoopEnabled => false;

    // -----------------------------------------------------------------------
    // Layout manager — swap LinearLayoutManager for CarouselLayoutManager
    // -----------------------------------------------------------------------

    protected override LayoutManager SelectLayoutManager(IItemsLayout layoutSpecification)
    {
        var orientation = RecyclerView.Horizontal;

        if (layoutSpecification is LinearItemsLayout linearItemsLayout)
        {
            orientation = linearItemsLayout.Orientation == ItemsLayoutOrientation.Vertical
                ? RecyclerView.Vertical
                : RecyclerView.Horizontal;
        }

        return new CarouselLayoutManager(CreateCarouselStrategy(), orientation);
    }

    /// <summary>
    /// Creates the <see cref="CarouselStrategy"/> to use.
    ///
    /// Currently locked to <see cref="FullScreenCarouselStrategy"/>: the other Material
    /// strategies (MultiBrowse, Hero, Uncontained) require items to be smaller than the
    /// viewport, which conflicts with how Handler2 sizes items (full RecyclerView width/
    /// height via <see cref="Items.SizedItemContentView"/>). If a future change wires up
    /// strategy-aware sizing, this can become user-selectable via an attached property.
    /// </summary>
    protected virtual CarouselStrategy CreateCarouselStrategy() => new FullScreenCarouselStrategy();

    // -----------------------------------------------------------------------
    // Snap — replace MAUI snap manager with CarouselSnapHelper
    // -----------------------------------------------------------------------

    protected override void UpdateSnapBehavior()
    {
        // Detach any previous snap helper to avoid duplicate fling listeners.
        _carouselSnapHelper?.AttachToRecyclerView(null);

        // CarouselLayoutManager ships its own snap helper; attach it directly.
        // Deliberately do NOT call base.UpdateSnapBehavior() so MAUI's SnapManager
        // does not attach a conflicting snap helper.
        _carouselSnapHelper = new CarouselSnapHelper();
        _carouselSnapHelper.AttachToRecyclerView(this);
    }

    public override void UpdateLayoutManager()
    {
        base.UpdateLayoutManager();

        // The base swaps the LayoutManager; the previously attached CarouselSnapHelper
        // still references the old LayoutManager internally. Re-attach so snapping
        // continues to track the current Material CarouselLayoutManager.
        UpdateSnapBehavior();
    }

    protected override void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
    {
        // Skip the MAUI snap-manager reset (no SingleSnapHelper attached) and go straight
        // to the underlying scroll so CarouselSnapHelper continues to control snapping.
        ScrollTo(args);
    }

    // -----------------------------------------------------------------------
    // Spacing decoration — CarouselLayoutManager manages item sizes via its
    // strategy, so we use a no-op decoration. PeekAreaInsets are applied as
    // RecyclerView padding by the handler instead.
    // -----------------------------------------------------------------------

    protected override RecyclerView.ItemDecoration CreateSpacingDecoration(IItemsLayout itemsLayout)
        => new NoOpItemDecoration();

    sealed class NoOpItemDecoration : RecyclerView.ItemDecoration { }

    // -----------------------------------------------------------------------
    // Scroll listener — override to use CarouselLayoutManager-aware listener
    // -----------------------------------------------------------------------

    protected override Items.RecyclerViewScrollListener<CarouselView, Items.IItemsViewSource> CreateScrollListener()
        => new CarouselViewOnScrollListener2(Carousel, ItemsViewAdapter, () => _carouselSnapHelper);

    // -----------------------------------------------------------------------
    // Dispose / teardown
    // -----------------------------------------------------------------------

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
            _carouselSnapHelper?.AttachToRecyclerView(null);
            _carouselSnapHelper = null;
        }

        base.Dispose(disposing);
    }

    // -----------------------------------------------------------------------
    // IMauiCarouselRecyclerView2 — forward to base IMauiCarouselRecyclerView impl
    // -----------------------------------------------------------------------

    void IMauiCarouselRecyclerView2.UpdateFromCurrentItem()
        => ((Items.IMauiCarouselRecyclerView)this).UpdateFromCurrentItem();

    void IMauiCarouselRecyclerView2.UpdateFromPosition()
        => ((Items.IMauiCarouselRecyclerView)this).UpdateFromPosition();

    bool IMauiCarouselRecyclerView2.IsSwipeEnabled
    {
        get => IsSwipeEnabled;
        set => IsSwipeEnabled = value;
    }
}
