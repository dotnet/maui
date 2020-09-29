using System;
using System.ComponentModel;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class TableModel : ITableModel
	{
		public virtual Cell GetCell(int section, int row)
		{
			object item = GetItem(section, row);
			var cell = item as Cell;
			if (cell != null)
				return cell;

			return new TextCell { Text = item.ToString() };
		}

		public virtual Cell GetHeaderCell(int section)
		{
			return null;
		}

		public abstract object GetItem(int section, int row);

		public abstract int GetRowCount(int section);

		public abstract int GetSectionCount();

		public virtual string[] GetSectionIndexTitles()
		{
			return null;
		}

		public virtual string GetSectionTitle(int section)
		{
			return null;
		}

		public virtual Color GetSectionTextColor(int section)
		{
			return Color.Default;
		}

		public event EventHandler<EventArg<object>> ItemLongPressed;

		public event EventHandler<EventArg<object>> ItemSelected;

		public void RowLongPressed(int section, int row)
		{
			RowLongPressed(GetItem(section, row));
		}

		public void RowLongPressed(object item)
		{
			if (ItemLongPressed != null)
				ItemLongPressed(this, new EventArg<object>(item));

			OnRowLongPressed(item);
		}

		public void RowSelected(int section, int row)
		{
			RowSelected(GetItem(section, row));
		}

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