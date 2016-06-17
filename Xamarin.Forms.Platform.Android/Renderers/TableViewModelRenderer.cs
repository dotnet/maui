using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using AView = Android.Views.View;
using AListView = Android.Widget.ListView;

namespace Xamarin.Forms.Platform.Android
{
	public class TableViewModelRenderer : CellAdapter
	{
		readonly TableView _view;
		protected readonly Context Context;
		ITableViewController Controller => _view;
		Cell _restoreFocus;

		public TableViewModelRenderer(Context context, AListView listView, TableView view) : base(context)
		{
			_view = view;
			Context = context;

			Controller.ModelChanged += (sender, args) => NotifyDataSetChanged();

			listView.OnItemClickListener = this;
			listView.OnItemLongClickListener = this;
		}

		public override int Count
		{
			get
			{
				var count = 0;

				//Get each adapter's count + 1 for the header
				ITableModel model = Controller.Model;
				int section = model.GetSectionCount();
				for (var i = 0; i < section; i++)
					count += model.GetRowCount(i) + 1;

				return count;
			}
		}

		public override object this[int position]
		{
			get
			{
				bool isHeader, nextIsHeader;
				Cell cell = GetCellForPosition(position, out isHeader, out nextIsHeader);
				return cell;
			}
		}

		public override int ViewTypeCount
		{
			get
			{
				//The headers count as a view type too
				var viewTypeCount = 1;
				ITableModel model = Controller.Model;

				//Get each adapter's ViewTypeCount
				int section = model.GetSectionCount();
				for (var i = 0; i < section; i++)
					viewTypeCount += model.GetRowCount(i);

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
			object obj = this[position];
			if (obj == null)
				return new AView(Context);

			bool isHeader, nextIsHeader;
			Cell item = GetCellForPosition(position, out isHeader, out nextIsHeader);

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

			int sectionCount = model.GetSectionCount();
			for (var sectionIndex = 0; sectionIndex < sectionCount; sectionIndex++)
			{
				if (position == 0)
					return;

				int size = model.GetRowCount(sectionIndex) + 1;

				if (position < size)
				{
					model.RowSelected(sectionIndex, position - 1);
					return;
				}

				position -= size;
			}
		}

		Cell GetCellForPosition(int position, out bool isHeader, out bool nextIsHeader)
		{
			isHeader = false;
			nextIsHeader = false;

			ITableModel model = Controller.Model;
			int sectionCount = model.GetSectionCount();

			for (var sectionIndex = 0; sectionIndex < sectionCount; sectionIndex ++)
			{
				int size = model.GetRowCount(sectionIndex) + 1;

				if (position == 0)
				{
					isHeader = true;
					nextIsHeader = size == 0 && sectionIndex < sectionCount - 1;

					Cell header = model.GetHeaderCell(sectionIndex);

					Cell resultCell = null;
					if (header != null)
						resultCell = header;

					if (resultCell == null)
						resultCell = new TextCell { Text = model.GetSectionTitle(sectionIndex) };

					resultCell.Parent = _view;

					return resultCell;
				}

				if (position < size)
				{
					nextIsHeader = position == size - 1;
					return (Cell)model.GetItem(sectionIndex, position - 1);
				}

				position -= size;
			}

			return null;
		}
	}
}