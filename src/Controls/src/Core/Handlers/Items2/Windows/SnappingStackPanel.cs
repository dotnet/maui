using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using WOrientation = Microsoft.UI.Xaml.Controls.Orientation;

namespace Microsoft.Maui.Controls.Handlers.Items2;

/// <summary>
/// A <see cref="StackPanel"/> subclass that implements <see cref="IScrollSnapPointsInfo"/>
/// to provide snap point offsets for <see cref="ScrollViewer"/>.
/// <para>
/// WinUI's <see cref="UI.Xaml.Controls.ItemsView"/> uses <c>ScrollView</c> (modern) internally,
/// which handles snap points via <c>ScrollPresenter.SnapPoints</c>. Since we replaced
/// <c>PART_ScrollView</c> with <see cref="ScrollViewer"/> for <c>RefreshContainer</c> compatibility,
/// the <c>ScrollPresenter</c>-based snap point mechanism is unavailable. <see cref="ScrollViewer"/>
/// requires its content to implement <see cref="IScrollSnapPointsInfo"/> to provide snap offsets.
/// </para>
/// <para>
/// This panel wraps the <see cref="ItemsRepeater"/> (and header/footer) and computes snap point
/// offsets from each realized child of the repeater.
/// </para>
/// </summary>
internal class SnappingStackPanel : StackPanel, IScrollSnapPointsInfo
{
	ItemsRepeater? _itemsRepeater;
	SnapPointsType _snapPointsType = SnapPointsType.None;
	SnapPointsAlignment _snapPointsAlignment = SnapPointsAlignment.Near;
	bool _isHorizontal;

	/// <summary>
	/// Gets or sets the <see cref="ItemsRepeater"/> whose children define the snap points.
	/// </summary>
	internal ItemsRepeater? ItemsRepeater
	{
		get => _itemsRepeater;
		set
		{
			_itemsRepeater = value;
			NotifySnapPointsChanged();
		}
	}

	/// <summary>
	/// Updates the snap point configuration from the MAUI <see cref="Items.ItemsLayout"/>.
	/// </summary>
	internal void UpdateSnapPoints(SnapPointsType type, SnapPointsAlignment alignment, bool isHorizontal)
	{
		_snapPointsType = type;
		_snapPointsAlignment = alignment;
		_isHorizontal = isHorizontal;
		NotifySnapPointsChanged();
	}

	/// <summary>
	/// Notifies the <see cref="ScrollViewer"/> that snap point offsets have changed,
	/// causing it to re-query <see cref="GetIrregularSnapPoints"/>.
	/// </summary>
	internal void NotifySnapPointsChanged()
	{
		if (_isHorizontal)
		{
			HorizontalSnapPointsChanged?.Invoke(this, this);
		}
		else
		{
			VerticalSnapPointsChanged?.Invoke(this, this);
		}
	}

	#region IScrollSnapPointsInfo

	/// <summary>
	/// Snap points are irregular because items may have varying sizes,
	/// plus header/footer offsets shift the positions.
	/// </summary>
	public bool AreHorizontalSnapPointsRegular => false;

	/// <inheritdoc cref="AreHorizontalSnapPointsRegular"/>
	public bool AreVerticalSnapPointsRegular => false;

	public event EventHandler<object>? HorizontalSnapPointsChanged;
	public event EventHandler<object>? VerticalSnapPointsChanged;

	public IReadOnlyList<float> GetIrregularSnapPoints(
		WOrientation orientation,
		SnapPointsAlignment alignment)
	{
		var snapPoints = new List<float>();

		if (_itemsRepeater is null || _snapPointsType == SnapPointsType.None)
		{
			return snapPoints;
		}

		bool horizontal = orientation == WOrientation.Horizontal;

		// Calculate the offset from the header (the ItemsRepeater may not start at offset 0
		// because the header ContentControl precedes it in the StackPanel).
		double repeaterOffset = 0;
		if (_itemsRepeater.TransformToVisual(this) is { } transform)
		{
			var point = transform.TransformPoint(new global::Windows.Foundation.Point(0, 0));
			repeaterOffset = horizontal ? point.X : point.Y;
		}

		// Walk realized children of the ItemsRepeater to compute snap offsets.
		int childCount = _itemsRepeater.ActualItemCount;
		for (int i = 0; i < childCount; i++)
		{
			var element = _itemsRepeater.TryGetElement(i);
			if (element is null)
			{
				continue;
			}

			double itemOffset;
			double itemSize;

			if (horizontal)
			{
				itemOffset = repeaterOffset + element.ActualOffset.X;
				itemSize = element.ActualSize.X;
			}
			else
			{
				itemOffset = repeaterOffset + element.ActualOffset.Y;
				itemSize = element.ActualSize.Y;
			}

			double snapOffset = alignment switch
			{
				SnapPointsAlignment.Near => itemOffset,
				SnapPointsAlignment.Center => itemOffset + (itemSize / 2.0),
				SnapPointsAlignment.Far => itemOffset + itemSize,
				_ => itemOffset,
			};

			snapPoints.Add((float)snapOffset);
		}

		return snapPoints;
	}

	public float GetRegularSnapPoints(
		WOrientation orientation,
		SnapPointsAlignment alignment,
		out float offset)
	{
		// We always use irregular snap points since items can vary in size.
		offset = 0;
		return 0;
	}

	#endregion
}
