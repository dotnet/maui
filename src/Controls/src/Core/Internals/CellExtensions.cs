using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/CellExtensions.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.CellExtensions']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class CellExtensions
	{
		public static bool GetIsGroupHeader<TView, [DynamicallyAccessedMembers(BindableProperty.DeclaringTypeMembers)] TItem>(this TItem cell) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			return TemplatedItemsList<TView, TItem>.GetIsGroupHeader(cell);
		}

		public static void SetIsGroupHeader<TView, [DynamicallyAccessedMembers(BindableProperty.DeclaringTypeMembers)] TItem>(this TItem cell, bool value) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			TemplatedItemsList<TView, TItem>.SetIsGroupHeader(cell, value);
		}

		public static TItem GetGroupHeaderContent<TView, [DynamicallyAccessedMembers(BindableProperty.DeclaringTypeMembers)] TItem>(this TItem cell) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			var group = TemplatedItemsList<TView, TItem>.GetGroup(cell);
			return group.HeaderContent;
		}

		public static int GetIndex<TView, [DynamicallyAccessedMembers(BindableProperty.DeclaringTypeMembers)] TItem>(this TItem cell) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			return TemplatedItemsList<TView, TItem>.GetIndex(cell);
		}

		public static ITemplatedItemsList<TItem> GetGroup<TView, [DynamicallyAccessedMembers(BindableProperty.DeclaringTypeMembers)] TItem>(this TItem cell) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			return TemplatedItemsList<TView, TItem>.GetGroup(cell);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/CellExtensions.xml" path="//Member[@MemberName='GetPath']/Docs/*" />
		public static Tuple<int, int> GetPath(this Cell cell)
		{
			return TableView.TableSectionModel.GetPath(cell);
		}
	}
}
