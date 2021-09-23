using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Recipes.Views;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Recipes.Models;

namespace Recipes.ViewModels
{
    [QueryProperty(nameof(ItemId), nameof(ItemId))]
    public class ItemDetailViewModel : BaseViewModel
    {
        public Command EditItemCommand { get; }

        string _itemId;
        string _recipeName;
        string _imageUrl;

        IList<Ingredient> source;
        public ObservableCollection<Ingredient> _ingredientCheckList;

        bool _ingredientChecked;

        string _recipeBody;
        FormattedString _recipeUrl;

        bool _recipeNameVisible;
        bool _imageUrlVisible;
        bool _ingredientsVisible;
        bool _recipeBodyVisible;
        bool _recipeUrlVisible;

        public string Id { get; set; }

        public ItemDetailViewModel()
        {
            EditItemCommand = new Command(OnEditItem);

            RecipeNameVisible = true;
            ImageUrlVisible = true;
            IngredientsVisible = true;
            RecipeBodyVisible = true;
            RecipeUrlVisible = true;
        }

		public string ItemId
        {
            get
            {
                return _itemId;
            }
            set
            {
                if (value == null)
                    return;

                _itemId = value;
                LoadItemId(value);
            }
        }

        public string RecipeName
        {
            get => _recipeName;
            set => SetProperty(ref _recipeName, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        public bool IngredientChecked
        {
            get => _ingredientChecked;
            set => SetProperty(ref _ingredientChecked, value);
        }

		public ObservableCollection<Ingredient> IngredientCheckList
		{
			get => _ingredientCheckList;
			set => SetProperty(ref _ingredientCheckList, value);
		}

		public string RecipeBody
        {
            get => _recipeBody;
            set => SetProperty(ref _recipeBody, value);
        }

        public FormattedString RecipeUrl
        {
            get => _recipeUrl;
            set => SetProperty(ref _recipeUrl, value);
        }

        public bool RecipeNameVisible
        {
            get => _recipeNameVisible;
            set => SetProperty(ref _recipeNameVisible, value);
        }

        public bool ImageUrlVisible
        {
            get => _imageUrlVisible;
            set => SetProperty(ref _imageUrlVisible, value);
        }

        public bool IngredientsVisible
        {
            get => _ingredientsVisible;
            set => SetProperty(ref _ingredientsVisible, value);
        }

        public bool RecipeBodyVisible
        {
            get => _recipeBodyVisible;
            set => SetProperty(ref _recipeBodyVisible, value);
        }

        public bool RecipeUrlVisible
        {
            get => _recipeUrlVisible;
            set => SetProperty(ref _recipeUrlVisible, value);
        }

        public async void LoadItemId(string itemId)
        {
            try
            {
                var item = await DataStore.GetItemAsync(itemId);
                Id = item.Id;
                RecipeName = item.RecipeName;
                ImageUrl = item.ImageUrl;
                RecipeBody = item.RecipeBody;
                RecipeUrl = item.RecipeUrl;

                source = item.Ingredients;
                IngredientCheckList = new ObservableCollection<Ingredient>(source);

                foreach (Ingredient ingredient in IngredientCheckList)
                    IngredientChecked = ingredient.IngredientChecked;

                Title = RecipeName;

                var emptyFormattedString = new FormattedString();
                emptyFormattedString.Spans.Add(new Span { Text = "" });

                RecipeNameVisible = !String.IsNullOrEmpty(RecipeName);
                ImageUrlVisible = !String.IsNullOrEmpty(ImageUrl);
                IngredientsVisible = IngredientCheckList.Count > 0;
                RecipeBodyVisible = !String.IsNullOrEmpty(RecipeBody);
                RecipeUrlVisible = !(RecipeUrl == null || FormattedString.Equals(RecipeUrl, emptyFormattedString));
                
            }
            catch (Exception) 
            {
                Debug.WriteLine("Failed to Load Item");
            }
        }

        private async void OnEditItem(object obj)
        {
            await Shell.Current.GoToAsync($"{nameof(EditItemPage)}?{nameof(EditItemViewModel.Id)}={_itemId}");
        }

        public void OnAppearing()
        {
            IsBusy = true;
            LoadItemId(_itemId);
        }
    }
}
