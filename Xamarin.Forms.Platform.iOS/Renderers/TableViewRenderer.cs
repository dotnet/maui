using System.Collections.Generic;
using System.ComponentModel;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Xamarin.Forms.Platform.iOS
{
	public class TableViewRenderer : ViewRenderer<TableView, UITableView>
	{
		const int DefaultRowHeight = 44;
		KeyboardInsetTracker _insetTracker;
		UIView _originalBackgroundView;
		RectangleF _previousFrame;

		[Internals.Preserve(Conditional = true)]
		public TableViewRenderer()
		{

		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return Control.GetSizeRequest(widthConstraint, heightConstraint, DefaultRowHeight, DefaultRowHeight);
		}

		public override void LayoutSubviews()
		{
			_insetTracker?.OnLayoutSubviews();
			base.LayoutSubviews();

			if (_previousFrame != Frame)
			{
				_previousFrame = Frame;
				_insetTracker?.UpdateInsets();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _insetTracker != null)
			{
				_insetTracker.Dispose();
				_insetTracker = null;

				var viewsToLookAt = new Stack<UIView>(Subviews);
				while (viewsToLookAt.Count > 0)
				{
					var view = viewsToLookAt.Pop();
					var viewCellRenderer = view as ViewCellRenderer.ViewTableCell;
					if (viewCellRenderer != null)
						viewCellRenderer.Dispose();
					else
					{
						foreach (var child in view.Subviews)
							viewsToLookAt.Push(child);
					}
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TableView> e)
		{
			if (e.NewElement != null)
			{
				var style = UITableViewStyle.Plain;
				if (e.NewElement.Intent != TableIntent.Data)
					style = UITableViewStyle.Grouped;

				if (Control == null || Control.Style != style)
				{
					if (Control != null)
					{
						_insetTracker.Dispose();
						Control.Dispose();
					}

					var tv = new UITableView(RectangleF.Empty, style);
					_originalBackgroundView = tv.BackgroundView;

					SetNativeControl(tv);
					if (Forms.IsiOS9OrNewer)
						tv.CellLayoutMarginsFollowReadableWidth = false;

					_insetTracker = new KeyboardInsetTracker(tv, () => Control.Window, insets => Control.ContentInset = Control.ScrollIndicatorInsets = insets, point =>
					{
						var offset = Control.ContentOffset;
						offset.Y += point.Y;
						Control.SetContentOffset(offset, true);
					}, this);
				}

				SetSource();
				UpdateRowHeight();
				UpdateEstimatedRowHeight();
				UpdateBackgroundView();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == TableView.RowHeightProperty.PropertyName)
				UpdateRowHeight();
			else if (e.PropertyName == TableView.HasUnevenRowsProperty.PropertyName)
				SetSource();
			else if (e.PropertyName == TableView.BackgroundColorProperty.PropertyName)
				UpdateBackgroundView();
		}

		protected override void UpdateNativeWidget()
		{
			if (Element.Opacity < 1)
			{
				if (!Control.Layer.ShouldRasterize)
				{
					Control.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
					Control.Layer.ShouldRasterize = true;
				}
			}
			else
				Control.Layer.ShouldRasterize = false;
			base.UpdateNativeWidget();
		}

		void SetSource()
		{
			var modeledView = Element;
			Control.Source = modeledView.HasUnevenRows ? new UnEvenTableViewModelRenderer(modeledView) : new TableViewModelRenderer(modeledView);
		}

		void UpdateBackgroundView()
		{
			Control.BackgroundView = Element.BackgroundColor == Color.Default ? _originalBackgroundView : null;
		}

		void UpdateRowHeight()
		{
			var rowHeight = Element.RowHeight;
			if (Element.HasUnevenRows && rowHeight == -1) {
				Control.RowHeight = UITableView.AutomaticDimension;
			} else
				Control.RowHeight = rowHeight <= 0 ? DefaultRowHeight : rowHeight;
		}

		void UpdateEstimatedRowHeight()
		{
			var rowHeight = Element.RowHeight;
			if (Element.HasUnevenRows && rowHeight == -1) {
				Control.EstimatedRowHeight = DefaultRowHeight;
			} else {
				Control.EstimatedRowHeight = 0;
			}
		}
	}
}
