#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class TableViewRenderer : ViewRenderer<TableView, UITableView>
	{
		const int DefaultRowHeight = 44;
		UIView _originalBackgroundView;
		RectangleF _previousFrame;

		public TableViewRenderer()
		{
			AutoPackage = false;
		}

		protected override Size MinimumSize()
		{
			return new Size(44, 44);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (_previousFrame != Frame)
				_previousFrame = Frame;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
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

		protected override UITableView CreateNativeControl()
		{
			return new UITableView(RectangleF.Empty, GetTableViewStyle(Element?.Intent ?? TableIntent.Data));
		}

		protected UITableViewStyle GetTableViewStyle(TableIntent intent)
		{
			return intent == TableIntent.Data ? UITableViewStyle.Plain : UITableViewStyle.Grouped;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TableView> e)
		{
			if (e.NewElement != null)
			{
				var style = GetTableViewStyle(e.NewElement.Intent);

				if (Control == null || Control.Style != style)
				{
					Control?.Dispose();

					var tv = CreateNativeControl();
					_originalBackgroundView = tv.BackgroundView;

					SetNativeControl(tv);
					tv.CellLayoutMarginsFollowReadableWidth = false;
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
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName || e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
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

		public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
		{
#pragma warning disable CA1422 // Validate platform compatibility
			base.TraitCollectionDidChange(previousTraitCollection);
#pragma warning restore CA1422 // Validate platform compatibility
			// Make sure the cells adhere to changes UI theme
			if (OperatingSystem.IsIOSVersionAtLeast(13) && previousTraitCollection?.UserInterfaceStyle != TraitCollection.UserInterfaceStyle)
				Control.ReloadData();
		}

		void SetSource()
		{
			var modeledView = Element;
			Control.Source = modeledView.HasUnevenRows ? new UnEvenTableViewModelRenderer(modeledView) : new TableViewModelRenderer(modeledView);
		}

		void UpdateBackgroundView()
		{
			Control.BackgroundView = Element.BackgroundColor == null ? _originalBackgroundView : null;
			Control.BackgroundView.UpdateBackground(Element.Background);
		}

		void UpdateRowHeight()
		{
			var rowHeight = Element.RowHeight;
			if (Element.HasUnevenRows && rowHeight == -1)
			{
				Control.RowHeight = UITableView.AutomaticDimension;
			}
			else
				Control.RowHeight = rowHeight <= 0 ? DefaultRowHeight : rowHeight;
		}

		void UpdateEstimatedRowHeight()
		{
			var rowHeight = Element.RowHeight;
			if (Element.HasUnevenRows && rowHeight == -1)
			{
				Control.EstimatedRowHeight = DefaultRowHeight;
			}
			else
			{
				Control.EstimatedRowHeight = 0;
			}
		}
	}
}
