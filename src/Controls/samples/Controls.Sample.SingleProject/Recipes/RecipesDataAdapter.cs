using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Recipes.ViewModels;

namespace Recipes
{
	public class RecipesDataAdapter : IVirtualListViewAdapter
	{
		private RecipeSearchViewModel _recipeSearchViewModel;
		private ItemsViewModel _itemsViewModel;

		public RecipesDataAdapter(RecipeSearchViewModel recipeSearchViewModel)
		{
			_recipeSearchViewModel = recipeSearchViewModel;
		}

		public RecipesDataAdapter(ItemsViewModel itemsViewModel)
		{
			_itemsViewModel = itemsViewModel;
		}

		public int Sections => _recipeSearchViewModel.RecipeData == null ? 0 : 1;

		public object Item(int sectionIndex, int itemIndex)
		{
			return _recipeSearchViewModel.RecipeData?.Hits[itemIndex] ?? null;
		}

		public int ItemsForSection(int sectionIndex)
		{
			return _recipeSearchViewModel.RecipeData?.Hits.Length ?? 0;
		}

		public object Section(int sectionIndex)
		{
			throw new NotImplementedException();
		}
	}
}
