#nullable disable
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/CellExtensions.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.CellExtensions']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class CellExtensions
	{
		const DynamicallyAccessedMemberTypes ItemTypeMembers = BindableProperty.DeclaringTypeMembers | BindableProperty.ReturnTypeMembers;

		public static bool GetIsGroupHeader<TView, [DynamicallyAccessedMembers(ItemTypeMembers)] TItem>(this TItem cell) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			return TemplatedItemsList<TView, TItem>.GetIsGroupHeader(cell);
		}

		public static void SetIsGroupHeader<TView, [DynamicallyAccessedMembers(ItemTypeMembers)] TItem>(this TItem cell, bool value) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			TemplatedItemsList<TView, TItem>.SetIsGroupHeader(cell, value);
		}

		public static TItem GetGroupHeaderContent<TView, [DynamicallyAccessedMembers(ItemTypeMembers)] TItem>(this TItem cell) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			var group = TemplatedItemsList<TView, TItem>.GetGroup(cell);
			return group.HeaderContent;
		}

		public static int GetIndex<TView, [DynamicallyAccessedMembers(ItemTypeMembers)] TItem>(this TItem cell) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			return TemplatedItemsList<TView, TItem>.GetIndex(cell);
		}

		public static ITemplatedItemsList<TItem> GetGroup<TView, [DynamicallyAccessedMembers(ItemTypeMembers)] TItem>(this TItem cell) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			return TemplatedItemsList<TView, TItem>.GetGroup(cell);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/CellExtensions.xml" path="//Member[@MemberName='GetPath']/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
		public static Tuple<int, int> GetPath(this Cell cell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
#pragma warning disable CS0618 // Type or member is obsolete
			return TableView.TableSectionModel.GetPath(cell);
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}
}
