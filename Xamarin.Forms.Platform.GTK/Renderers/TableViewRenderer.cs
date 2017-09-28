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

        private void SetSource()
        {
            Control.Root = Element.Root;
        }

        private void UpdateRowHeight()
        {
            var hasUnevenRows = Element.HasUnevenRows;

            if (hasUnevenRows)
            {
                return;
            }

            var rowHeight = Element.RowHeight;

            Control.SetRowHeight(rowHeight > 0 ? rowHeight : DefaultRowHeight);
        }

        private void UpdateHasUnevenRows()
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

        private void UpdateBackgroundView()
        {
            if (Element.BackgroundColor.IsDefault)
            {
                return;
            }

            var backgroundColor = Element.BackgroundColor.ToGtkColor();
            Control.SetBackgroundColor(backgroundColor);
        }

        private void OnItemTapped(object sender, Controls.ItemTappedEventArgs args)
        {
            if (Element == null)
                return;

            var cell = args.Item as Cell;

            if (cell != null)
            {
                if (cell.IsEnabled)
                {
                    Element.Model.RowSelected(cell);
                }
            }
        }
    }
}