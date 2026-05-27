#nullable disable
using System;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Carousel;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Items2;

/// <summary>
/// Scroll listener for <see cref="MauiCarouselRecyclerView2"/>.
/// Inherits directly from <see cref="Items.RecyclerViewScrollListener{TItemsView,TItemsViewSource}"/>
/// (not from <see cref="Items.CarouselViewOnScrollListener"/>) so it has no dependency on
/// <see cref="Items.CarouselViewLoopManager"/> and can use
/// <see cref="Google.Android.Material.Carousel.CarouselLayoutManager"/> for item positions.
/// </summary>
internal class CarouselViewOnScrollListener2 : Items.RecyclerViewScrollListener<CarouselView, Items.IItemsViewSource>
{
    readonly CarouselView _carouselView;
    readonly Func<CarouselSnapHelper> _snapHelperProvider;

    public CarouselViewOnScrollListener2(
        CarouselView carouselView,
        Items.ItemsViewAdapter<CarouselView, Items.IItemsViewSource> itemsViewAdapter,
        Func<CarouselSnapHelper> snapHelperProvider)
        : base(carouselView, itemsViewAdapter, true)
    {
        _carouselView = carouselView;
        _snapHelperProvider = snapHelperProvider;
    }

    public override void OnScrollStateChanged(RecyclerView recyclerView, int state)
    {
        base.OnScrollStateChanged(recyclerView, state);

        if (_carouselView.IsSwipeEnabled)
        {
            _carouselView.SetIsDragging(state == RecyclerView.ScrollStateDragging);
        }

        _carouselView.IsScrolling = state != RecyclerView.ScrollStateIdle;

    }

    protected override (int First, int Center, int Last) GetVisibleItemsIndex(RecyclerView recyclerView)
    {
        if (recyclerView.GetLayoutManager() is not CarouselLayoutManager carouselLayoutManager)
        {
            return (-1, -1, -1);
        }

        // CarouselLayoutManager doesn't extend LinearLayoutManager, so we walk children.
        var (first, last) = GetFirstAndLastVisiblePositions(carouselLayoutManager);

        if (first == RecyclerView.NoPosition)
        {
            return (-1, -1, -1);
        }

        // Prefer the CarouselSnapHelper's snap target as the "current" item; that's the
        // position the user lands on after a fling.
        int centerPosition = -1;
        var snapHelper = _snapHelperProvider?.Invoke();
        if (snapHelper is not null)
        {
            var snapView = snapHelper.FindSnapView(carouselLayoutManager);
            if (snapView is not null)
            {
                centerPosition = recyclerView.GetChildAdapterPosition(snapView);
            }
        }

        if (centerPosition == RecyclerView.NoPosition || centerPosition < 0)
        {
            centerPosition = (first + last) / 2;
        }

        return (first, centerPosition, last);
    }

    static (int First, int Last) GetFirstAndLastVisiblePositions(CarouselLayoutManager layoutManager)
    {
        int first = int.MaxValue;
        int last = int.MinValue;

        for (int i = 0; i < layoutManager.ChildCount; i++)
        {
            var child = layoutManager.GetChildAt(i);
            if (child is null)
            {
                continue;
            }

            int pos = layoutManager.GetPosition(child);
            if (pos < first)
            {
                first = pos;
            }

            if (pos > last)
            {
                last = pos;
            }
        }

        if (first == int.MaxValue)
        {
            return (RecyclerView.NoPosition, RecyclerView.NoPosition);
        }

        return (first, last);
    }
}
