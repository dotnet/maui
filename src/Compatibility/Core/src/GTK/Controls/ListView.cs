using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GLib;
using Gtk;
using OpenTK.Input;
using Microsoft.Maui.Controls.Compatibility.Internals;
using Microsoft.Maui.Controls.Compatibility.Platform.GTK.Cells;
using Microsoft.Maui.Controls.Compatibility.Platform.GTK.Extensions;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK.Controls
{
	public class ItemTappedEventArgs : EventArgs
	{
		private object _item;
		private MouseButton _mouseButton;

		public object Item
		{
			get
			{
				return _item;
			}
		}

		public MouseButton MouseButton
		{
			get
			{
				return _mouseButton;
			}
		}

		public ItemTappedEventArgs(object item, MouseButton button)
		{
			_item = item;
			_mouseButton = button;
		}
	}

	public class SelectedItemEventArgs : EventArgs
	{
		private object _selectedItem;

		public object SelectedItem
		{
			get
			{
				return _selectedItem;
			}
		}

		public SelectedItemEventArgs(object selectedItem)
		{
			_selectedItem = selectedItem;
		}
	}

	public class ListViewSeparator : EventBox
	{
		public ListViewSeparator()
		{
			HeightRequest = 1;
			ModifyBg(StateType.Normal, Color.Gray.ToGtkColor());    // Default Color: Gray
			VisibleWindow = false;
		}
	}

	public enum State : uint
	{
		Started,
		Loading,
		Completed,
		Finished
	};

	public class IdleData
	{
		public State LoadState;
		public uint LoadId;
		public ListStore ListStore;
		public int NumItems;
		public int NumLoaded;
		public List Items;
	}

	public class ListView : ScrolledWindow
	{
		private const int RefreshHeight = 48;

		private VBox _root;
		private EventBox _headerContainer;
		private Widget _header;
		private VBox _list;
		private EventBox _footerContainer;
		private Widget _footer;
		private Viewport _viewPort;
		private IEnumerable<Widget> _cells;
		private List<ListViewSeparator> _separators;
		private object _selectedItem;
		private Table _refreshHeader;
		private ImageButton _refreshButton;
		private Gtk.Label _refreshLabel;
		private bool _isPullToRequestEnabled;
		private bool _refreshing;
		private IdleData _data;
		private ListStore _store = null;
		private List _items;
		private CellBase _selectedCell;
		private Gdk.Color _selectionColor;

		public delegate void ItemTappedEventHandler(object sender, ItemTappedEventArgs args);
		public event ItemTappedEventHandler OnItemTapped = null;

		public delegate void SelectedItemEventHandler(object sender, SelectedItemEventArgs args);
		public event SelectedItemEventHandler OnSelectedItemChanged = null;

		public delegate void RefreshEventHandler(object sender, EventArgs args);
		public event RefreshEventHandler OnRefresh = null;

		public ListView()
		{
			BuildListView();

			_selectionColor = DefaultSelectionColor;
		}

		public override void Destroy()
		{
			_store?.Dispose();
			_store = null;
			_root = null;
			_refreshButton = null;
			_refreshLabel = null;
			_headerContainer = null;
			_header = null;
			_list = null;
			_footerContainer = null;
			_footer = null;
			_viewPort = null;
			_refreshHeader = null;
			base.Destroy();
		}

		public static Gdk.Color DefaultSelectionColor = Color.FromHex("#3498DB").ToGtkColor();

		public Widget Header
		{
			get
			{
				return _header;
			}
			set
			{
				if (_header != value)
				{
					_header = value;
					RefreshHeader(_header);
				}
			}
		}

		public IEnumerable<Widget> Items
		{
			get
			{
				return _cells;
			}
			set
			{
				_cells = value;
				_items = new List(typeof(CellBase));
				PopulateData(_items);
			}
		}

		public Widget Footer
		{
			get
			{
				return _footer;
			}
			set
			{
				if (_footer != value)
				{
					_footer = value;
					RefreshFooter(_footer);
				}
			}
		}

		public object SelectedItem
		{
			get { return _selectedItem; }
			set
			{
				if (value != _selectedItem)
				{
					UpdateSelectedItem(value);
				}
			}
		}

		public bool IsPullToRequestEnabled
		{
			get { return _isPullToRequestEnabled; }
			set { _isPullToRequestEnabled = value; }
		}

		public bool Refreshing
		{
			get { return _refreshing; }
			set { _refreshing = value; }
		}

		public Gdk.Color SelectionColor
		{
			get
			{
				return _selectionColor;
			}

			set
			{
				_selectionColor = value;
				SelectionColorUpdated();
			}
		}

		public void SetBackgroundColor(Gdk.Color backgroundColor)
		{
			if (_root != null)
			{
				_root.ModifyBg(StateType.Normal, backgroundColor);
				_viewPort.ModifyBg(StateType.Normal, backgroundColor);

				if (_headerContainer != null && !_headerContainer.Children.Any())
				{
					_headerContainer.ModifyBg(StateType.Normal, backgroundColor);
				}

				if (_footerContainer != null && !_footerContainer.Children.Any())
				{
					_footerContainer.ModifyBg(StateType.Normal, backgroundColor);
				}
			}
		}

		public void SetSeparatorColor(Gdk.Color separatorColor)
		{
			foreach (var separator in _separators)
			{
				separator.ModifyBg(StateType.Normal, separatorColor);
				separator.VisibleWindow = true;
			}
		}

		public void SetSeparatorVisibility(bool visible)
		{
			foreach (var separator in _separators)
			{
				separator.HeightRequest = visible ? 1 : 0;
			}
		}

		public void UpdatePullToRefreshEnabled(bool isPullToRequestEnabled)
		{
			IsPullToRequestEnabled = isPullToRequestEnabled;

			if (_refreshHeader == null)
			{
				return;
			}

			if (IsPullToRequestEnabled)
			{
				_root.RemoveFromContainer(_refreshHeader);
				_root.PackStart(_refreshHeader, false, false, 0);
				_root.ReorderChild(_refreshHeader, 0);
			}
			else
			{
				_root.RemoveFromContainer(_refreshHeader);
			}
		}

		public void UpdateIsRefreshing(bool refreshing)
		{
			Refreshing = refreshing;

			if (Refreshing)
			{
				_refreshHeader.Attach(_refreshLabel, 0, 1, 0, 1);
			}
			else
			{
				_refreshHeader.RemoveFromContainer(_refreshLabel);
			}

			_refreshButton.Visible = !Refreshing;
			_refreshLabel.Visible = Refreshing;
		}

		public void SetSeletedItem(object selectedItem)
		{
			if (selectedItem == null)
			{
				return;
			}

			SelectedItem = selectedItem;
		}

		private void BuildListView()
		{
			_items = new List(typeof(CellBase));

			CanFocus = true;
			ShadowType = ShadowType.None;
			BorderWidth = 0;
			HscrollbarPolicy = PolicyType.Never;
			VscrollbarPolicy = PolicyType.Automatic;

			_root = new VBox();
			_refreshHeader = new Table(1, 1, true);
			_refreshHeader.HeightRequest = RefreshHeight;

			// Refresh Loading
			_refreshLabel = new Gtk.Label();
			_refreshLabel.Text = "Loading";

			// Refresh Button
			_refreshButton = new ImageButton();
			_refreshButton.LabelWidget.SetTextFromSpan(
				new Span()
				{
					Text = "Refresh"
				});
			_refreshButton.ImageWidget.Stock = Stock.Refresh;
			_refreshButton.SetImagePosition(PositionType.Left);
			_refreshButton.Clicked += (sender, args) =>
			{
				OnRefresh?.Invoke(this, new EventArgs());
			};

			_refreshHeader.Attach(_refreshButton, 0, 1, 0, 1);

			_root.PackStart(_refreshHeader, false, false, 0);

			// Header
			_headerContainer = new EventBox();
			_root.PackStart(_headerContainer, false, false, 0);

			// List
			_list = new VBox();
			_separators = new List<ListViewSeparator>();
			_root.PackStart(_list, true, true, 0);

			// Footer
			_footerContainer = new EventBox();
			_root.PackStart(_footerContainer, false, false, 0);

			_viewPort = new Viewport();
			_viewPort.ShadowType = ShadowType.None;
			_viewPort.BorderWidth = 0;
			_viewPort.Add(_root);

			Add(_viewPort);

			ShowAll();
		}

		private void RefreshHeader(Widget newHeader)
		{
			if (_headerContainer != null)
			{
				foreach (var child in _headerContainer.Children)
				{
					_headerContainer.RemoveFromContainer(child);
					child.Dispose();
					child.Destroy();
				}
			}

			if (newHeader != null)
			{
				_header = newHeader;
				_headerContainer.Add(_header);
				_header.ShowAll();
			}
		}

		private void RefreshFooter(Widget newFooter)
		{
			if (_footerContainer != null)
			{
				foreach (var child in _footerContainer.Children)
				{
					_footerContainer.RemoveFromContainer(child);
					child.Dispose();
					child.Destroy();
				}
			}

			if (newFooter != null)
			{
				_footer = newFooter;
				_footerContainer.Add(_footer);
				_footer.ShowAll();
			}
		}

		private void LazyLoadItems(List items)
		{
			_data = new IdleData();

			_data.Items = items;
			_data.NumItems = 0;
			_data.NumLoaded = 0;
			_data.ListStore = _store;
			_data.LoadState = Controls.State.Started;
			_data.LoadId = Idle.Add(new IdleHandler(LoadItems));
		}

		private bool LoadItems()
		{
			IdleData id = _data;
			CellBase obj;
			TreeIter iter;

			// Make sure we're in the right state 
			var isLoading = (id.LoadState == Controls.State.Started) ||
				(id.LoadState == Controls.State.Loading);

			if (!isLoading)
			{
				id.LoadState = Controls.State.Completed;
				return false;
			}

			// No items 
			if (id.Items.Count == 0)
			{
				id.LoadState = Controls.State.Completed;
				return false;
			}

			// First run 
			if (id.NumItems == 0)
			{
				id.NumItems = id.Items.Count;
				id.NumLoaded = 0;
				id.LoadState = Controls.State.Loading;
			}

			// Get the item in the list at pos n_loaded 
			obj = id.Items[id.NumLoaded] as CellBase;

			// Append the row to the store
			iter = id.ListStore.AppendValues(obj);

			// Fill in the row at position n_loaded
			id.ListStore.SetValue(iter, 0, obj);

			id.NumLoaded += 1;

			// Update UI with every item
			UpdateItem(obj);

			// We loaded everything, so we can change state and remove the idle callback function
			if (id.NumLoaded == id.NumItems)
			{
				id.LoadState = Controls.State.Completed;
				id.NumLoaded = 0;
				id.NumItems = 0;
				id.Items = null;

				CleanupLoadItems();

				return false;
			}
			else
			{
				return true;
			}
		}

		private void UpdateItem(CellBase cell)
		{
			cell.ButtonPressEvent += (sender, args) =>
			{
				var gtkCell = sender as CellBase;

				if (gtkCell != null && gtkCell.Cell != null)
				{
					SelectedItem = gtkCell.Item;

					MarkCellAsSelected(gtkCell);

					OnItemTapped?.Invoke(this, new ItemTappedEventArgs(SelectedItem, (MouseButton)args.Event.Button - 1));
				}
			};

			cell.VisibleWindow = false;

			_list.PackStart(cell, false, false, 0);
			cell.ShowAll();

			var separator = new ListViewSeparator();
			_separators.Add(separator);
			_list.PackStart(separator, false, false, 0);
			separator.ShowAll();
		}

		private void CleanupLoadItems()
		{
			Debug.Assert(_data.LoadState == Controls.State.Completed);

			_list.ShowAll();

			if (_data.ListStore == null)
				Debug.WriteLine("Something was wrong!");
		}

		private void PopulateData(List items)
		{
			_store = new ListStore(typeof(CellBase));

			foreach (var cell in _cells)
			{
				items.Append(cell);
			}

			ClearList();
			LazyLoadItems(items);
		}

		private void ClearList()
		{
			_selectedCell = null;

			if (_list != null)
			{
				foreach (var child in _list.Children)
				{
					_list.RemoveFromContainer(child);
				}
			}

			if (_separators != null)
			{
				_separators.Clear();
			}
		}

		private void UpdateSelectedItem(object value)
		{
			_selectedItem = value;

			CellBase cell = _list.Children.OfType<CellBase>().FirstOrDefault(c => c.Item == value);
			MarkCellAsSelected(cell);

			OnSelectedItemChanged?.Invoke(this, new SelectedItemEventArgs(_selectedItem));
		}

		private void MarkCellAsSelected(CellBase cell)
		{
			if (cell == null)
				return;

			if (cell.Cell.GetIsGroupHeader<ItemsView<Cell>, Cell>())
				return;

			foreach (var childCell in _list.Children.OfType<CellBase>())
			{
				bool isTargetCell = cell == childCell;

				childCell.VisibleWindow = isTargetCell;

				if (isTargetCell)
				{
					_selectedCell = childCell;
					childCell.ModifyBg(StateType.Normal, _selectionColor);
				}
			}
		}

		private void SelectionColorUpdated()
		{
			if (_selectedCell == null)
				return;

			_selectedCell.ModifyBg(StateType.Normal, _selectionColor);
		}
	}
}
