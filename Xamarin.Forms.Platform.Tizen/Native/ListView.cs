using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms.Internals;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Type alias which identifies list of cells whose data model was transformed by Xamarin.
	/// </summary>
	using GroupList = TemplatedItemsList<ItemsView<Cell>, Cell>;

	/// <summary>
	/// Native ListView implementation for Xamarin renderer
	/// </summary>
	/// <remarks> 
	/// This internally uses GenList class.
	/// One should note that it is optimized for displaying many elements which may be
	/// unavailable at first. This means that only currently visible elements will be constructed.
	/// Whenever element disappears from visible space its content is destroyed for time being.
	/// This is carried out by so called Cell Handlers.
	/// </remarks>
	public class ListView : GenList
	{
		/// <summary>
		/// ItemContext helper class. This represents the association between Xamarin.Forms.Cell and
		/// native elements. It also stores useful context for them.
		/// </summary>
		public class ItemContext
		{
			public ItemContext()
			{
				Item = null;
				Cell = null;
				Renderer = null;
				ListOfSubItems = null;
			}

			public GenItem Item;
			public Cell Cell;
			public bool IsGroupItem;
			public CellRenderer Renderer;
			internal TemplatedItemsList<ItemsView<Cell>, Cell> ListOfSubItems;
		}

		/// <summary>
		/// The item context list for each added element.
		/// </summary>
		readonly List<ItemContext> _itemContextList = new List<ItemContext>();

		/// <summary>
		/// Registered cell handlers.
		/// </summary>
		protected readonly IDictionary<Type, CellRenderer> _cellRendererCache = new Dictionary<Type, CellRenderer>();

		/// <summary>
		/// Registered group handlers.
		/// </summary>
		protected readonly IDictionary<Type, CellRenderer> _groupCellRendererCache = new Dictionary<Type, CellRenderer>();

		/// <summary>
		/// The header context.
		/// </summary>
		ItemContext _headerContext;

		/// <summary>
		/// The header element.
		/// </summary>
		VisualElement _headerElement;

		/// <summary>
		/// The footer context.
		/// </summary>
		ItemContext _footerContext;

		/// <summary>
		/// The footer element.
		/// </summary>
		VisualElement _footerElement;

		/// <summary>
		/// The item class for header and footer.
		/// </summary>
		GenItemClass _headerFooterItemClass = null;

		/// <summary>
		/// Gets or sets a value indicating whether this instance has grouping enabled.
		/// </summary>
		/// <value><c>true</c> if this instance has grouping enabled.</value>
		public bool IsGroupingEnabled { get; set; }

		/// <summary>
		/// Constructor of ListView native control.
		/// </summary>
		/// <param name="parent">ElmSharp object which is parent of particular list view</param>
		public ListView(EvasObject parent)
			: base(parent)
		{
		}

		protected ListView() : base()
		{
		}

		/// <summary>
		/// Gets the item context based on Cell item.
		/// </summary>
		/// <returns>The item context.</returns>
		/// <param name="cell">Cell for which context should be found.</param>
		internal ItemContext GetItemContext(Cell cell)
		{
			if (cell == null)
			{
				return null;
			}
			else
			{
				return _itemContextList.Find(X => X.Cell == cell);
			}
		}

		/// <summary>
		/// Sets the HasUnevenRows property.
		/// </summary>
		/// <param name="hasUnevenRows">If <c>true</c>, the list will allow uneven sizes for its rows.</param>
		public void SetHasUnevenRows(bool hasUnevenRows)
		{
			Homogeneous = !hasUnevenRows;
			UpdateRealizedItems();
		}

		/// <summary>
		/// Adds elements to the list and defines its presentation based on Cell type.
		/// </summary>
		/// <param name="_source">IEnumerable on Cell collection.</param>
		/// <param name="beforeCell">Cell before which new items will be placed. 
		/// Null value may also be passed as this parameter, which results in appending new items to the end.
		/// </param>
		public void AddSource(IEnumerable _source, Cell beforeCell = null)
		{
			foreach (var data in _source)
			{
				GroupList groupList = data as GroupList;
				if (groupList != null)
				{
					AddGroupItem(groupList, beforeCell);
					foreach (var item in groupList)
					{
						AddItem(item as Cell, groupList.HeaderContent);
					}
				}
				else
				{
					AddItem(data as Cell, null, beforeCell);
				}
			}
		}

		/// <summary>
		/// Deletes all items from a given group.
		/// </summary>
		/// <param name="group">Group of items to be deleted.</param>
		internal void ResetGroup(GroupList group)
		{
			var items = _itemContextList.FindAll(x => x.ListOfSubItems == group && x.Cell != group.HeaderContent);
			foreach (var item in items)
			{
				item.Item?.Delete();
			}
		}

		/// <summary>
		/// Adds items to the group.
		/// </summary>
		/// <param name="itemGroup">Group to which elements will be added.</param>
		/// <param name="newItems">New list items to be added.</param>
		/// <param name="cellBefore">A reference to the Cell already existing in a ListView.
		///  Newly added cells will be put just before this cell.</param>
		public void AddItemsToGroup(IEnumerable itemGroup, IEnumerable newItems, Cell cellBefore = null)
		{
			ItemContext groupCtx = GetItemContext((itemGroup as GroupList)?.HeaderContent);
			if (groupCtx != null)
			{
				foreach (var item in newItems)
				{
					AddItem(item as Cell, groupCtx.Cell, cellBefore);
				}
			}
		}

		/// <summary>
		/// Removes the specified cells.
		/// </summary>
		/// <param name="cells">Cells to be removed.</param>
		public void Remove(IEnumerable cells)
		{
			foreach (var data in cells)
			{
				var group = data as GroupList;
				if (group != null)
				{
					ItemContext groupCtx = GetItemContext(group.HeaderContent);
					Remove(groupCtx.ListOfSubItems);
					groupCtx.Item.Delete();
				}
				else
				{
					ItemContext itemCtx = GetItemContext(data as Cell);
					itemCtx?.Item?.Delete();
				}
			}
		}

		/// <summary>
		/// Scrolls the list to a specified cell.
		/// </summary>
		/// <remarks>
		/// Different scrolling behaviors are also possible. The element may be positioned in the center,
		/// top or bottom of the visible part of the list depending on the value of the <c>position</c> parameter.
		/// </remarks>
		/// <param name="cell">Cell which will be displayed after scrolling .</param>
		/// <param name="position">This will defines scroll to behavior based on ScrollToPosition values.</param>
		/// <param name="animated">If <c>true</c>, scrolling will be animated. Otherwise the cell will be moved instantaneously.</param>
		public void ApplyScrollTo(Cell cell, ScrollToPosition position, bool animated)
		{
			GenListItem item = GetItemContext(cell)?.Item as GenListItem;
			if (item != null)
				this.ScrollTo(item, position.ToNative(), animated);
		}

		/// <summary>
		/// Selects the specified cell.
		/// </summary>
		/// <param name="cell">Cell to be selected.</param>
		public void ApplySelectedItem(Cell cell)
		{
			GenListItem item = GetItemContext(cell)?.Item as GenListItem;
			if (item != null)
				item.IsSelected = true;
		}

		/// <summary>
		/// Sets the header.
		/// </summary>
		/// <param name="header">Header of the list.</param>
		public void SetHeader(VisualElement header)
		{
			if (header == null)
			{
				if (HasHeader())
				{
					RemoveHeader();
				}

				return;
			}

			GenItemClass headerTemplate = GetHeaderFooterItemClass();

			_headerElement = header;
			if (HasHeader())
			{
				FirstItem.UpdateItemClass(headerTemplate, header);
			}
			else
			{
				_headerContext = new ItemContext();
				_headerContext.Item = _itemContextList.Count > 0 ? InsertBefore(headerTemplate, header, FirstItem) : Append(headerTemplate, header);
				_headerContext.Item.SelectionMode = GenItemSelectionMode.None;
				_headerContext.Item.Deleted += HeaderDeletedHandler;
				_itemContextList.Insert(0, _headerContext);
			}
		}

		/// <summary>
		/// Sets the footer.
		/// </summary>
		/// <param name="footer">Footer of the list.</param>
		public void SetFooter(VisualElement footer)
		{
			if (footer == null)
			{
				if (HasFooter())
				{
					RemoveFooter();
				}
				return;
			}

			GenItemClass footerTemplate = GetHeaderFooterItemClass();

			_footerElement = footer;
			if (HasFooter())
			{
				(_footerContext.Item as GenListItem).UpdateItemClass(footerTemplate, footer);
			}
			else
			{
				_footerContext = new ItemContext();
				_footerContext.Item = Append(footerTemplate, footer);
				_footerContext.Item.SelectionMode = GenItemSelectionMode.None;
				_footerContext.Item.Deleted += FooterDeletedHandler;
				_itemContextList.Add(_footerContext);
			}
		}

		/// <summary>
		/// Removes the header.
		/// </summary>
		public void RemoveHeader()
		{
			_itemContextList.Remove(_headerContext);
			_headerContext?.Item?.Delete();
			_headerContext = null;
			_headerElement = null;
		}

		/// <summary>
		/// Removes the footer.
		/// </summary>
		public void RemoveFooter()
		{
			_itemContextList.Remove(_footerContext);
			_footerContext?.Item?.Delete();
			_footerContext = null;
			_footerElement = null;
		}

		/// <summary>
		/// Determines whether this instance has a header.
		/// </summary>
		/// <returns><c>true</c> if the header is present.</returns>
		public bool HasHeader()
		{
			return _headerContext != null;
		}

		/// <summary>
		/// Determines whether this instance has a footer.
		/// </summary>
		/// <returns><c>true</c> if the footer is present.</returns>
		public bool HasFooter()
		{
			return _footerContext != null;
		}

		/// <summary>
		/// Gets the header.
		/// </summary>
		/// <returns>The header.</returns>
		public VisualElement GetHeader()
		{
			return _headerElement;
		}

		/// <summary>
		/// Gets the footer.
		/// </summary>
		/// <returns>The footer.</returns>
		public VisualElement GetFooter()
		{
			return _footerElement;
		}

		/// <summary>
		/// Handles the header deleted event.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Empty argument.</param>
		void HeaderDeletedHandler(object sender, EventArgs e)
		{
			_itemContextList.Remove(_headerContext);
			_headerContext = null;
		}

		/// <summary>
		/// Handles the footer deleted event.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Empty argument.</param>
		void FooterDeletedHandler(object sender, EventArgs e)
		{
			_itemContextList.Remove(_footerContext);
			_footerContext = null;
		}

		/// <summary>
		/// Called every time an object gets realized.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="evt">GenListItemEventArgs.</param>
		void OnItemAppear(object sender, GenListItemEventArgs evt)
		{
			ItemContext itemContext = (evt.Item.Data as ItemContext);

			if (itemContext != null && itemContext.Cell != null)
			{
				(itemContext.Cell as ICellController).SendAppearing();
			}
		}

		/// <summary>
		/// Called every time an object gets unrealized.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="evt">GenListItemEventArgs.</param>
		void OnItemDisappear(object sender, GenListItemEventArgs evt)
		{
			ItemContext itemContext = (evt.Item.Data as ItemContext);
			if (itemContext != null && itemContext.Cell != null)
			{
				(itemContext.Cell as ICellController).SendDisappearing();
				itemContext.Renderer?.SendUnrealizedCell(itemContext.Cell);
			}
		}

		protected override void OnRealized()
		{
			base.OnRealized();
			ItemRealized += OnItemAppear;
			ItemUnrealized += OnItemDisappear;
		}

		/// <summary>
		/// A convenience shorthand method for derivate classes.
		/// </summary>
		/// <param name="cell">Cell to be added.</param>
		protected void AddCell(Cell cell)
		{
			AddItem(cell);
		}

		/// <summary>
		/// Gets the cell renderer for given cell type.
		/// </summary>
		/// <returns>The cell handler.</returns>
		/// <param name="cell">Cell to be added.</param>
		/// <param name="isGroup">If <c>true</c>, then group handlers will be included in the lookup as well.</param>
		protected virtual CellRenderer GetCellRenderer(Cell cell, bool isGroup = false)
		{
			Type type = cell.GetType();
			var cache = isGroup ? _groupCellRendererCache : _cellRendererCache;
			if (cache.ContainsKey(type))
				return cache[type];

			CellRenderer renderer = null;
			renderer = Registrar.Registered.GetHandler<CellRenderer>(type);

			if (renderer == null)
			{
				Log.Error("Cell type is not handled: {0}", cell.GetType());
				throw new ArgumentNullException("Unsupported cell type");
			}

			renderer.SetGroupMode(isGroup);

			return cache[type] = renderer;
		}

		/// <summary>
		/// Adds the group item. Group item is actually of class GroupList because
		/// group item has sub items (can be zero) which needs to be added.
		/// If beforeCell is not null, new group will be added just before it.
		/// </summary>
		/// <param name="groupList">Group to be added.</param>
		/// <param name="beforeCell">Before cell.</param>
		void AddGroupItem(GroupList groupList, Cell beforeCell = null)
		{
			Cell groupCell = groupList.HeaderContent;
			CellRenderer groupRenderer = GetCellRenderer(groupCell, true);

			ItemContext groupItemContext = new ItemContext();
			groupItemContext.Cell = groupCell;
			groupItemContext.Renderer = groupRenderer;
			groupItemContext.IsGroupItem = true;
			groupItemContext.ListOfSubItems = groupList;
			_itemContextList.Add(groupItemContext);

			if (beforeCell != null)
			{
				GenListItem beforeItem = GetItemContext(beforeCell)?.Item as GenListItem;
				groupItemContext.Item = InsertBefore(groupRenderer.Class, groupItemContext, beforeItem, GenListItemType.Group);
			}
			else
			{
				groupItemContext.Item = Append(groupRenderer.Class, groupItemContext, GenListItemType.Group);
			}

			groupItemContext.Item.SelectionMode = GenItemSelectionMode.None;
			groupItemContext.Item.IsEnabled = groupCell.IsEnabled;
			groupItemContext.Item.Deleted += ItemDeletedHandler;

		}

		/// <summary>
		/// Adds the item.
		/// </summary>
		/// <param name="cell">Cell to be added.</param>
		/// <param name="groupCell">Group to which the new item should belong.</param>
		/// <remark>If the value of <c>groupCell</c> is not null, the new item will be put into the requested group. </remark>
		/// <param name="beforeCell">The cell before which the new item should be placed.</param>
		/// <remarks> If the value of <c>beforeCell</c> is not null, the new item will be placed just before the requested cell. </remarks>
		void AddItem(Cell cell, Cell groupCell = null, Cell beforeCell = null)
		{
			CellRenderer renderer = GetCellRenderer(cell);
			GenListItem parentItem = null;

			ItemContext itemContext = new ItemContext();
			itemContext.Cell = cell;
			itemContext.Renderer = renderer;
			_itemContextList.Add(itemContext);

			if (IsGroupingEnabled && groupCell != null)
			{
				var groupContext = GetItemContext(groupCell);
				itemContext.ListOfSubItems = groupContext.ListOfSubItems;
				parentItem = groupContext.Item as GenListItem;
			}

			if (beforeCell != null)
			{
				GenListItem beforeItem = GetItemContext(beforeCell)?.Item as GenListItem;
				itemContext.Item = InsertBefore(renderer.Class, itemContext, beforeItem, GenListItemType.Normal, parentItem);
			}
			else
			{
				itemContext.Item = Append(renderer.Class, itemContext, GenListItemType.Normal, parentItem);
			}

			itemContext.Item.SelectionMode = GenItemSelectionMode.Always;
			itemContext.Item.IsEnabled = cell.IsEnabled;
			itemContext.Item.Deleted += ItemDeletedHandler;

			cell.PropertyChanged += OnCellPropertyChanged;
			(cell as ICellController).ForceUpdateSizeRequested += OnForceUpdateSizeRequested;
		}

		/// <summary>
		/// Handles item deleted event.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Empty argument.</param>
		void ItemDeletedHandler(object sender, EventArgs e)
		{
			ItemContext itemContext = (sender as GenListItem).Data as ItemContext;
			if (itemContext.Cell != null)
			{
				itemContext.Cell.PropertyChanged -= OnCellPropertyChanged;
				(itemContext.Cell as ICellController).ForceUpdateSizeRequested -= OnForceUpdateSizeRequested;
			}
			_itemContextList.Remove(itemContext);
		}

		/// <summary>
		/// Invoked whenever the properties of data model change.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">PropertyChangedEventArgs.</param>
		/// <remarks>
		/// The purpose of this method is to propagate these changes to the presentation layer.
		/// </remarks>
		void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var cell = sender as Cell;
			var context = GetItemContext(cell);
			context.Renderer.SendCellPropertyChanged(cell, context.Item, e.PropertyName);
		}

		void OnForceUpdateSizeRequested(object sender, EventArgs e)
		{
			var cell = sender as Cell;
			var itemContext = GetItemContext(cell);
			if (itemContext.Item != null)
				itemContext.Item.Update();
		}

		/// <summary>
		/// Gets the item class used for header and footer cells.
		/// </summary>
		/// <returns>The header and footer item class.</returns>
		GenItemClass GetHeaderFooterItemClass()
		{
			if (_headerFooterItemClass == null)
			{
				_headerFooterItemClass = new GenItemClass("full")
				{
					GetContentHandler = (data, part) =>
					{
						VisualElement element = data as VisualElement;
						var renderer = Platform.GetOrCreateRenderer(element);

						if (element.MinimumHeightRequest == -1)
						{
							SizeRequest request = element.Measure(double.PositiveInfinity, double.PositiveInfinity);
							renderer.NativeView.MinimumHeight = Forms.ConvertToScaledPixel(request.Request.Height);
						}
						else
						{
							renderer.NativeView.MinimumHeight = Forms.ConvertToScaledPixel(element.MinimumHeightRequest);
						}

						(renderer as LayoutRenderer)?.RegisterOnLayoutUpdated();

						return renderer.NativeView;
					}
				};
			}
			return _headerFooterItemClass;
		}
	}
}
