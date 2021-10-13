using System;
using System.Windows.Input;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;
using Recipes.Models;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;

namespace Recipes.ViewModels
{
    [QueryProperty(nameof(HitId), nameof(HitId))]
    public class HitDetailViewModel : BaseViewModel
    {

        public Hit Hit { get; set; }

        string _hitId;
        string _recipeName;
        string _imageUrl;
        string _ingredients;
        string _recipeBody;
        FormattedString _recipeUrl;

		bool _recipeNameVisible;
		bool _imageUrlVisible;
		bool _ingredientsVisible;
		bool _recipeBodyVisible;
		bool _recipeUrlVisible;

		public ICommand TapCommand { get; }
        public Command AddItemCommand { get; }

        public HitDetailViewModel()
        {
            // Launcher.OpenAsync is provided by Microsoft.Maui.Essentials.
            TapCommand = new Command<string>(async (url) => await Launcher.OpenAsync(url));
            AddItemCommand = new Command(OnAddItem);

			RecipeNameVisible = true;
			ImageUrlVisible = true;
			IngredientsVisible = true;
			RecipeBodyVisible = true;
			RecipeUrlVisible = true;
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

		private void OnAddItem()
        {

            List<Ingredient> ingredientList = new List<Ingredient>();
            string[] ingredientStringList = Ingredients.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string ingredientString in ingredientStringList)
                ingredientList.Add(new Ingredient { IngredientItem = ingredientString });

            Item newItem = new Item()
            {
                Id = HitId,
                RecipeName = RecipeName,
                ImageUrl = ImageUrl,
                Ingredients = ingredientList,
                RecipeBody = RecipeBody,
                RecipeUrl = RecipeUrl
            };

            DataStore.AddItemAsync(newItem);
        }

        public string HitId
        {
            get
            {
                return _hitId;
            }
            set
            {
                _hitId = value;
                LoadHitId(value);
            }
        }

        public void LoadHitId(string hitId)
        {
            try
            {
                Hit = AppShell.Data.Hits.FirstOrDefault(h => h.Id == int.Parse(hitId));
                OnPropertyChanged(nameof(Hit));
                LoadHitDetails(Hit);
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Hit");
            }
        }

        public void LoadHitDetails(Hit hit)
		{
			var emptyFormattedString = new FormattedString();
			emptyFormattedString.Spans.Add(new Span { Text = "" });

			Title = Hit.Recipe.RecipeName;

            RecipeName = Hit.Recipe.RecipeName;
            ImageUrl = Hit.Recipe.ImageUrl;
			//RecipeBody = Hit.Recipe;
			Ingredients = String.Join(Environment.NewLine, Hit.Recipe.Ingredients);
			var recipeBodyFormattedString = new FormattedString();
            recipeBodyFormattedString.Spans.Add(new Span { Text = "Click " });

            var recipeUrlFormattedString = new Span { Text = "here", TextColor = Colors.Blue, TextDecorations = TextDecorations.Underline };
            recipeUrlFormattedString.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = TapCommand,
                CommandParameter = Hit.Recipe.RecipeUrl
            });
            recipeBodyFormattedString.Spans.Add(recipeUrlFormattedString);

            recipeBodyFormattedString.Spans.Add(new Span { Text = " to view full recipe." });
            RecipeUrl = recipeBodyFormattedString;
			RecipeNameVisible = !String.IsNullOrEmpty(RecipeName);
			ImageUrlVisible = !String.IsNullOrEmpty(ImageUrl);
			IngredientsVisible = Hit.Recipe.Ingredients.Length > 0;
			RecipeBodyVisible = !String.IsNullOrEmpty(RecipeBody);
			RecipeUrlVisible = !(RecipeUrl == null || FormattedString.Equals(RecipeUrl, emptyFormattedString));
		}
    }
}