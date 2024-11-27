#nullable disable
using System;
using System.Collections.Generic;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using AListView = Android.Widget.ListView;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class TableViewModelRenderer : CellAdapter
	{
		readonly TableView _view;
		protected readonly Context Context;
		ITableViewController Controller => _view;
		Cell _restoreFocus;
		Cell[] _cellCache;

		Cell[] CellCache
		{
			get
			{
				if (_cellCache == null)
					FillCache();
				return _cellCache;
			}
		}
		bool[] _isHeaderCache;
		bool[] IsHeaderCache
		{
			get
			{
				if (_isHeaderCache == null)
					FillCache();
				return _isHeaderCache;
			}
		}
		bool[] _nextIsHeaderCache;
		bool[] NextIsHeaderCache
		{
			get
			{
				if (_nextIsHeaderCache == null)
					FillCache();
				return _nextIsHeaderCache;
			}
		}

		// This resource shouldn't change during the application lifecycle, so we only need to grab it once 
		// and we can use it for every TableView
		static int? _dividerResourceId;

		int DividerResourceId
		{
			get
			{
				if (_dividerResourceId is null)
				{
					using var value = new TypedValue();

					_dividerResourceId = global::Android.Resource.Drawable.DividerHorizontalDark;
					if (Context.Theme.ResolveAttribute(global::Android.Resource.Attribute.ListDivider, value, true))
					{
						_dividerResourceId = value.ResourceId;
					}
					else if (Context.Theme.ResolveAttribute(global::Android.Resource.Attribute.Divider, value, true))
					{
						_dividerResourceId = value.ResourceId;
					}
				}

				return _dividerResourceId.Value;
			}
		}

		public TableViewModelRenderer(Context context, AListView listView, TableView view) : base(context)
		{
			_view = view;
			Context = context;

			Controller.ModelChanged += OnModelChanged;

			listView.OnItemClickListener = this;
			listView.OnItemLongClickListener = this;
		}

		public override int Count => CellCache.Length;

		public override object this[int position]
		{
			get
			{
				if (position < 0 || position >= CellCache.Length)
					return null;

				return CellCache[position];
			}
		}

		public override int ViewTypeCount
		{
			get
			{
				// The GetView implementation literally only returns ConditionalFocusLayout. There is only one type.
				return 1;
			}
		}

		public override int GetItemViewType(int position)
		{
			// Tell the adapter not to attempt to re-use recycled cells; because this currently only returns ConditionalFocusLayouts,
			// there's no actual type count or item type information that allows us to accurately re-use cells. The CFLs we get from
			// the convertView parameters may have entirely types of content, or they may have ViewCells with arbitrary content. And
			// the cell content (the native views) is tied to the virtual Cells anyway, so the thing the adapter gives us to "reuse"
			// may not be safe for reuse. Our one-to-one VirtualView<->(Renderer/Handler) design is not currently compatible with the
			// ListView's recycling scheme, so we need to turn that off.

			return IAdapter.IgnoreItemViewType;
		}

		public override bool AreAllItemsEnabled()
		{
			return false;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override AView GetView(int position, AView convertView, ViewGroup parent)
		{
			Cell item = GetCellForPosition(position, out var isHeader, out var nextIsHeader);

			if (item == null)
			{
				return new AView(Context);
			}

			if (convertView is ConditionalFocusLayout cfl && cfl.ChildCount > 0)
			{
				convertView = cfl.GetChildAt(0);
			}

			AView nativeCellContent = CellFactory.GetCell(item, convertView, parent, Context, _view);

			// The cell content we get back might already be in a ConditionalFocusLayout; if it is, 
			// we'll just use that. If not, we'll need to create one and add the content to it

			if (nativeCellContent.Parent is not ConditionalFocusLayout layout)
			{
				layout = new ConditionalFocusLayout(Context) { Orientation = Orientation.Vertical };
				layout.AddView(nativeCellContent);
			}

			UpdateDivider(isHeader, nextIsHeader, layout);

			layout.ApplyTouchListenersToSpecialCells(item);

			UpdateCellFocus(item, nativeCellContent);

			return layout;
		}

		void UpdateDivider(bool forHeader, bool precedingHeader, ConditionalFocusLayout wrapper)
		{
			// Android's ListView provides built-in dividers, for some reason (perhaps our rules are too complex for the built-in
			// capabilities?) we don't use those. Instead, we fake them with 1pt views at the bottom of CFLs.

			var divider = wrapper.GetChildAt(1);
			if (divider is null)
			{
				divider = new AView(Context) { LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 1) };
				wrapper.AddView(divider);
			}

			if (forHeader)
			{
				if (Application.AccentColor != null)
				{
					divider.SetBackgroundColor(Application.AccentColor.ToPlatform());
				}

				return;
			}

			if (precedingHeader)
			{
				divider.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
				return;
			}

			divider.SetBackgroundResource(DividerResourceId);
		}

		void UpdateCellFocus(Cell cell, AView nativeCell)
		{
			// If this cell is the one that's supposed to have focus, then request focus for the native cell
			if (_restoreFocus == cell)
			{
				if (!nativeCell.HasFocus)
				{
					nativeCell.RequestFocus();
				}

				_restoreFocus = null;

				return;
			}

			// Otherwise, remove focus from the native cell
			if (nativeCell.HasFocus)
			{
				nativeCell.ClearFocus();
			}
		}

		public override bool IsEnabled(int position)
		{
			Cell item = GetCellForPosition(position, out var isHeader, out _);
			return !isHeader && item.IsEnabled;
		}

		protected override Cell GetCellForPosition(int position)
		{
			bool isHeader, nextIsHeader;
			return GetCellForPosition(position, out isHeader, out nextIsHeader);
		}

		protected override void HandleItemClick(AdapterView parent, AView nview, int position, long id)
		{
			ITableModel model = Controller.Model;

			if (position < 0 || position >= CellCache.Length)
				return;

			if (IsHeaderCache[position])
				return;

			model.RowSelected(CellCache[position]);
		}

		Cell GetCellForPosition(int position, out bool isHeader, out bool nextIsHeader)
		{
			isHeader = false;
			nextIsHeader = false;

			if (position < 0 || position >= CellCache.Length)
				return null;

			isHeader = IsHeaderCache[position];
			nextIsHeader = NextIsHeaderCache[position];
			return CellCache[position];
		}

		void FillCache()
		{
			ITableModel model = Controller.Model;
			int sectionCount = model.GetSectionCount();

			var newCellCache = new List<Cell>();
			var newIsHeaderCache = new List<bool>();
			var newNextIsHeaderCache = new List<bool>();

			for (var sectionIndex = 0; sectionIndex < sectionCount; sectionIndex++)
			{
				var sectionTitle = model.GetSectionTitle(sectionIndex);
				var sectionTextColor = model.GetSectionTextColor(sectionIndex);
				var sectionRowCount = model.GetRowCount(sectionIndex);

				if (!string.IsNullOrEmpty(sectionTitle))
				{
					Cell headerCell = model.GetHeaderCell(sectionIndex);
					if (headerCell == null)
						headerCell = new TextCell { Text = sectionTitle, TextColor = sectionTextColor };
					headerCell.Parent = _view;

					newIsHeaderCache.Add(true);
					newNextIsHeaderCache.Add(sectionRowCount == 0 && sectionIndex < sectionCount - 1);
					newCellCache.Add(headerCell);
				}

				for (int i = 0; i < sectionRowCount; i++)
				{
					newIsHeaderCache.Add(false);
					newNextIsHeaderCache.Add(i == sectionRowCount - 1 && sectionIndex < sectionCount - 1);
					newCellCache.Add((Cell)model.GetItem(sectionIndex, i));
				}
			}

			_cellCache = newCellCache.ToArray();
			_isHeaderCache = newIsHeaderCache.ToArray();
			_nextIsHeaderCache = newNextIsHeaderCache.ToArray();
		}

		void InvalidateCellCache()
		{
			_cellCache = null;
			_isHeaderCache = null;
			_nextIsHeaderCache = null;
		}

		void OnModelChanged(object sender, EventArgs e)
		{
			InvalidateCellCache();
			NotifyDataSetChanged();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				InvalidateCellCache();
				Controller.ModelChanged -= OnModelChanged;
			}

			base.Dispose(disposing);
		}
	}
}