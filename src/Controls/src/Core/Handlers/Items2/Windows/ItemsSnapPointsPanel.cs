using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Handlers.Items2;

/// <summary>
/// A custom <see cref="Panel"/> with stack layout behavior that implements
/// <see cref="IScrollSnapPointsInfo"/> to provide snap points at individual item boundaries
/// inside the <see cref="ItemsRepeater"/> (PART_ItemsRepeater), rather than at the panel's
/// direct child boundaries (Header, ItemsRepeater, EmptyView, Footer).
///
/// We derive from <see cref="Panel"/> instead of <see cref="StackPanel"/> because
/// <see cref="StackPanel"/> already implements <see cref="IScrollSnapPointsInfo"/> via WinRT
/// projection, and C# cannot re-implement a WinRT-projected interface on a derived managed class
/// (CS0539). By using <see cref="Panel"/> as the base, we own the <see cref="IScrollSnapPointsInfo"/>
/// implementation and can report per-item snap points to the ScrollViewer compositor.
///
/// Without this, enabling <c>SnapPointsType.Mandatory</c> on the ScrollViewer causes snap points
/// to be at the 4 panel children, creating large gaps that prevent gesture-based scrolling
/// from reaching intermediate item positions.
/// </summary>
internal class ItemsSnapPointsPanel : Panel, IScrollSnapPointsInfo
{
    EventHandler<object>? _horizontalSnapPointsChanged;
    EventHandler<object>? _verticalSnapPointsChanged;
    int _lastSnapPointCount; // track realized item count to avoid redundant notifications
    /// <summary>
    /// Identifies the <see cref="Orientation"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(ItemsSnapPointsPanel),
            new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));

    /// <summary>
    /// Gets or sets the axis along which children are stacked.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((ItemsSnapPointsPanel)d).InvalidateMeasure();
    }

    protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
    {
        double stackExtent = 0;
        double crossMax = 0;
        bool isVertical = Orientation == Orientation.Vertical;

        // In the stacking direction the children get infinite space (ScrollViewer
        // already provides this, but being explicit avoids surprises).
        // In the cross direction they get the full available width/height.
        var childAvailable = isVertical
            ? new global::Windows.Foundation.Size(availableSize.Width, double.PositiveInfinity)
            : new global::Windows.Foundation.Size(double.PositiveInfinity, availableSize.Height);

        foreach (var child in Children)
        {
            child.Measure(childAvailable);
            var desired = child.DesiredSize;

            if (isVertical)
            {
                stackExtent += desired.Height;
                if (desired.Width > crossMax)
                    crossMax = desired.Width;
            }
            else
            {
                stackExtent += desired.Width;
                if (desired.Height > crossMax)
                    crossMax = desired.Height;
            }
        }

        // StackPanel clamps the cross-axis desired size to the available size, so that
        // it never reports a DesiredSize larger than what the parent offered. We replicate
        // this behavior to avoid layout differences (e.g., in a ScrollViewer, the
        // cross-axis is constrained to the viewport and must not overflow).
        if (isVertical)
        {
            double clampedWidth = double.IsPositiveInfinity(availableSize.Width)
                ? crossMax
                : Math.Min(crossMax, availableSize.Width);
            return new global::Windows.Foundation.Size(clampedWidth, stackExtent);
        }
        else
        {
            double clampedHeight = double.IsPositiveInfinity(availableSize.Height)
                ? crossMax
                : Math.Min(crossMax, availableSize.Height);
            return new global::Windows.Foundation.Size(stackExtent, clampedHeight);
        }
    }

    protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
    {
        double offset = 0;
        bool isVertical = Orientation == Orientation.Vertical;

        foreach (var child in Children)
        {
            var desired = child.DesiredSize;

            if (isVertical)
            {
                child.Arrange(new global::Windows.Foundation.Rect(0, offset, finalSize.Width, desired.Height));
                offset += desired.Height;
            }
            else
            {
                child.Arrange(new global::Windows.Foundation.Rect(offset, 0, desired.Width, finalSize.Height));
                offset += desired.Width;
            }
        }

        // After layout, notify the ScrollViewer that snap points may have changed.
        // The ScrollViewer caches snap point values and only re-queries when this
        // event fires. Without this, newly realized items (from virtualization or
        // collection changes) won't be included in snap point calculations, causing
        // the user to get stuck and unable to scroll to the next item.
        NotifySnapPointsChanged();

        return finalSize;
    }

    /// <summary>
    /// Counts currently realized item containers and raises the appropriate
    /// SnapPointsChanged event if the count has changed since the last notification.
    /// This avoids firing on every arrange pass when nothing meaningful changed.
    /// </summary>
    void NotifySnapPointsChanged()
    {
        int currentCount = 0;
        foreach (var child in Children)
        {
            if (child is ItemsRepeater repeater)
            {
                var itemCount = repeater.ItemsSourceView?.Count ?? 0;
                for (int i = 0; i < itemCount; i++)
                {
                    if (repeater.TryGetElement(i) is not null)
                        currentCount++;
                }
            }
            else
            {
                currentCount++;
            }
        }

        if (currentCount != _lastSnapPointCount)
        {
            _lastSnapPointCount = currentCount;

            if (Orientation == Orientation.Horizontal)
                _horizontalSnapPointsChanged?.Invoke(this, EventArgs.Empty);
            else
                _verticalSnapPointsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    #region IScrollSnapPointsInfo

    /// <summary>
    /// Gets whether horizontal snap points are regularly spaced.
    /// Returns false because item sizes may vary (headers, footers, different item templates).
    /// </summary>
    bool IScrollSnapPointsInfo.AreHorizontalSnapPointsRegular => false;

    /// <summary>
    /// Gets whether vertical snap points are regularly spaced.
    /// Returns false because item sizes may vary.
    /// </summary>
    bool IScrollSnapPointsInfo.AreVerticalSnapPointsRegular => false;

    event EventHandler<object>? IScrollSnapPointsInfo.HorizontalSnapPointsChanged
    {
        add => _horizontalSnapPointsChanged += value;
        remove => _horizontalSnapPointsChanged -= value;
    }

    event EventHandler<object>? IScrollSnapPointsInfo.VerticalSnapPointsChanged
    {
        add => _verticalSnapPointsChanged += value;
        remove => _verticalSnapPointsChanged -= value;
    }

    IReadOnlyList<float> IScrollSnapPointsInfo.GetIrregularSnapPoints(Orientation orientation, SnapPointsAlignment alignment)
    {
        var snapPoints = new List<float>();

        // Walk each direct child of this panel. For the ItemsRepeater, enumerate
        // its realized item containers so we get per-item snap points.
        // For other children (Header, Footer, EmptyView), add a single snap point.
        foreach (var child in Children)
        {
            if (child is not FrameworkElement fe)
                continue;

            if (child is ItemsRepeater repeater)
            {
                AddItemsRepeaterSnapPoints(repeater, orientation, alignment, snapPoints);
            }
            else
            {
                AddSnapPointForElement(fe, orientation, alignment, snapPoints);
            }
        }

        snapPoints.Sort();
        return snapPoints;
    }

    float IScrollSnapPointsInfo.GetRegularSnapPoints(Orientation orientation, SnapPointsAlignment alignment, out float offset)
    {
        // Not regular — return 0 interval and 0 offset.
        offset = 0;
        return 0;
    }

    #endregion

    void AddItemsRepeaterSnapPoints(ItemsRepeater repeater, Orientation orientation, SnapPointsAlignment alignment, List<float> snapPoints)
    {
        // The ItemsRepeater's realized children are the ItemContainer elements.
        // Walk them to get snap points at each item boundary.
        // Use ItemsSourceView.Count to know how many items exist, then
        // TryGetElement to get only the realized (visible + cached) containers.
        var itemCount = repeater.ItemsSourceView?.Count ?? 0;

        for (int i = 0; i < itemCount; i++)
        {
            var element = repeater.TryGetElement(i);
            if (element is FrameworkElement fe)
            {
                AddSnapPointForElement(fe, orientation, alignment, snapPoints);
            }
        }
    }

    void AddSnapPointForElement(FrameworkElement element, Orientation orientation, SnapPointsAlignment alignment, List<float> snapPoints)
    {
        if (element.Visibility != WVisibility.Visible)
            return;

        try
        {
            var transform = element.TransformToVisual(this);
            var point = transform.TransformPoint(new global::Windows.Foundation.Point(0, 0));

            float snapPoint;
            if (orientation == Orientation.Vertical)
            {
                snapPoint = alignment switch
                {
                    SnapPointsAlignment.Near => (float)point.Y,
                    SnapPointsAlignment.Center => (float)(point.Y + element.ActualHeight / 2),
                    SnapPointsAlignment.Far => (float)(point.Y + element.ActualHeight),
                    _ => (float)point.Y
                };
            }
            else
            {
                snapPoint = alignment switch
                {
                    SnapPointsAlignment.Near => (float)point.X,
                    SnapPointsAlignment.Center => (float)(point.X + element.ActualWidth / 2),
                    SnapPointsAlignment.Far => (float)(point.X + element.ActualWidth),
                    _ => (float)point.X
                };
            }

            snapPoints.Add(snapPoint);
        }
        catch
        {
            // TransformToVisual can throw if element is not in the visual tree
        }
    }
}
