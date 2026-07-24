#nullable disable
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers.Items2;

/// <summary>
/// Android-specific partial for <see cref="CarouselViewHandler2"/>.
/// Uses <see cref="MauiCarouselRecyclerView2"/> as the platform view, which replaces
/// <see cref="AndroidX.RecyclerView.Widget.LinearLayoutManager"/> with the Material Design
/// <see cref="Google.Android.Material.Carousel.CarouselLayoutManager"/>.
///
/// <para>
/// <b>Looping is not supported by this handler.</b> Callers must keep
/// <see cref="CarouselView.Loop"/> set to <c>false</c> — see
/// <see cref="MauiCarouselRecyclerView2"/> for details.
/// </para>
/// </summary>
public partial class CarouselViewHandler2 : Items.ItemsViewHandler<CarouselView>
{
    double _widthConstraint;
    double _heightConstraint;

    protected override IItemsLayout GetItemsLayout() => VirtualView.ItemsLayout;

    protected override Items.ItemsViewAdapter<CarouselView, Items.IItemsViewSource> CreateAdapter()
    {
        // CarouselViewAdapter2 wraps each item in MaskableFrameLayout, which is required
        // by CarouselLayoutManager. Items must be sized through SizedItemContentView
        // (driven by GetItemWidth/GetItemHeight) so the Material carousel strategy gets
        // a non-zero measured first child to build its KeylineState from.
        return new CarouselViewAdapter2(
            VirtualView,
            context => new Items.SizedItemContentView(context, GetItemWidth, GetItemHeight),
            IsHorizontal);
    }

    bool IsHorizontal() =>
        VirtualView?.ItemsLayout is LinearItemsLayout { Orientation: ItemsLayoutOrientation.Horizontal } == true;

    protected override RecyclerView CreatePlatformView()
    {
        var carouselView = new MauiCarouselRecyclerView2(Context, GetItemsLayout, CreateAdapter);
        carouselView.SetClipChildren(false);
        carouselView.SetClipToPadding(false);
        return carouselView;
    }

    public static PropertyMapper<CarouselView, CarouselViewHandler2> Mapper =
        new(Items.ItemsViewHandler<CarouselView>.ItemsViewMapper)
        {
            [Controls.CarouselView.ItemsLayoutProperty.PropertyName] = MapItemsLayout,
            [Controls.CarouselView.IsSwipeEnabledProperty.PropertyName] = MapIsSwipeEnabled,
            [Controls.CarouselView.PeekAreaInsetsProperty.PropertyName] = MapPeekAreaInsets,
            [Controls.CarouselView.IsBounceEnabledProperty.PropertyName] = MapIsBounceEnabled,
            [Controls.CarouselView.PositionProperty.PropertyName] = MapPosition,
            [Controls.CarouselView.CurrentItemProperty.PropertyName] = MapCurrentItem,
        };

    public CarouselViewHandler2() : base(Mapper) { }

    public CarouselViewHandler2(PropertyMapper mapper = null) : base(mapper ?? Mapper) { }

    public static void MapIsSwipeEnabled(CarouselViewHandler2 handler, CarouselView carouselView)
    {
        if (handler.PlatformView is Items.IMauiCarouselRecyclerView carousel)
        {
            carousel.IsSwipeEnabled = carouselView.IsSwipeEnabled;
        }
    }

    public static void MapIsBounceEnabled(CarouselViewHandler2 handler, CarouselView carouselView)
    {
        handler.PlatformView.OverScrollMode =
            carouselView?.IsBounceEnabled == true ? OverScrollMode.Always : OverScrollMode.Never;
    }

    public static void MapPeekAreaInsets(CarouselViewHandler2 handler, CarouselView carouselView)
    {
        // PeekAreaInsets is intentionally a no-op for this handler.
        //
        // PeekAreaInsets is meant to shrink each item below the viewport so a sliver of the
        // adjacent item(s) "peeks" in at the edges. That requires a CarouselStrategy that lays
        // out partially-visible neighbours (MultiBrowse, Hero, Uncontained). This handler is
        // locked to FullScreenCarouselStrategy (see MauiCarouselRecyclerView2.CreateCarouselStrategy),
        // which builds its KeylineState from the full container size and masks every child to fill
        // the viewport — one full-screen item per page with no peeking neighbours. The child's
        // measured size does not influence the keyline, so re-measuring items here would only
        // rebuild the adapter (and reset the carousel position) without producing any peek effect.
        //
        // TODO: Wire this up once a peek-capable, strategy-aware sizing path exists for the
        // Material3 handler.
    }

    public static void MapPosition(CarouselViewHandler2 handler, CarouselView carouselView)
    {
        if (carouselView.Position < 0)
        {
            return;
        }

        if (handler.PlatformView is Items.IMauiCarouselRecyclerView carousel)
        {
            carousel.UpdateFromPosition();
        }
    }

    public static void MapCurrentItem(CarouselViewHandler2 handler, CarouselView carouselView)
    {
        if (handler.PlatformView is Items.IMauiCarouselRecyclerView carousel)
        {
            carousel.UpdateFromCurrentItem();
        }
    }

    internal static void MapItemsLayout(CarouselViewHandler2 handler, CarouselView carouselView)
    {
        if (handler.PlatformView is Items.IMauiRecyclerView<CarouselView> recyclerView)
        {
            recyclerView.UpdateLayoutManager();
        }
    }

    public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
    {
        _widthConstraint = widthConstraint;
        _heightConstraint = heightConstraint;

        if (Context is not null)
        {
            if (!double.IsInfinity(_widthConstraint))
            {
                _widthConstraint = Context.ToPixels(_widthConstraint);
            }

            if (!double.IsInfinity(_heightConstraint))
            {
                _heightConstraint = Context.ToPixels(_heightConstraint);
            }
        }

        return base.GetDesiredSize(widthConstraint, heightConstraint);
    }

    public override void PlatformArrange(Rect frame)
    {
        if (Context is not null)
        {
            _widthConstraint = Context.ToPixels(frame.Width);
            _heightConstraint = Context.ToPixels(frame.Height);
        }

        base.PlatformArrange(frame);
    }

    double GetItemWidth()
    {
        var itemWidth = _widthConstraint;

        if (PlatformView is Items.IMauiRecyclerView<CarouselView> { ItemsLayout: LinearItemsLayout { Orientation: ItemsLayoutOrientation.Horizontal } })
        {
            var width = PlatformView.MeasuredWidth == 0 ? _widthConstraint : PlatformView.MeasuredWidth;
            if (double.IsInfinity(width))
            {
                return width;
            }

            // PeekAreaInsets is intentionally not honoured here: this handler is locked to
            // FullScreenCarouselStrategy, which masks every item to fill the viewport (see
            // MapPeekAreaInsets). Subtracting the insets would only shrink the inner content
            // inside a still-full-width slot, never producing a peek. Use the full width.
            itemWidth = (int)width;
        }

        return itemWidth;
    }

    double GetItemHeight()
    {
        var itemHeight = _heightConstraint;

        if (PlatformView is Items.IMauiRecyclerView<CarouselView> { ItemsLayout: LinearItemsLayout { Orientation: ItemsLayoutOrientation.Vertical } })
        {
            var height = PlatformView.MeasuredHeight == 0 ? _heightConstraint : PlatformView.MeasuredHeight;
            if (double.IsInfinity(height))
            {
                return height;
            }

            // PeekAreaInsets is intentionally not honoured here: this handler is locked to
            // FullScreenCarouselStrategy, which masks every item to fill the viewport (see
            // MapPeekAreaInsets). Subtracting the insets would only shrink the inner content
            // inside a still-full-height slot, never producing a peek. Use the full height.
            itemHeight = (int)height;
        }

        return itemHeight;
    }
}
