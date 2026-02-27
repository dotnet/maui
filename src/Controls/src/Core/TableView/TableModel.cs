#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>
	/// Abstract base class that provides the data model for a <see cref="TableView"/>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class TableModel : ITableModel
	{
		/// <summary>
		/// Returns the cell for the specified section and row.
		/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
		public virtual Cell GetCell(int section, int row)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			object item = GetItem(section, row);
#pragma warning disable CS0618 // Type or member is obsolete
			var cell = item as Cell;
#pragma warning restore CS0618 // Type or member is obsolete
			if (cell != null)
				return cell;

#pragma warning disable CS0618 // Type or member is obsolete
			return new TextCell { Text = item.ToString() };
#pragma warning restore CS0618 // Type or member is obsolete
		}

		/// <summary>
		/// Returns the header cell for the specified section.
		/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
		public virtual Cell GetHeaderCell(int section)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return null;
		}

		/// <summary>
		/// Returns the data item for the specified section and row.
		/// </summary>
		public abstract object GetItem(int section, int row);

		/// <summary>
		/// Returns the number of rows in the specified section.
		/// </summary>
		public abstract int GetRowCount(int section);

		/// <summary>
		/// Returns the total number of sections in the table.
		/// </summary>
		public abstract int GetSectionCount();

		/// <summary>
		/// Returns an array of index titles for quick navigation between sections.
		/// </summary>
		public virtual string[] GetSectionIndexTitles()
		{
			return null;
		}

		/// <summary>
		/// Returns the title for the specified section.
		/// </summary>
		public virtual string GetSectionTitle(int section)
		{
			return null;
		}

		/// <summary>
		/// Returns the text color for the specified section header.
		/// </summary>
		public virtual Color GetSectionTextColor(int section)
		{
			return null;
		}

		public event EventHandler<EventArg<object>> ItemLongPressed;

		public event EventHandler<EventArg<object>> ItemSelected;

		/// <summary>
		/// Invokes the long-press event for the item at the specified section and row.
		/// </summary>
		public void RowLongPressed(int section, int row)
		{
			RowLongPressed(GetItem(section, row));
		}

		/// <summary>
		/// Invokes the long-press event for the specified item.
		/// </summary>
		public void RowLongPressed(object item)
		{
			if (ItemLongPressed != null)
				ItemLongPressed(this, new EventArg<object>(item));

			OnRowLongPressed(item);
		}

		/// <summary>
		/// Invokes the selection event for the item at the specified section and row.
		/// </summary>
		public void RowSelected(int section, int row)
		{
			RowSelected(GetItem(section, row));
		}

		/// <summary>
		/// Invokes the selection event for the specified item.
		/// </summary>
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