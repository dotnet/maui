using System;
using System.Collections.Generic;
using Gtk;
using OpenTK.Input;
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
				_source = value;
				RefreshSource(_source);
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

		int GetUnevenRowCellHeight(Container cell)
		{
			int height = -1;

			var formsCell = GetXamarinFormsCell(cell);

			if (formsCell != null)
			{
				height = Convert.ToInt32(formsCell.Height);
			}

			return height;
		}

		Cell GetXamarinFormsCell(Container cell)
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

		void BuildTableView()
		{
			CanFocus = true;
			ShadowType = ShadowType.None;
			HscrollbarPolicy = PolicyType.Never;
			VscrollbarPolicy = PolicyType.Automatic;
			BorderWidth = 0;

			_root = new VBox(false, 0);

			Viewport viewPort = new Viewport
			{
				ShadowType = ShadowType.None
			};

			viewPort.Add(_root);

			Add(viewPort);

			_cells = new List<Container>();
		}

		void RefreshSource(TableRoot source)
		{
			// Clear
			_cells.Clear();

			foreach (var child in _root.AllChildren)
			{
				_root.RemoveFromContainer((Widget)child);
			}

			// Add Title
			if (!string.IsNullOrEmpty(source.Title))
			{
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
				if (source[i] is TableSection tableSection)
				{
					var tableSectionSpan = new Span()
					{
						FontSize = 12,
						Text = tableSection.Title ?? string.Empty,
						TextColor = tableSection.TextColor
					};

					// Table Section Title
					Gtk.Label sectionTitle = new Gtk.Label();
					sectionTitle.SetAlignment(0, 0);
					sectionTitle.SetTextFromSpan(tableSectionSpan);
					_root.PackStart(sectionTitle, false, false, 0);

					// Table Section Separator
					EventBox separator = new EventBox
					{
						HeightRequest = 1
					};

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
								if (sender is CellBase gtkCell && gtkCell.Cell != null)
								{
									var selectedCell = gtkCell.Cell;

									OnItemTapped?.Invoke(this, new ItemTappedEventArgs(selectedCell, (MouseButton)args.Event.Button - 1));
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

				// Refresh
				_root.ShowAll();
			}
		}
	}
}
