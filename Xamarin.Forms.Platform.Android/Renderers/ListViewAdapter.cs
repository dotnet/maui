using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Internals;
using AListView = Android.Widget.ListView;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	internal class ListViewAdapter : CellAdapter
	{
		bool _disposed;
		static readonly object DefaultItemTypeOrDataTemplate = new object();
		const int DefaultGroupHeaderTemplateId = 0;
		const int DefaultItemTemplateId = 1;

		static int s_dividerHorizontalDarkId = int.MinValue;

		internal static readonly BindableProperty IsSelectedProperty = BindableProperty.CreateAttached("IsSelected", typeof(bool), typeof(Cell), false);

		readonly Context _context;
		protected readonly ListView _listView;
		readonly AListView _realListView;
		readonly Dictionary<DataTemplate, int> _templateToId = new Dictionary<DataTemplate, int>();
		readonly List<ConditionalFocusLayout> _layoutsCreated = new List<ConditionalFocusLayout>();
		int _dataTemplateIncrementer = 2; // lets start at not 0 because ... 

		// We will use _dataTemplateIncrementer to get the proper ViewType key for the item's DataTemplate and store these keys in  _templateToId.
		// If an item does _not_ use a DataTemplate, then the ViewType key will be DefaultItemTemplateId (1) or DefaultGroupHeaderTemplateId (0).
		// To prevent a conflict in the event that a ListView supports both templates and non-templates, we will start the DataTemplate key at 2.

		int _listCount = -1; // -1 we need to get count from the list
		Dictionary<object, Cell> _prototypicalCellByTypeOrDataTemplate;

		bool _fromNative;
		AView _lastSelected;
		WeakReference<Cell> _selectedCell;

		IListViewController Controller => _listView;
		protected ITemplatedItemsView<Cell> TemplatedItemsView => _listView;

		public ListViewAdapter(Context context, AListView realListView, ListView listView) : base(context)
		{
			_context = context;
			_realListView = realListView;
			_listView = listView;
			_prototypicalCellByTypeOrDataTemplate = new Dictionary<object, Cell>();

			if (listView.SelectedItem != null)
				SelectItem(listView.SelectedItem);

			var templatedItems = ((ITemplatedItemsView<Cell>)listView).TemplatedItems;
			templatedItems.CollectionChanged += OnCollectionChanged;
			templatedItems.GroupedCollectionChanged += OnGroupedCollectionChanged;
			listView.ItemSelected += OnItemSelected;

			realListView.OnItemClickListener = this;
			realListView.OnItemLongClickListener = this;

			MessagingCenter.Subscribe<ListViewAdapter>(this, Platform.CloseContextActionsSignalName, lva => CloseContextActions());

			InvalidateCount();
		}

		public override int Count
		{
			get
			{
				if (_listCount == -1)
				{
					var templatedItems = TemplatedItemsView.TemplatedItems;
					int count = templatedItems.Count;

					if (_listView.IsGroupingEnabled)
					{
						for (var i = 0; i < templatedItems.Count; i++)
							count += templatedItems.GetGroup(i).Count;
					}

					_listCount = count;
				}
				return _listCount;
			}
		}

		public AView FooterView { get; set; }

		public override bool HasStableIds
		{
			get { return false; }
		}

		public AView HeaderView { get; set; }

		public bool IsAttachedToWindow { get; set; }

		public override object this[int index]
		{
			get
			{
				if (_listView.IsGroupingEnabled)
				{
					Cell cell = GetCellForPosition(index);
					return cell.BindingContext;
				}

				return TemplatedItemsView.ListProxy[index];
			}
		}

		public override int ViewTypeCount
		{
			get
			{
				// We have a documented limit of 20 templates on Android.
				// ViewTypes are selected on a zero-based index, so this count must be at least 20 + 1.
				// Plus, we arbitrarily increased the index of the DataTemplate index by 2 (see _dataTemplateIncrementer).
				return 23;
			}
		}

		public override bool AreAllItemsEnabled()
		{
			return false;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int GetItemViewType(int position)
		{
			var group = 0;
			var row = 0;
			DataTemplate itemTemplate;
			if (!_listView.IsGroupingEnabled)
				itemTemplate = _listView.ItemTemplate;
			else
			{
				group = TemplatedItemsView.TemplatedItems.GetGroupIndexFromGlobal(position, out row);

				if (row == 0)
				{
					itemTemplate = _listView.GroupHeaderTemplate;
					if (itemTemplate == null)
						return DefaultGroupHeaderTemplateId;
				}
				else
				{
					itemTemplate = _listView.ItemTemplate;
					row--;
				}
			}

			if (itemTemplate == null)
				return DefaultItemTemplateId;

			if (itemTemplate is DataTemplateSelector selector)
			{
				object item = null;

				if (_listView.IsGroupingEnabled)
				{
					if (TemplatedItemsView.TemplatedItems.GetGroup(group).ListProxy.Count > 0)
						item = TemplatedItemsView.TemplatedItems.GetGroup(group).ListProxy[row];
				}
				else
				{
					if (TemplatedItemsView.TemplatedItems.ListProxy.Count > 0)
						item = TemplatedItemsView.TemplatedItems.ListProxy[position];
				}

				itemTemplate = selector.SelectTemplate(item, _listView);
			}

			// check again to guard against DataTemplateSelectors that return null
			if (itemTemplate == null)
				return DefaultItemTemplateId;

			if (!_templateToId.TryGetValue(itemTemplate, out int key))
			{
				_dataTemplateIncrementer++;
				key = _dataTemplateIncrementer;
				_templateToId[itemTemplate] = key;
			}

			if (key >= ViewTypeCount)
			{
				throw new Exception($"ItemTemplate count has exceeded the limit of {ViewTypeCount}" + Environment.NewLine +
									 "Please make sure to reuse DataTemplate objects");
			}

			return key;
		}

		public override AView GetView(int position, AView convertView, ViewGroup parent)
		{
			Cell cell = null;

			Performance.Start(out string reference);

			ListViewCachingStrategy cachingStrategy = Controller.CachingStrategy;
			var nextCellIsHeader = false;
			if (cachingStrategy == ListViewCachingStrategy.RetainElement || convertView == null)
			{
				if (_listView.IsGroupingEnabled)
				{
					List<Cell> cells = GetCellsFromPosition(position, 2);
					if (cells.Count > 0)
						cell = cells[0];

					if (cells.Count == 2)
						nextCellIsHeader = cells[1].GetIsGroupHeader<ItemsView<Cell>, Cell>();
				}

				if (cell == null)
				{
					cell = GetCellForPosition(position);

					if (cell == null)
					{
						Performance.Stop(reference);

						return new AView(_context);
					}
				}
			}

			var cellIsBeingReused = false;
			var layout = convertView as ConditionalFocusLayout;
			if (layout != null)
			{
				cellIsBeingReused = true;
				convertView = layout.GetChildAt(0);
			}
			else
			{
				layout = new ConditionalFocusLayout(_context) { Orientation = Orientation.Vertical };
				_layoutsCreated.Add(layout);
			}

			if (((cachingStrategy & ListViewCachingStrategy.RecycleElement) != 0) && convertView != null)
			{
				var boxedCell = convertView as INativeElementView;
				if (boxedCell == null)
				{
					throw new InvalidOperationException($"View for cell must implement {nameof(INativeElementView)} to enable recycling.");
				}
				cell = (Cell)boxedCell.Element;

#pragma warning disable CS0618 // Type or member is obsolete
				// The Platform property is no longer necessary, but we have to set it because some third-party
				// library might still be retrieving it and using it
				cell.Platform = _listView.Platform;
#pragma warning restore CS0618 // Type or member is obsolete

				ICellController cellController = cell;
				cellController.SendDisappearing();

				int row = position;
				var group = 0;
				var templatedItems = TemplatedItemsView.TemplatedItems;
				if (_listView.IsGroupingEnabled)
					group = templatedItems.GetGroupIndexFromGlobal(position, out row);

				var templatedList = templatedItems.GetGroup(group);

				if (_listView.IsGroupingEnabled)
				{
					if (row == 0)
						templatedList.UpdateHeader(cell, group);
					else
						templatedList.UpdateContent(cell, row - 1);
				}
				else
					templatedList.UpdateContent(cell, row);

				cellController.SendAppearing();

				if (cell.BindingContext == ActionModeObject)
				{
					ActionModeContext = cell;
					ContextView = layout;
				}

				if (ReferenceEquals(_listView.SelectedItem, cell.BindingContext))
					Select(_listView.IsGroupingEnabled ? row - 1 : row, layout);
				else if (cell.BindingContext == ActionModeObject)
					SetSelectedBackground(layout, true);
				else
					UnsetSelectedBackground(layout);

				Performance.Stop(reference);
				return layout;
			}

			AView view = CellFactory.GetCell(cell, convertView, parent, _context, _listView);

			Performance.Start(reference, "AddView");

			if (cellIsBeingReused)
			{
				if (convertView != view)
				{
					layout.RemoveViewAt(0);
					layout.AddView(view, 0);
				}
			}
			else
				layout.AddView(view, 0);

			Performance.Stop(reference, "AddView");

			bool isHeader = cell.GetIsGroupHeader<ItemsView<Cell>, Cell>();

			AView bline;

			bool isSeparatorVisible = _listView.SeparatorVisibility == SeparatorVisibility.Default;

			if (isSeparatorVisible)
			{
				UpdateSeparatorVisibility(cell, cellIsBeingReused, isHeader, nextCellIsHeader, isSeparatorVisible, layout, out bline);

				UpdateSeparatorColor(isHeader, bline);
			}
			else if (layout.ChildCount > 1)
			{
				layout.RemoveViewAt(1);
			}

			if ((bool)cell.GetValue(IsSelectedProperty))
				Select(position, layout);
			else
				UnsetSelectedBackground(layout);

			layout.ApplyTouchListenersToSpecialCells(cell);

			Performance.Stop(reference);

			return layout;
		}

		internal void InvalidatePrototypicalCellCache()
		{
			_prototypicalCellByTypeOrDataTemplate.Clear();
		}

		internal ITemplatedItemsList<Cell> GetTemplatedItemsListForPath(int indexPath)
		{
			var templatedItems = TemplatedItemsView.TemplatedItems;
			if (_listView.IsGroupingEnabled)
				templatedItems = (ITemplatedItemsList<Cell>)((IList)templatedItems)[indexPath];

			return templatedItems;
		}

		internal DataTemplate GetDataTemplateForPath(int indexPath)
		{
			var templatedItemsList = GetTemplatedItemsListForPath(indexPath);
			var item = templatedItemsList.ListProxy[indexPath];
			return templatedItemsList.SelectDataTemplate(item);
		}

		internal Type GetItemTypeForPath(int indexPath)
		{
			var templatedItemsList = GetTemplatedItemsListForPath(indexPath);
			var item = templatedItemsList.ListProxy[indexPath];
			return item.GetType();
		}

		internal Cell GetCellForPath(int indexPath)
		{
			var templatedItemsList = GetTemplatedItemsListForPath(indexPath);
			var cell = templatedItemsList[indexPath];
			return cell;
		}

		internal Cell GetPrototypicalCell(int indexPath)
		{
			var itemTypeOrDataTemplate = default(object);

			var cachingStrategy = _listView.CachingStrategy;
			if (cachingStrategy == ListViewCachingStrategy.RecycleElement)
				itemTypeOrDataTemplate = GetDataTemplateForPath(indexPath);

			else if (cachingStrategy == ListViewCachingStrategy.RecycleElementAndDataTemplate)
				itemTypeOrDataTemplate = GetItemTypeForPath(indexPath);

			else // ListViewCachingStrategy.RetainElement
				return GetCellForPosition(indexPath);

			if (itemTypeOrDataTemplate == null)
				itemTypeOrDataTemplate = DefaultItemTypeOrDataTemplate;

			Cell protoCell;
			if (!_prototypicalCellByTypeOrDataTemplate.TryGetValue(itemTypeOrDataTemplate, out protoCell))
			{
				// cache prototypical cell by item type; Items of the same Type share
				// the same DataTemplate (this is enforced by RecycleElementAndDataTemplate)
				protoCell = GetCellForPosition(indexPath);
				_prototypicalCellByTypeOrDataTemplate[itemTypeOrDataTemplate] = protoCell;
			}

			var templatedItems = GetTemplatedItemsListForPath(indexPath);
			return templatedItems.UpdateContent(protoCell, indexPath);
		}

		public override bool IsEnabled(int position)
		{
			ListView list = _listView;

			ITemplatedItemsView<Cell> templatedItemsView = list;
			if (list.IsGroupingEnabled)
			{
				int leftOver;
				templatedItemsView.TemplatedItems.GetGroupIndexFromGlobal(position, out leftOver);
				return leftOver > 0;
			}

			Cell item = GetPrototypicalCell(position);
			return item?.IsEnabled ?? false;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				CloseContextActions();

				MessagingCenter.Unsubscribe<ListViewAdapter>(this, Platform.CloseContextActionsSignalName);

				_realListView.OnItemClickListener = null;
				_realListView.OnItemLongClickListener = null;

				var templatedItems = TemplatedItemsView.TemplatedItems;
				templatedItems.CollectionChanged -= OnCollectionChanged;
				templatedItems.GroupedCollectionChanged -= OnGroupedCollectionChanged;
				_listView.ItemSelected -= OnItemSelected;

				if (_lastSelected != null)
				{
					_lastSelected.Dispose();
					_lastSelected = null;
				}

				DisposeCells();
			}

			base.Dispose(disposing);
		}

		protected override Cell GetCellForPosition(int position)
		{
			return GetCellsFromPosition(position, 1).FirstOrDefault();
		}

		protected override void HandleItemClick(AdapterView parent, AView view, int position, long id)
		{
			Cell cell = null;

			if ((Controller.CachingStrategy & ListViewCachingStrategy.RecycleElement) != 0)
			{
				AView cellOwner = view;
				var layout = cellOwner as ConditionalFocusLayout;
				if (layout != null)
					cellOwner = layout.GetChildAt(0);
				cell = (Cell)(cellOwner as INativeElementView)?.Element;
			}

			// All our ListView's have called AddHeaderView. This effectively becomes index 0, so our index 0 is index 1 to the listView.
			position--;

			if (position < 0 || position >= Count)
				return;

			if (_lastSelected != view)
				_fromNative = true;
			if (_listView.SelectionMode != ListViewSelectionMode.None)
				Select(position, view);
			Controller.NotifyRowTapped(position, cell);
		}

		void DisposeCells()
		{
			var cellCount = _layoutsCreated.Count;

			for (int i = 0; i < cellCount; i++)
			{
				var layout = _layoutsCreated[i];

				if (layout.IsDisposed())
					continue;

				DisposeOfConditionalFocusLayout(layout);
			}

			_layoutsCreated.Clear();
		}

		void DisposeOfConditionalFocusLayout(ConditionalFocusLayout layout)
		{
			var renderedView = layout?.GetChildAt(0);

			var element = (renderedView as INativeElementView)?.Element;
			var view = (element as ViewCell)?.View;

			if (view != null)
			{
				var renderer = Platform.GetRenderer(view);

				if (renderer == renderedView)
					element.ClearValue(Platform.RendererProperty);

				renderer?.Dispose();
				renderer = null;
			}

			renderedView?.Dispose();
			renderedView = null;
		}

		// TODO: We can optimize this by storing the last position, group index and global index
		// and increment/decrement from that starting place.	
		List<Cell> GetCellsFromPosition(int position, int take)
		{
			var cells = new List<Cell>(take);
			if (position < 0)
				return cells;

			var templatedItems = TemplatedItemsView.TemplatedItems;
			var templatedItemsCount = templatedItems.Count;
			if (!_listView.IsGroupingEnabled)
			{
				for (var x = 0; x < take; x++)
				{
					if (position + x >= templatedItemsCount)
						return cells;

					cells.Add(templatedItems[x + position]);
				}

				return cells;
			}

			var i = 0;
			var global = 0;
			for (; i < templatedItemsCount; i++)
			{
				var group = templatedItems.GetGroup(i);

				if (global == position || cells.Count > 0)
				{
					//Always create a new cell if we are using the RecycleElement strategy
					var recycleElement = (_listView.CachingStrategy & ListViewCachingStrategy.RecycleElement) != 0;
					var headerCell = recycleElement ? GetNewGroupHeaderCell(group) : group.HeaderContent;
					cells.Add(headerCell);

					if (cells.Count == take)
						return cells;
				}

				global++;

				if (global + group.Count < position)
				{
					global += group.Count;
					continue;
				}

				for (var g = 0; g < group.Count; g++)
				{
					if (global == position || cells.Count > 0)
					{
						cells.Add(group[g]);
						if (cells.Count == take)
							return cells;
					}

					global++;
				}
			}

			return cells;
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnDataChanged();
		}

		void OnDataChanged()
		{
			InvalidateCount();
			if (ActionModeContext != null && !TemplatedItemsView.TemplatedItems.Contains(ActionModeContext))
				CloseContextActions();

			if (IsAttachedToWindow)
				NotifyDataSetChanged();
			else
			{
				// In a TabbedPage page with two pages, Page A and Page B with ListView, if A changes B's ListView,
				// we need to reset the ListView's adapter to reflect the changes on page B
				// If there header and footer are present at the reset time of the adapter
				// they will be DOUBLE added to the ViewGround (the ListView) causing indexes to be off by one. 
				_realListView.RemoveHeaderView(HeaderView);
				_realListView.RemoveFooterView(FooterView);
				_realListView.Adapter = _realListView.Adapter;
				_realListView.AddHeaderView(HeaderView);
				_realListView.AddFooterView(FooterView);
			}
		}

		void OnGroupedCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnDataChanged();
		}

		void OnItemSelected(object sender, SelectedItemChangedEventArgs eventArg)
		{
			if (_fromNative)
			{
				_fromNative = false;
				return;
			}

			SelectItem(eventArg.SelectedItem);
		}

		void Select(int index, AView view)
		{
			if (_lastSelected != null)
			{
				UnsetSelectedBackground(_lastSelected);
				Cell previousCell;
				if (_selectedCell.TryGetTarget(out previousCell))
					previousCell.SetValue(IsSelectedProperty, false);
			}

			_lastSelected = view;

			if (index == -1)
				return;

			Cell cell = GetCellForPosition(index);
			cell.SetValue(IsSelectedProperty, true);
			_selectedCell = new WeakReference<Cell>(cell);

			if (view != null)
				SetSelectedBackground(view);
		}

		void SelectItem(object item)
		{
			if (_listView == null)
				return;

			int position = TemplatedItemsView.TemplatedItems.GetGlobalIndexOfItem(item);
			AView view = null;
			if (position != -1)
				view = _realListView.GetChildAt(position + 1 - _realListView.FirstVisiblePosition);

			Select(position, view);
		}

		void UpdateSeparatorVisibility(Cell cell, bool cellIsBeingReused, bool isHeader, bool nextCellIsHeader, bool isSeparatorVisible, ConditionalFocusLayout layout, out AView bline)
		{
			bline = null;
			if (cellIsBeingReused && layout.ChildCount > 1)
			{
				layout.RemoveViewAt(1);
			}
			var makeBline = isSeparatorVisible || isHeader && isSeparatorVisible && !nextCellIsHeader;
			if (makeBline)
			{
				bline = new AView(_context) { LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 1) };
				layout.AddView(bline);
			}
			else if (layout.ChildCount > 1)
			{
				layout.RemoveViewAt(1);
			}
		}


		void UpdateSeparatorColor(bool isHeader, AView bline)
		{
			if (bline == null)
				return;

			Color separatorColor = _listView.SeparatorColor;

			if (isHeader || !separatorColor.IsDefault)
				bline.SetBackgroundColor(separatorColor.ToAndroid(Color.Accent));
			else
			{
				if (s_dividerHorizontalDarkId == int.MinValue)
				{
					using (var value = new TypedValue())
					{
						int id = global::Android.Resource.Drawable.DividerHorizontalDark;
						if (_context.Theme.ResolveAttribute(global::Android.Resource.Attribute.ListDivider, value, true))
							id = value.ResourceId;
						else if (_context.Theme.ResolveAttribute(global::Android.Resource.Attribute.Divider, value, true))
							id = value.ResourceId;

						s_dividerHorizontalDarkId = id;
					}
				}

				bline.SetBackgroundResource(s_dividerHorizontalDarkId);
			}
		}

		Cell GetNewGroupHeaderCell(ITemplatedItemsList<Cell> group)
		{
			var groupHeaderCell = _listView.TemplatedItems.GroupHeaderTemplate?.CreateContent(group.ItemsSource, _listView) as Cell;

			if (groupHeaderCell != null)
			{
				groupHeaderCell.BindingContext = group.ItemsSource;
			}
			else
			{
				groupHeaderCell = new TextCell();
				groupHeaderCell.SetBinding(TextCell.TextProperty, nameof(group.Name));
				groupHeaderCell.BindingContext = group;
			}

			groupHeaderCell.Parent = _listView;
			groupHeaderCell.SetIsGroupHeader<ItemsView<Cell>, Cell>(true);
			return groupHeaderCell;
		}

		enum CellType
		{
			Row,
			Header
		}

		protected virtual void InvalidateCount()
		{
			_listCount = -1;
		}
	}

}