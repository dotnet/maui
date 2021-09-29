using System;
using Microsoft.Maui.Controls;
using Recipes.Models;
using System.Collections.Generic;

namespace Recipes.ViewModels
{
    public class NewItemViewModel : BaseViewModel
    {
        string _recipeName;
        string _imageUrl;
        string _ingredients;
        string _recipeBody;
        FormattedString _recipeUrl;

        public NewItemViewModel()
        {
            SaveCommand = new Command(OnSave, ValidateSave);
            CancelCommand = new Command(OnCancel);
            this.PropertyChanged +=
                (_, __) => SaveCommand.ChangeCanExecute();
        }

        private bool ValidateSave()
        {
            return !String.IsNullOrWhiteSpace(_recipeName);
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

        public string Ingredients
        {
            get => _ingredients;
            set => SetProperty(ref _ingredients, value);
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

        public Command SaveCommand { get; }
        public Command CancelCommand { get; }

        private async void OnCancel()
        {
            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }

        private async void OnSave()
        {
            List<Ingredient> ingredientList = new List<Ingredient>();
            string[] ingredientStringList = Ingredients.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string ingredientString in ingredientStringList)
				ingredientList.Add(new Ingredient { IngredientItem = ingredientString });

            Item newItem = new Item()
            {
                Id = Guid.NewGuid().ToString(),
                RecipeName = RecipeName,
                ImageUrl = ImageUrl,
                Ingredients = ingredientList,
                RecipeBody = RecipeBody,
                RecipeUrl = RecipeUrl
            };

            await DataStore.AddItemAsync(newItem);

            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }
    }
}
