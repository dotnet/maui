using System;
using Android.Content;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.ControlGallery.Android;
using Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.AlternateLayoutGalleries;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Platform;
using ARect = Android.Graphics.Rect;
using AView = Android.Views.View;

[assembly: ExportRenderer(typeof(StaggeredCollectionView), typeof(StaggeredCollectionViewRenderer))]
namespace Microsoft.Maui.Controls.ControlGallery.Android
{
	public class StaggeredCollectionViewRenderer : CollectionViewRenderer
	{
		public StaggeredCollectionViewRenderer(Context context) : base(context) { }

		protected override LayoutManager SelectLayoutManager(IItemsLayout layoutSpecification)
		{
			if (layoutSpecification is StaggeredGridItemsLayout staggeredGridLayout)
			{
				var manager = new StaggeredGridLayoutManager(staggeredGridLayout.Span,
					staggeredGridLayout.Orientation == ItemsLayoutOrientation.Horizontal
					? LinearLayoutManager.Horizontal
					: LinearLayoutManager.Vertical);

				manager.GapStrategy = StaggeredGridLayoutManager.GapHandlingNone;

				return manager;
			}

			return base.SelectLayoutManager(layoutSpecification);
		}

		protected override ItemDecoration CreateSpacingDecoration(IItemsLayout itemsLayout)
		{
			return new SpacingItemDecoration(itemsLayout as StaggeredGridItemsLayout);
		}
	}

	public class SpacingItemDecoration : RecyclerView.ItemDecoration
	{
		ItemsLayoutOrientation _orientation;
		double _verticalSpacing;
		double _adjustedVerticalSpacing = -1;
		double _horizontalSpacing;
		double _adjustedHorizontalSpacing = -1;
		int _span = 1;

		public SpacingItemDecoration(StaggeredGridItemsLayout itemsLayout)
		{
			if (itemsLayout == null)
			{
				throw new ArgumentNullException(nameof(itemsLayout));
			}

			switch (itemsLayout)
			{
				case StaggeredGridItemsLayout gridItemsLayout:
					_orientation = gridItemsLayout.Orientation;
					_horizontalSpacing = gridItemsLayout.HorizontalItemSpacing;
					_verticalSpacing = gridItemsLayout.VerticalItemSpacing;
					_span = gridItemsLayout.Span;
					break;
			}
		}

		public override void GetItemOffsets(ARect outRect, AView view, RecyclerView parent, RecyclerView.State state)
		{
			base.GetItemOffsets(outRect, view, parent, state);

			var position = parent.GetChildAdapterPosition(view);

			if (_adjustedVerticalSpacing == -1)
			{
				_adjustedVerticalSpacing = parent.Context.ToPixels(_verticalSpacing);
			}

			if (_adjustedHorizontalSpacing == -1)
			{
				_adjustedHorizontalSpacing = parent.Context.ToPixels(_horizontalSpacing);
			}

			var spanIndex = 0;

			var layoutParameters = view.LayoutParameters as StaggeredGridLayoutManager.LayoutParams;

			if (layoutParameters != null)
			{
				spanIndex = layoutParameters.SpanIndex;
			}

			if (_orientation == ItemsLayoutOrientation.Vertical)
			{
				outRect.Left = spanIndex == 0 ? 0 : (int)_adjustedHorizontalSpacing;
				outRect.Top = position < _span ? 0 : (int)_adjustedVerticalSpacing;
			}
			else
			{
				outRect.Left = position < _span ? 0 : (int)_adjustedHorizontalSpacing;
				outRect.Top = spanIndex == 0 ? 0 : (int)_adjustedVerticalSpacing;
			}
		}
	}
}