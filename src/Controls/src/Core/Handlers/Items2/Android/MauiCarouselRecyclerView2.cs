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
/// Most MAUI CarouselView API surface (Position, CurrentItem, IsSwipeEnabled, IsBounceEnabled,
/// ItemsLayout) is preserved by inheriting the existing scroll machinery from
/// <see cref="Items.MauiCarouselRecyclerView"/>.
///
/// <para>
/// <b>Per-item visual states are not supported on this handler.</b> The inherited visual-state
/// machinery only runs for a <see cref="LinearLayoutManager"/> with ItemContentView children, so it
/// does not drive the CurrentItem/PreviousItem/NextItem/DefaultItem states (or
/// <see cref="CarouselView.VisibleViews"/>) under Material's <see cref="CarouselLayoutManager"/>. A
/// Material-specific override was intentionally omitted: the documented visual-state scenarios
/// (PreviousItem/NextItem/DefaultItem opacity) are only observable when adjacent items are partially
/// shown via <see cref="CarouselView.PeekAreaInsets"/>, but peek is not supported under
/// <see cref="FullScreenCarouselStrategy"/> — every item is masked to the full viewport and snaps one
/// per page, so only the current item is ever on screen. Revisit if a non-full-screen strategy with
/// peek support is added.
/// </para>
///
/// <para>
/// <b>Looping is not supported on this handler.</b> Material's <see cref="CarouselLayoutManager"/>
/// has no concept of a virtual range, so the LoopScale (≈16384) trick used by MAUI's
/// LinearLayoutManager-based implementation does not work. Callers must keep
/// <see cref="CarouselView.Loop"/> set to <c>false</c>; <see cref="CarouselViewAdapter2.ItemCount"/>
/// is locked to <c>ItemsSource.Count</c> to guard the adapter side.
/// </para>
/// </summary>
public class MauiCarouselRecyclerView2 :
    Items.MauiCarouselRecyclerView
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

    protected override LayoutManager SelectLayoutManager(IItemsLayout layoutSpecification)
    {
        var orientation = RecyclerView.Horizontal;

        if (layoutSpecification is LinearItemsLayout linearItemsLayout)
        {
            orientation = linearItemsLayout.Orientation == ItemsLayoutOrientation.Vertical
                ? RecyclerView.Vertical
                : RecyclerView.Horizontal;
        }

        // While the EmptyView is showing, the RecyclerView holds a single non-carousel
        // item. Material's CarouselLayoutManager applies keyline masking sized for
        // full-viewport carousel items; applied to a normally-sized EmptyView the mask
        // collapses on a later layout pass, so the EmptyView appears for one frame and
        // then disappears. Use a plain LinearLayoutManager for the empty state — this
        // mirrors the LinearLayoutManager-based Handler1 path where the EmptyView renders
        // correctly. The empty-view branch in UpdateEmptyViewVisibility swaps in the
        // EmptyViewAdapter before calling SelectLayoutManager, so GetAdapter() reflects
        // the empty state here.
        if (GetAdapter() is Items.EmptyViewAdapter)
        {
            _carouselSnapHelper?.AttachToRecyclerView(null);
            _carouselSnapHelper = null;
            return new LinearLayoutManager(Context, orientation, false);
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
    protected override void UpdateSnapBehavior()
    {
        // Detach any previous snap helper to avoid duplicate fling listeners.
        _carouselSnapHelper?.AttachToRecyclerView(null);
        _carouselSnapHelper = null;

        // CarouselSnapHelper requires a CarouselLayoutManager. While the EmptyView is
        // showing we use a LinearLayoutManager, so don't attach the snap helper.
        if (GetLayoutManager() is not CarouselLayoutManager)
        {
            return;
        }

        // CarouselLayoutManager ships its own snap helper; attach it directly.
        // Deliberately do NOT call base.UpdateSnapBehavior() so MAUI's SnapManager
        // does not attach a conflicting snap helper.
        _carouselSnapHelper = new CarouselSnapHelper();
        _carouselSnapHelper.AttachToRecyclerView(this);
    }

    public override void UpdateFlowDirection()
    {
        // CarouselLayoutManager has no anchor/saved-state mechanism, so it always
        // re-lays out from item 0 when LayoutDirection changes (unlike LinearLayoutManager).
        // Save the position first, then set a pending scroll on the layout manager
        // synchronously — before the layout pass runs — so onLayoutChildren() starts at
        // the correct item instead of 0, with no intermediate frame visible.
        var positionToRestore = Carousel?.Position ?? -1;

        base.UpdateFlowDirection();

        if (positionToRestore > 0)
        {
            var itemCount = ItemsViewAdapter?.ItemsSource?.Count ?? 0;
            if (positionToRestore < itemCount)
            {
                // Qualified call to avoid resolving to the Microsoft.Maui.Controls.ScrollToPosition enum.
                ScrollHelper.JumpScrollToPosition(positionToRestore, Microsoft.Maui.Controls.ScrollToPosition.Center);
            }
        }
    }

    public override void UpdateLayoutManager()
    {
        base.UpdateLayoutManager();

        // base.UpdateLayoutManager() early-returns when the ItemsLayout object is
        // unchanged, which is the case on every EmptyView <-> items transition. That
        // leaves the layout manager from the previous state attached. Ensure items always
        // use the Material CarouselLayoutManager and the EmptyView uses a plain
        // LinearLayoutManager.
        var needsCarousel = GetAdapter() is not Items.EmptyViewAdapter;
        var hasCarousel = GetLayoutManager() is CarouselLayoutManager;
        if (needsCarousel != hasCarousel)
        {
            SetLayoutManager(SelectLayoutManager(ItemsLayout));
        }

        // Re-attach the CarouselSnapHelper to the current Material layout manager. The
        // helper is skipped while a LinearLayoutManager is active for the EmptyView.
        UpdateSnapBehavior();
    }

    protected override void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
    {
        // Skip the MAUI snap-manager reset (no SingleSnapHelper attached) and go straight
        // to the underlying scroll so CarouselSnapHelper continues to control snapping.
        ScrollTo(args);
    }

    // LinearItemsLayout.ItemSpacing is intentionally not honoured by this handler.
    //
    // The base class applies a CarouselSpacingItemDecoration, but that is incompatible with
    // Material's CarouselLayoutManager for two reasons:
    //   1. CarouselLayoutManager builds its entire KeylineState from the *decorated* measured
    //      size of the first child (onFirstChildMeasuredWithMargins). RecyclerView folds item-
    //      decoration offsets into that decorated size, so injecting spacing offsets would
    //      distort the measurement the carousel uses to lay out every item.
    //   2. FullScreenCarouselStrategy masks each item to fill the viewport and snaps one item
    //      per page, so there is no inter-item gutter for ItemSpacing to occupy. Inter-item
    //      spacing is a property of the partially-visible-neighbour strategies (MultiBrowse,
    //      Hero, Uncontained) that this handler is deliberately locked out of (see
    //      CreateCarouselStrategy) — the same class of limitation as Loop and PeekAreaInsets.
    //
    // A no-op decoration is used (rather than no decoration) so the base class's decoration
    // wiring still has an instance to add/remove.
    // TODO: Provide a Material3-compatible spacing implementation once a peek-capable,
    // strategy-aware layout path exists for this handler.
    protected override RecyclerView.ItemDecoration CreateSpacingDecoration(IItemsLayout itemsLayout)
        => new NoOpItemDecoration();

    sealed class NoOpItemDecoration : RecyclerView.ItemDecoration { }

    protected override Items.RecyclerViewScrollListener<CarouselView, Items.IItemsViewSource> CreateScrollListener()
        => new CarouselViewOnScrollListener2(Carousel, ItemsViewAdapter, () => _carouselSnapHelper);

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
}
