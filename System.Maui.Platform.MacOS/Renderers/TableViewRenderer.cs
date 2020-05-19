using System.Collections.Generic;
using System.ComponentModel;
using AppKit;

namespace System.Maui.Platform.MacOS
{
	public class TableViewRenderer : ViewRenderer<TableView, NSView>
	{
		const int DefaultRowHeight = 44;
		bool _disposed;

		internal NSTableView TableView { get; set; }

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return Control.GetSizeRequest(widthConstraint, heightConstraint, DefaultRowHeight, DefaultRowHeight);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				var viewsToLookAt = new Stack<NSView>(Subviews);
				while (viewsToLookAt.Count > 0)
				{
					var view = viewsToLookAt.Pop();
					var viewCellRenderer = view as ViewCellNSView;
					if (viewCellRenderer != null)
					{
						viewCellRenderer.RemoveFromSuperview();
						viewCellRenderer.Dispose();
					}
					else
					{
						foreach (var child in view.Subviews)
							viewsToLookAt.Push(child);
					}
				}
			}
			_disposed = true;

			base.Dispose(disposing);
		}

		protected virtual NSTableView CreateNSTableView(TableView list)
		{
			return new NSTableView().AsListViewLook();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TableView> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var scroller = new NSScrollView
					{
						AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable,
						DocumentView = TableView = CreateNSTableView(e.NewElement)
					};

					SetNativeControl(scroller);
				}

				SetSource();
				UpdateRowHeight();
				UpdateBackgroundView();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == System.Maui.TableView.RowHeightProperty.PropertyName)
				UpdateRowHeight();
			else if (e.PropertyName == System.Maui.TableView.HasUnevenRowsProperty.PropertyName)
				SetSource();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundView();
		}

		void SetSource()
		{
			var modeledView = Element;
			TableView.Source = modeledView.HasUnevenRows ? new UnEvenTableViewModelRenderer(this) : new TableViewDataSource(this);
		}

		void UpdateBackgroundView()
		{
		}

		void UpdateRowHeight()
		{
			var rowHeight = Element.RowHeight;
			TableView.RowHeight = rowHeight <= 0 ? DefaultRowHeight : rowHeight;
		}
	}
}