using System.Windows.Input;
using System.Diagnostics;
using Recipes.Models;
using System.Collections.ObjectModel;

namespace Recipes.ViewModels
{
	[QueryProperty(nameof(HitId), nameof(HitId))]
    public class SearchResultDetailViewModel : BaseViewModel
    {
        public Hit Hit { get; set; }

        string _hitId;
        string _recipeName;
        string _imageUrl;
		string[] _ingredients;
        string _recipeBody;
        string _recipeUrl;
		public ObservableCollection<Ingredient> _ingredientCheckList;

		bool _recipeNameVisible;
		bool _imageUrlVisible;
		bool _ingredientsVisible;
		bool _recipeBodyVisible;
		bool _recipeUrlVisible;

		public ICommand TapCommand { get; }
		public Command AddItemCommand { get; }
		public Command RemoveItemCommand { get; }

		public SearchResultDetailViewModel()
        {
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

        public string[] Ingredients
        {
            get => _ingredients;
            set => SetProperty(ref _ingredients, value);
        }

        public string RecipeBody
        {
            get => _recipeBody;
            set => SetProperty(ref _recipeBody, value);
        }

        public string RecipeUrl
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

		public ObservableCollection<Ingredient> IngredientCheckList
		{
			get => _ingredientCheckList;
			set => SetProperty(ref _ingredientCheckList, value);
		}

		async void OnAddItem()
        {
            List<Ingredient> ingredientList = new List<Ingredient>();
            foreach (string ingredientString in Ingredients)
                ingredientList.Add(new Ingredient { IngredientItem = ingredientString });

            Item NewRecipe = new Item()
            {
                Id = HitId,
                RecipeName = RecipeName,
                ImageUrl = ImageUrl,
                Ingredients = ingredientList,
                RecipeBody = RecipeBody,
                RecipeUrl = RecipeUrl
            };

            await DataStore.AddItemAsync(NewRecipe);
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
                LoadSearchResultDetails();
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Hit");
            }
        }

        public void LoadSearchResultDetails()
		{
			var emptyFormattedString = new FormattedString();
			emptyFormattedString.Spans.Add(new Span { Text = "" });

			Title = Hit.Recipe.RecipeName;

            RecipeName = Hit.Recipe.RecipeName;
            ImageUrl = Hit.Recipe.ImageUrl;
			Ingredients = Hit.Recipe.Ingredients;

			RecipeUrl = Hit.Recipe.RecipeUrl;
			RecipeNameVisible = !string.IsNullOrEmpty(RecipeName);
			ImageUrlVisible = !string.IsNullOrEmpty(ImageUrl);
			IngredientsVisible = Hit.Recipe.Ingredients.Length > 0;
			RecipeBodyVisible = !string.IsNullOrEmpty(RecipeBody);
			RecipeUrlVisible = !(RecipeUrl == null || Equals(RecipeUrl, emptyFormattedString));
		}
    }
}
