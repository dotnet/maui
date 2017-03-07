using System;
using System.ComponentModel;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Internals
{
	public abstract class TableModel: ITableModel
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual Cell GetCell(int section, int row)
		{
			object item = GetItem(section, row);
			var cell = item as Cell;
			if (cell != null)
				return cell;

			return new TextCell { Text = item.ToString() };
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual Cell GetHeaderCell(int section)
		{
			return null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public abstract object GetItem(int section, int row);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public abstract int GetRowCount(int section);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public abstract int GetSectionCount();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual string[] GetSectionIndexTitles()
		{
			return null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual string GetSectionTitle(int section)
		{
			return null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<EventArg<object>> ItemLongPressed;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<EventArg<object>> ItemSelected;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void RowLongPressed(int section, int row)
		{
			RowLongPressed(GetItem(section, row));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void RowLongPressed(object item)
		{
			if (ItemLongPressed != null)
				ItemLongPressed(this, new EventArg<object>(item));

			OnRowLongPressed(item);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void RowSelected(int section, int row)
		{
			RowSelected(GetItem(section, row));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void RowSelected(object item)
		{
			if (ItemSelected != null)
				ItemSelected(this, new EventArg<object>(item));

			OnRowSelected(item);
		}

		protected virtual void OnRowLongPressed(object item)
		{
		}

		protected virtual void OnRowSelected(object item)
		{
		}
	}
}