using System;
using System.Collections.Generic;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using AListView = Android.Widget.ListView;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
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
				// 1 for the headers + 1 for each non header cell
				var viewTypeCount = 1;
				foreach (var b in IsHeaderCache)
					if (!b)
						viewTypeCount++;
				return viewTypeCount;
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

		public override AView GetView(int position, AView convertView, ViewGroup parent)
		{
			bool isHeader, nextIsHeader;
			Cell item = GetCellForPosition(position, out isHeader, out nextIsHeader);
			if (item == null)
				return new AView(Context);

			var makeBline = true;
			var layout = convertView as ConditionalFocusLayout;
			if (layout != null)
			{
				makeBline = false;
				convertView = layout.GetChildAt(0);
			}
			else
				layout = new ConditionalFocusLayout(Context) { Orientation = Orientation.Vertical };

			AView aview = CellFactory.GetCell(item, convertView, parent, Context, _view);

			if (!makeBline)
			{
				if (convertView != aview)
				{
					layout.RemoveViewAt(0);
					layout.AddView(aview, 0);
				}
			}
			else
				layout.AddView(aview, 0);

			AView bline;
			if (makeBline)
			{
				bline = new AView(Context) { LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 1) };

				layout.AddView(bline);
			}
			else
				bline = layout.GetChildAt(1);

			if (isHeader)
				bline.SetBackgroundColor(Color.Accent.ToAndroid());
			else if (nextIsHeader)
				bline.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
			else
			{
				using (var value = new TypedValue())
				{
					int id = global::Android.Resource.Drawable.DividerHorizontalDark;
					if (Context.Theme.ResolveAttribute(global::Android.Resource.Attribute.ListDivider, value, true))
						id = value.ResourceId;
					else if (Context.Theme.ResolveAttribute(global::Android.Resource.Attribute.Divider, value, true))
						id = value.ResourceId;

					bline.SetBackgroundResource(id);
				}
			}

			layout.ApplyTouchListenersToSpecialCells(item);

			if (_restoreFocus == item)
			{
				if (!aview.HasFocus)
					aview.RequestFocus();

				_restoreFocus = null;
			}
			else if (aview.HasFocus)
				aview.ClearFocus();

			return layout;
		}

		public override bool IsEnabled(int position)
		{
			bool isHeader, nextIsHeader;
			Cell item = GetCellForPosition(position, out isHeader, out nextIsHeader);
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