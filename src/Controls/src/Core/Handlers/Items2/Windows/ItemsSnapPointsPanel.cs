using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Controls.Handlers.Items2;

/// <summary>
/// A custom <see cref="StackPanel"/> that overrides the default <see cref="IScrollSnapPointsInfo"/>
/// behavior to provide snap points at individual item boundaries inside the <see cref="ItemsRepeater"/>
/// (PART_ItemsRepeater), rather than at the StackPanel's direct child boundaries
/// (Header, ItemsRepeater, EmptyView, Footer).
///
/// Without this, enabling <c>SnapPointsType.Mandatory</c> on the ScrollViewer causes snap points
/// to be at the 4 StackPanel children, creating large gaps that prevent gesture-based scrolling
/// from reaching intermediate item positions.
/// </summary>
internal class ItemsSnapPointsPanel : StackPanel, IScrollSnapPointsInfo
{
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
		add { }
		remove { }
	}

	event EventHandler<object>? IScrollSnapPointsInfo.VerticalSnapPointsChanged
	{
		add { }
		remove { }
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
		if (element.Visibility != Visibility.Visible)
			return;

		try
		{
			var transform = element.TransformToVisual(this);
			var point = transform.TransformPoint(new Windows.Foundation.Point(0, 0));

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
