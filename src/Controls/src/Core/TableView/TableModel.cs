#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TableModel.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.TableModel']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class TableModel : ITableModel
	{
		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TableModel.xml" path="//Member[@MemberName='GetCell']/Docs/*" />
		public virtual Cell GetCell(int section, int row)
		{
			object item = GetItem(section, row);
			var cell = item as Cell;
			if (cell != null)
				return cell;

			return new TextCell { Text = item.ToString() };
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TableModel.xml" path="//Member[@MemberName='GetHeaderCell']/Docs/*" />
		public virtual Cell GetHeaderCell(int section)
		{
			return null;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TableModel.xml" path="//Member[@MemberName='GetItem']/Docs/*" />
		public abstract object GetItem(int section, int row);

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TableModel.xml" path="//Member[@MemberName='GetRowCount']/Docs/*" />
		public abstract int GetRowCount(int section);

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TableModel.xml" path="//Member[@MemberName='GetSectionCount']/Docs/*" />
		public abstract int GetSectionCount();

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TableModel.xml" path="//Member[@MemberName='GetSectionIndexTitles']/Docs/*" />
		public virtual string[] GetSectionIndexTitles()
		{
			return null;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TableModel.xml" path="//Member[@MemberName='GetSectionTitle']/Docs/*" />
		public virtual string GetSectionTitle(int section)
		{
			return null;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TableModel.xml" path="//Member[@MemberName='GetSectionTextColor']/Docs/*" />
		public virtual Color GetSectionTextColor(int section)
		{
			return null;
		}

		public event EventHandler<EventArg<object>> ItemLongPressed;

		public event EventHandler<EventArg<object>> ItemSelected;

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TableModel.xml" path="//Member[@MemberName='RowLongPressed'][2]/Docs/*" />
		public void RowLongPressed(int section, int row)
		{
			RowLongPressed(GetItem(section, row));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TableModel.xml" path="//Member[@MemberName='RowLongPressed'][1]/Docs/*" />
		public void RowLongPressed(object item)
		{
			if (ItemLongPressed != null)
				ItemLongPressed(this, new EventArg<object>(item));

			OnRowLongPressed(item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TableModel.xml" path="//Member[@MemberName='RowSelected'][2]/Docs/*" />
		public void RowSelected(int section, int row)
		{
			RowSelected(GetItem(section, row));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TableModel.xml" path="//Member[@MemberName='RowSelected'][1]/Docs/*" />
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