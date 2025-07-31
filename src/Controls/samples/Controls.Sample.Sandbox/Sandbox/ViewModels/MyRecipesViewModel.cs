using System.Collections.ObjectModel;
using System.Diagnostics;
using Recipes.Models;
using Recipes.Views;

namespace Recipes.ViewModels
{
    public class MyRecipesViewModel : BaseViewModel
    {
        public Item _selectedItem;

		public ObservableCollection<Item> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command NewRecipeCommand { get; }
        public Command<Item> ItemTapped { get; }

        public MyRecipesViewModel()
        {
            Title = "Recipes";
            Items = new ObservableCollection<Item>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            ItemTapped = new Command<Item>(OnItemSelected);
            NewRecipeCommand = new Command(OnNewRecipe);
		}

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
		}

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;

            LoadItemsCommand.Execute(null);
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        private async void OnNewRecipe(object obj)
        {
            await Shell.Current.GoToAsync(nameof(NewRecipePage));
        }

        async void OnItemSelected(Item item)
        {
            if (item == null)
                return;

            // This will push the RecipeDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(RecipeDetailPage)}?{nameof(RecipeDetailViewModel.ItemId)}={item.Id}");
        }
    }
}