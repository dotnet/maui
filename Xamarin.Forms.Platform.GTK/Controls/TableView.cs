using Gtk;
using System;
using System.Collections.Generic;
using Xamarin.Forms.Platform.GTK.Cells;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    public class TableView : ScrolledWindow
    {
        private VBox _root;
        private TableRoot _source;
        private List<Container> _cells;

        public delegate void ItemTappedEventHandler(object sender, ItemTappedEventArgs args);
        public event ItemTappedEventHandler OnItemTapped = null;

        public TableView()
        {
            BuildTableView();
        }

        public TableRoot Root
        {
            get
            {
                return _source;
            }
            set
            {
                if (_source != value)
                {
                    _source = value;
                    RefreshSource(_source);
                }
            }
        }

        public void SetBackgroundColor(Gdk.Color backgroundColor)
        {
            Child?.ModifyBg(StateType.Normal, backgroundColor);
        }

        public void SetRowHeight(int rowHeight)
        {
            foreach (var cell in _cells)
            {
                cell.HeightRequest = rowHeight;
            }
        }

        public void SetHasUnevenRows()
        {
            foreach (var cell in _cells)
            {
                var rowHeight = GetUnevenRowCellHeight(cell);

                cell.HeightRequest = rowHeight;
            }
        }

        private int GetUnevenRowCellHeight(Gtk.Container cell)
        {
            int height = -1;

            var formsCell = GetXamarinFormsCell(cell);

            if (formsCell != null)
            {
                height = Convert.ToInt32(formsCell.Height);
            }

            return height;
        }

        private Cell GetXamarinFormsCell(Container cell)
        {
            try
            {
                var formsCell = cell
                   .GetType()
                   .GetProperty("Cell")
                   .GetValue(cell, null) as Cell;

                return formsCell;
            }
            catch
            {
                return null;
            }
        }

        private void BuildTableView()
        {
            CanFocus = true;
            ShadowType = ShadowType.None;
            HscrollbarPolicy = PolicyType.Never;
            VscrollbarPolicy = PolicyType.Automatic;
            BorderWidth = 0;

            _root = new VBox(false, 0);

            Viewport viewPort = new Viewport();
            viewPort.ShadowType = ShadowType.None;
            viewPort.Add(_root);

            Add(viewPort);

            _cells = new List<Container>();
        }

        private void RefreshSource(TableRoot source)
        {
            if (!string.IsNullOrEmpty(source.Title))
            {
                // Add Title
                var titleSpan = new Span()
                {
                    FontSize = 16,
                    Text = source.Title ?? string.Empty
                };

                Gtk.Label title = new Gtk.Label();
                title.SetAlignment(0, 0);
                title.SetTextFromSpan(titleSpan);
                _root.PackStart(title, false, false, 0);
            }

            // Add Table Section
            for (int i = 0; i < source.Count; i++)
            {
                var tableSection = source[i] as TableSection;

                if (tableSection != null)
                {
                    var tableSectionSpan = new Span()
                    {
                        FontSize = 12,
                        Text = tableSection.Title ?? string.Empty
                    };

                    // Table Section Title
                    Gtk.Label sectionTitle = new Gtk.Label();
                    sectionTitle.SetAlignment(0, 0);
                    sectionTitle.SetTextFromSpan(tableSectionSpan);
                    _root.PackStart(sectionTitle, false, false, 0);

                    // Table Section Separator
                    EventBox separator = new EventBox();
                    separator.HeightRequest = 1;
                    separator.ModifyBg(StateType.Normal, Color.Black.ToGtkColor());
                    _root.PackStart(separator, false, false, 0);

                    // Cells
                    _cells.Clear();

                    for (int j = 0; j < tableSection.Count; j++)
                    {
                        var cell = tableSection[j];

                        var renderer =
                            (Cells.CellRenderer)Internals.Registrar.Registered.GetHandlerForObject<IRegisterable>(cell);
                        var nativeCell = renderer.GetCell(cell, null, null);

                        if (nativeCell != null)
                        {
                            nativeCell.ButtonPressEvent += (sender, args) =>
                            {
                                var gtkCell = sender as CellBase;

                                if (gtkCell != null && gtkCell.Cell != null)
                                {
                                    var selectedCell = gtkCell.Cell;

                                    OnItemTapped?.Invoke(this, new ItemTappedEventArgs(selectedCell));
                                }
                            };
                            _cells.Add(nativeCell);
                        }
                    }

                    foreach (var cell in _cells)
                    {
                        _root.PackStart(cell, false, false, 0);
                    }
                }
            }
        }
    }
}