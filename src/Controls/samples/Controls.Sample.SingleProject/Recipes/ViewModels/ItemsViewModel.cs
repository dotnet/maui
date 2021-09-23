using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Recipes.Models;
using Recipes.Views;

namespace Recipes.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        public Item _selectedItem;

		public MyRecipesAdapter Adapter { get; private set; }

		public ObservableCollection<Item> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command NewItemCommand { get; }
        public Command<Item> ItemTapped { get; }

        public ItemsViewModel()
        {
            Title = "Recipes";
            Items = new ObservableCollection<Item>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            ItemTapped = new Command<Item>(OnItemSelected);
            NewItemCommand = new Command(OnNewItem);
			Adapter = new MyRecipesAdapter(this);
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

			MessagingCenter.Send(this, "RemoveRecipeFromVirtualListView");
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

        private async void OnNewItem(object obj)
        {
            await Shell.Current.GoToAsync(nameof(NewItemPage));
        }

        async void OnItemSelected(Item item)
        {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
        }
    }
}