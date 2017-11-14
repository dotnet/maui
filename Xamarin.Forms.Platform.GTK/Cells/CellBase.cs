using Gtk;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Xamarin.Forms.Platform.GTK.Cells
{
    public abstract class CellBase : EventBox
    {
        private Cell _cell;
        private int _desiredHeight;
        private IList<MenuItem> _contextActions;

        public Action<object, PropertyChangedEventArgs> PropertyChanged;

        protected CellBase()
        {
            ButtonReleaseEvent += OnClick;
        }

        public Cell Cell
        {
            get { return _cell; }
            set
            {
                if (_cell == value)
                    return;

                if (_cell != null)
                    Device.BeginInvokeOnMainThread(_cell.SendDisappearing);

                _cell = value;
                UpdateCell();
                _contextActions = Cell.ContextActions;

                if (_cell != null)
                    Device.BeginInvokeOnMainThread(_cell.SendAppearing);
            }
        }

        public object Item => Cell?.BindingContext;

        protected bool ParentHasUnevenRows
        {
            get
            {
                var table = Cell.RealParent as TableView;
                if (table != null)
                    return table.HasUnevenRows;

                var list = Cell.RealParent as ListView;
                if (list != null)
                    return list.HasUnevenRows;

                return false;
            }
        }

        public int DesiredHeight
        {
            get
            {
                return _desiredHeight;
            }

            set
            {
                _desiredHeight = value;
            }
        }

        public void SetDesiredHeight(int height)
        {
            DesiredHeight = height;

            if (IsRealized)
            {
                HeightRequest = DesiredHeight;
            }
        }

        public void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected override void OnRealized()
        {
            base.OnRealized();

            HeightRequest = DesiredHeight;
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            ButtonReleaseEvent -= OnClick;
        }

        protected virtual void UpdateCell()
        {
        }

        private void OnClick(object o, ButtonReleaseEventArgs args)
        {
            if (args.Event.Button != 3)  // Right button
            {
                return;
            }

            if (_contextActions.Any())
            {
                OpenContextMenu();
            }
        }

        private void OpenContextMenu()
        {
            var menu = new Gtk.Menu();

            SetupMenuItems(menu);
            menu.ShowAll();
            menu.Popup();
        }

        private void SetupMenuItems(Gtk.Menu menu)
        {
            foreach (MenuItem item in Cell.ContextActions)
            {
                var menuItem = new ImageMenuItem(item.Text);

                string icon = item.Icon;

                if (!string.IsNullOrEmpty(icon))
                {
                    menuItem.Image = new Gtk.Image(icon);
                }

                menuItem.ButtonPressEvent += (sender, args) =>
                {
                    item.Activate();
                };

                menu.Add(menuItem);
            }
        }
    }
}
