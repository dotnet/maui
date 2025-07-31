using System.Diagnostics;
using Recipes.Models;

namespace Recipes.ViewModels
{
    [QueryProperty(nameof(Id), nameof(Id))]
    public class EditRecipeViewModel : BaseViewModel
    {
        string _id;
        string _recipeName;
        string _ingredients;
        string _imageUrl;
        string _recipeBody;
        string _recipeUrl;
        float _recipeRating;
        string _recipeReview;
        
        public EditRecipeViewModel()
        {
            UpdateCommand = new Command(OnUpdate, ValidateUpdate);
            DeleteCommand = new Command(OnDelete);
            CancelCommand = new Command(OnCancel);
            PropertyChanged +=
                (_, __) => UpdateCommand.ChangeCanExecute();
            PropertyChanged +=
                (_, __) => DeleteCommand.ChangeCanExecute();
        }

        private bool ValidateUpdate()
        {
            return !string.IsNullOrWhiteSpace(_recipeName);
        }

        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                LoadItemId(value);
            }
        }

        public string RecipeName
        {
            get => _recipeName;
            set => SetProperty(ref _recipeName, value);
        }

        public string Ingredients
        {
            get => _ingredients;
            set => SetProperty(ref _ingredients, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
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

        public async void LoadItemId(string itemId)
        {
            try
            {
                var item = await DataStore.GetItemAsync(itemId);
                RecipeName = item.RecipeName;

                string ingredients = "";
                foreach (Ingredient ingredient in item.Ingredients)
                    ingredients += ingredient.IngredientItem + Environment.NewLine;
                Ingredients = ingredients;

                ImageUrl = item.ImageUrl;
                RecipeBody = item.RecipeBody;
                RecipeUrl = item.RecipeUrl;
                RecipeRating = item.RecipeRating;
                RecipeReview = item.RecipeReview;
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Item");
            }
        }

        public Command UpdateCommand { get; }
        public Command DeleteCommand { get; }
        public Command CancelCommand { get; }

        private async void OnCancel()
        {
            await Shell.Current.GoToAsync("..");
        }

        private async void OnDelete()
        {
            await DataStore.DeleteItemAsync(_id);

            SemanticScreenReader.Announce(RecipeName + " recipe deleted.");

            await Shell.Current.GoToAsync("../..");
        }

        private async void OnUpdate()
        {
            List<Ingredient> ingredientList = new List<Ingredient>();
            string[] ingredientStringList = Ingredients.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string ingredientString in ingredientStringList)
                ingredientList.Add(new Ingredient { IngredientItem = ingredientString });

            Item NewRecipe = new Item()
            {
                Id = _id,
                RecipeName = RecipeName,
                Ingredients = ingredientList,
                ImageUrl = ImageUrl,
                RecipeBody = RecipeBody,
                RecipeUrl = RecipeUrl,
                RecipeRating = RecipeRating,
                RecipeReview = RecipeReview
            };

            await DataStore.UpdateItemAsync(NewRecipe);

            SemanticScreenReader.Announce(RecipeName + " recipe updated.");

            await Shell.Current.GoToAsync("..");
        }
    }
}
