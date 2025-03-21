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

		/// <include file="../../docs/Microsoft.Maui.Controls.Internals/TableModel.xml" path="//Member[@MemberName='GetHeaderCell']/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
		public virtual Cell GetHeaderCell(int section)
#pragma warning restore CS0618 // Type or member is obsolete
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