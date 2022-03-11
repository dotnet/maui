using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/CellExtensions.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.CellExtensions']/Docs" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class CellExtensions
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/CellExtensions.xml" path="//Member[@MemberName='GetIsGroupHeader']/Docs" />
		public static bool GetIsGroupHeader<TView, TItem>(this TItem cell) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			return TemplatedItemsList<TView, TItem>.GetIsGroupHeader(cell);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/CellExtensions.xml" path="//Member[@MemberName='SetIsGroupHeader']/Docs" />
		public static void SetIsGroupHeader<TView, TItem>(this TItem cell, bool value) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			TemplatedItemsList<TView, TItem>.SetIsGroupHeader(cell, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/CellExtensions.xml" path="//Member[@MemberName='GetGroupHeaderContent']/Docs" />
		public static TItem GetGroupHeaderContent<TView, TItem>(this TItem cell) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			var group = TemplatedItemsList<TView, TItem>.GetGroup(cell);
			return group.HeaderContent;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/CellExtensions.xml" path="//Member[@MemberName='GetIndex']/Docs" />
		public static int GetIndex<TView, TItem>(this TItem cell) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			return TemplatedItemsList<TView, TItem>.GetIndex(cell);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/CellExtensions.xml" path="//Member[@MemberName='GetGroup']/Docs" />
		public static ITemplatedItemsList<TItem> GetGroup<TView, TItem>(this TItem cell) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			return TemplatedItemsList<TView, TItem>.GetGroup(cell);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/CellExtensions.xml" path="//Member[@MemberName='GetPath']/Docs" />
		public static Tuple<int, int> GetPath(this Cell cell)
		{
			return TableView.TableSectionModel.GetPath(cell);
		}
	}
}
