using System;
using System.ComponentModel;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Cells
{
    public abstract class CellRenderer : IRegisterable
    {
        static readonly BindableProperty RealCellProperty =
            BindableProperty.CreateAttached("RealCell", typeof(Gtk.Container),
                typeof(Cell), null);

        protected Cell Cell { get; private set; }

        private EventHandler _onForceUpdateSizeRequested;

        public virtual CellBase GetCell(Cell item, Gtk.Container reusableView, Controls.ListView listView)
        {
            Cell = item;

            var cell = reusableView as Gtk.Container ?? GetCellWidgetInstance(item);

            var cellBase = cell as CellBase;

            if (cellBase != null)
            {
                if (cellBase.Cell != null)
                {
                    cellBase.Cell.PropertyChanged -= cellBase.HandlePropertyChanged;
                }

                cellBase.Cell = item;

                item.PropertyChanged += cellBase.HandlePropertyChanged;
                cellBase.PropertyChanged = CellPropertyChanged;
            }

            SetRealCell(item, cell);
            WireUpForceUpdateSizeRequested(item, cell);
            UpdateBackground(cell, item);
            UpdateIsEnabled(cellBase);
            UpdateHeight(cellBase);

            return cellBase;
        }

        protected virtual void CellPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var viewCell = sender as CellBase;

            if (args.PropertyName == Cell.IsEnabledProperty.PropertyName)
                UpdateIsEnabled(viewCell);
            else if (args.PropertyName == nameof(Cell.RenderHeight))
                UpdateHeight(viewCell);
        }

        protected virtual Gtk.Container GetCellWidgetInstance(Cell item)
        {
            return new Gtk.Container(IntPtr.Zero);
        }

        protected void UpdateBackground(Gtk.Container tableViewCell, Cell cell)
        {
            var bgColor = Color.White.ToGtkColor();
            var element = cell.RealParent as VisualElement;

            if (element != null)
                bgColor = element.BackgroundColor == Color.Default ? bgColor : element.BackgroundColor.ToGtkColor();

            UpdateBackgroundChild(cell, bgColor);

            tableViewCell.ModifyBg(Gtk.StateType.Normal, bgColor);
        }

        protected virtual void OnForceUpdateSizeRequest(Cell cell, Gtk.Container nativeCell)
        {
            nativeCell.HeightRequest = (int)cell.RenderHeight;
            nativeCell.QueueDraw();
        }

        protected void UpdateHeight(CellBase cell)
        {
            if (cell?.Cell != null)
            {
                cell.HeightRequest = (int)cell.Cell.RenderHeight;
            }
        }

        private void WireUpForceUpdateSizeRequested(Cell cell, Gtk.Container nativeCell)
        {
            cell.ForceUpdateSizeRequested -= _onForceUpdateSizeRequested;

            _onForceUpdateSizeRequested = (sender, e) =>
            {
                OnForceUpdateSizeRequest(cell, nativeCell);
            };

            cell.ForceUpdateSizeRequested += _onForceUpdateSizeRequested;
        }

        private static void UpdateIsEnabled(CellBase cell)
        {
            if (cell?.Cell != null)
            {
                cell.Sensitive = cell.Cell.IsEnabled;
            }
        }

        internal virtual void UpdateBackgroundChild(Cell cell, Gdk.Color backgroundColor)
        {
            // TODO
        }

        internal static Gtk.Container GetRealCell(BindableObject cell)
        {
            return (Gtk.Container)cell.GetValue(RealCellProperty);
        }

        internal static void SetRealCell(BindableObject cell, Gtk.Container renderer)
        {
            cell.SetValue(RealCellProperty, renderer);
        }
    }
}