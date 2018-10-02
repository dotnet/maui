using System;
using System.ComponentModel;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
	public class TableViewRenderer : ViewRenderer<TableView, Controls.TableView>
	{
		private const int DefaultRowHeight = 44;

		private bool _disposed;
		private Controls.TableView _tableView;

		protected override void UpdateBackgroundColor()
		{
			base.UpdateBackgroundColor();

			UpdateBackgroundView();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && !_disposed)
			{
				_disposed = true;

				if (_tableView != null)
				{
					_tableView.OnItemTapped -= OnItemTapped;
				}
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TableView> e)
		{
			if (e.OldElement != null)
			{
				e.OldElement.ModelChanged -= OnModelChanged;
			}

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					// Custom control very similar to ListView.
					_tableView = new Controls.TableView();
					_tableView.OnItemTapped += OnItemTapped;

					SetNativeControl(_tableView);
				}

				SetSource();
				UpdateRowHeight();
				UpdateHasUnevenRows();
				UpdateBackgroundView();

				e.NewElement.ModelChanged += OnModelChanged;
				OnModelChanged(e.NewElement, EventArgs.Empty);
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == TableView.RowHeightProperty.PropertyName)
				UpdateRowHeight();
			else if (e.PropertyName == TableView.HasUnevenRowsProperty.PropertyName)
			{
				UpdateHasUnevenRows();
				SetSource();
			}
		}

		void SetSource()
		{
			Control.Root = Element.Root;
		}

		void UpdateRowHeight()
		{
			var hasUnevenRows = Element.HasUnevenRows;

			if (hasUnevenRows)
			{
				return;
			}

			var rowHeight = Element.RowHeight;

			Control.SetRowHeight(rowHeight > 0 ? rowHeight : DefaultRowHeight);
		}

		void UpdateHasUnevenRows()
		{
			var hasUnevenRows = Element.HasUnevenRows;

			if (hasUnevenRows)
			{
				Control.SetHasUnevenRows();
			}
			else
			{
				UpdateRowHeight();
			}
		}

		void UpdateBackgroundView()
		{
			if (Element.BackgroundColor.IsDefault)
			{
				return;
			}

			var backgroundColor = Element.BackgroundColor.ToGtkColor();
			Control.SetBackgroundColor(backgroundColor);
		}

		void OnItemTapped(object sender, Controls.ItemTappedEventArgs args)
		{
			if (Element == null)
				return;

			if (args.Item is Cell cell)
			{
				if (cell.IsEnabled)
				{
					Element.Model.RowSelected(cell);
				}
			}
		}

		void OnModelChanged(object sender, EventArgs e)
		{
			SetSource();
		}
	}
}