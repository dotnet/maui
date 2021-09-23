using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Recipes.ViewModels;

namespace Recipes
{
	public class MyRecipesAdapter : IVirtualListViewAdapter
	{
		private ItemsViewModel _itemsViewModel;

		public MyRecipesAdapter(ItemsViewModel itemsViewModel)
		{
			_itemsViewModel = itemsViewModel;
		}

		public int Sections => _itemsViewModel.Items == null ? 0 : 1;

		public object Item(int sectionIndex, int itemIndex)
		{
			if (itemIndex >= _itemsViewModel.Items.Count)
				return new StackLayout();

			var item = _itemsViewModel.Items[itemIndex];
			return item ?? throw new NullReferenceException();
		}

		public int ItemsForSection(int sectionIndex)
		{
			return _itemsViewModel.Items.Count;
		}

		public object Section(int sectionIndex)
		{
			throw new NotImplementedException();
		}
	}
}
