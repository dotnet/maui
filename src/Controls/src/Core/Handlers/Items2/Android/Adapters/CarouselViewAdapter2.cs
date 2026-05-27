#nullable disable
using System;
using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Carousel;
using Google.Android.Material.Shape;

namespace Microsoft.Maui.Controls.Handlers.Items2;

/// <summary>
/// An adapter for <see cref="MauiCarouselRecyclerView2"/> that wraps each item view in a
/// <see cref="MaskableFrameLayout"/>, satisfying the Material <see cref="CarouselLayoutManager"/>
/// requirement that every direct RecyclerView child must be a <see cref="MaskableFrameLayout"/>.
/// </summary>
internal sealed class CarouselViewAdapter2
    : Items.CarouselViewAdapter<CarouselView, Items.IItemsViewSource>
{
    readonly Func<Context, Items.ItemContentView> _createItemContentView;
    readonly Func<bool> _isHorizontal;

    internal CarouselViewAdapter2(
        CarouselView carouselView,
        Func<Context, Items.ItemContentView> createItemContentView,
        Func<bool> isHorizontal)
        : base(carouselView)
    {
        _createItemContentView = createItemContentView;
        _isHorizontal = isHorizontal;
    }

    /// <summary>
    /// Override the base <see cref="Items.CarouselViewAdapter{TItemsView,TItemsViewSource}.ItemCount"/>
    /// to ignore <see cref="CarouselView.Loop"/>.
    ///
    /// The base adapter returns <c>CarouselViewLoopManager.LoopScale</c> (≈16384) when
    /// <c>Loop=true</c>, which works with <c>LinearLayoutManager</c> + MAUI's
    /// <c>SnapManager</c>. Material's <see cref="CarouselLayoutManager"/> was not designed
    /// for that scale: every measure pass that triggers <c>MeasureInvalidated</c> →
    /// <c>RequestLayout</c> re-enters layout from a different anchor in the 16384-item
    /// virtual range, never converges, and inflates view holders without ever recycling.
    /// That produces the "stuck on splash" / endless GC symptom on Android.
    ///
    /// CarouselLayoutManager has no native looping support, so for <c>Handler2</c> we
    /// expose the real item count. Callers should keep <see cref="CarouselView.Loop"/>
    /// set to <c>false</c>.
    /// </summary>
    public override int ItemCount => ItemsSource?.Count ?? 0;

    // -----------------------------------------------------------------------
    // ViewHolder creation — wrap ItemContentView in MaskableFrameLayout
    // -----------------------------------------------------------------------

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        var context = parent.Context;

        if (viewType == Items.ItemViewType.TextItem)
        {
            // Text items don't need MaskableFrameLayout; delegate to base.
            return base.OnCreateViewHolder(parent, viewType);
        }

        var itemContentView = _createItemContentView(context);
        itemContentView.LayoutParameters = new ViewGroup.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.MatchParent);

        // CarouselLayoutManager's strategy reads the *measured* width (horizontal) or
        // height (vertical) of the first child to build its KeylineState. Use
        // WRAP_CONTENT on the carousel axis so SizedItemContentView can push the
        // desired pixel size up through measurement. A fixed pixel value sampled
        // here from GetItemWidth/Height would be 0 before the RecyclerView is laid
        // out — Math.Max(1, 0) then yields 1px items, which combined with looping
        // (LoopScale ≈ 16384 items) causes an infinite measure / GC loop and a
        // stuck UI.
        bool horizontal = _isHorizontal?.Invoke() ?? true;
        var maskable = new MaskableFrameLayout(context)
        {
            LayoutParameters = new RecyclerView.LayoutParams(
                horizontal ? ViewGroup.LayoutParams.WrapContent : ViewGroup.LayoutParams.MatchParent,
                horizontal ? ViewGroup.LayoutParams.MatchParent : ViewGroup.LayoutParams.WrapContent),
        };

        // Resolve the Material 3 "Corner Extra Large" shape appearance from the current theme
        // and apply it so the carousel item gets the expected rounded-corner mask.
        using (var value = new global::Android.Util.TypedValue())
        {
            if (context.Theme.ResolveAttribute(Resource.Attribute.shapeAppearanceCornerExtraLarge, value, true)
                && value.ResourceId != 0)
            {
                maskable.ShapeAppearanceModel = ShapeAppearanceModel
                    .InvokeBuilder(context, value.ResourceId, 0)
                    .Build();
            }
        }

        maskable.AddView(itemContentView);

        // MaskableCarouselItemViewHolder.Bind resolves the correct DataTemplate per item
        // via SelectDataTemplate, which handles both plain templates and DataTemplateSelector,
        // so we pass the root ItemTemplate directly.
        return new MaskableCarouselItemViewHolder(maskable, itemContentView, ItemsView.ItemTemplate);
    }

    // -----------------------------------------------------------------------
    // Bind / recycle
    // -----------------------------------------------------------------------

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
    {
        if (holder is MaskableCarouselItemViewHolder maskableHolder)
        {
            if (CarouselView is null || ItemsSource is null || position < 0 || position >= ItemsSource.Count)
            {
                return;
            }

            var item = ItemsSource.GetItem(position);
            maskableHolder.Bind(item, CarouselView);
            return;
        }

        base.OnBindViewHolder(holder, position);
    }

    public override void OnViewRecycled(Java.Lang.Object holder)
    {
        if (holder is MaskableCarouselItemViewHolder maskableHolder)
        {
            maskableHolder.Recycle(CarouselView);
            return;
        }

        base.OnViewRecycled(holder);
    }
}
