using System;
using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class CellExtensions
	{
		public static bool GetIsGroupHeader<TView, TItem>(this TItem cell) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			return TemplatedItemsList<TView, TItem>.GetIsGroupHeader(cell);
		}

		public static void SetIsGroupHeader<TView, TItem>(this TItem cell, bool value) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			TemplatedItemsList<TView, TItem>.SetIsGroupHeader(cell, value);
		}

		public static TItem GetGroupHeaderContent<TView, TItem>(this TItem cell) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			var group = TemplatedItemsList<TView, TItem>.GetGroup(cell);
			return group.HeaderContent;
		}

		public static int GetIndex<TView, TItem>(this TItem cell) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			return TemplatedItemsList<TView, TItem>.GetIndex(cell);
		}

		public static ITemplatedItemsList<TItem> GetGroup<TView, TItem>(this TItem cell) where TView : BindableObject, ITemplatedItemsView<TItem> where TItem : BindableObject
		{
			return TemplatedItemsList<TView, TItem>.GetGroup(cell);
		}

		public static Tuple<int, int> GetPath(this Cell cell)
		{
			return TableView.TableSectionModel.GetPath(cell);
		}
	}
}