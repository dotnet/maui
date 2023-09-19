using Recipes.Models;

namespace Recipes.ViewModels
{
    public class NewRecipeViewModel : BaseViewModel
    {
        string _recipeName;
		string _imageUrl;
        string _ingredients;
        string _recipeBody;
        string _recipeUrl;
        float _recipeRating;
        string _recipeReview;

        public NewRecipeViewModel()
        {
            SaveCommand = new Command(OnSave, ValidateSave);
            CancelCommand = new Command(OnCancel);
            PropertyChanged +=
                (_, __) => SaveCommand.ChangeCanExecute();
        }

        private bool ValidateSave()
        {
            return !string.IsNullOrWhiteSpace(_recipeName);
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

        public string RecipeUrl
        {
            get => _recipeUrl;
            set => SetProperty(ref _recipeUrl, value);
        }

        public float RecipeRating
        {
            get => _recipeRating;
            set => SetProperty(ref _recipeRating, value);
        }

        public string RecipeReview
        {
            get => _recipeReview;
            set => SetProperty(ref _recipeReview, value);
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
            string[] ingredientStringList = (Ingredients ?? string.Empty).Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string ingredientString in ingredientStringList)
				ingredientList.Add(new Ingredient { IngredientItem = ingredientString });

            Item NewRecipe = new Item()
            {
                Id = Guid.NewGuid().ToString(),
                RecipeName = RecipeName,
                ImageUrl = ImageUrl,
                Ingredients = ingredientList,
                RecipeBody = RecipeBody,
                RecipeUrl = RecipeUrl,
                RecipeRating = RecipeRating,
                RecipeReview = RecipeReview
            };

            await DataStore.AddItemAsync(NewRecipe);

            SemanticScreenReader.Announce(RecipeName + " recipe added.");

            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }
    }
}
