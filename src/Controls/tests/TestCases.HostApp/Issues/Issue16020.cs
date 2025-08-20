using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Maui.Controls.Sample;

namespace Controls.TestCases.Sample.Issues;

[Issue(IssueTracker.Github, "16020", "Newly created recipes cannot be deleted", PlatformAffected.iOS)]
public class Issue16020 : Shell
{
	public Issue16020()
	{
		DependencyService.Register<IDataStore<Item>, MockDataStore>();
		Routing.RegisterRoute(nameof(RecipeDetailPage), typeof(RecipeDetailPage));
		Routing.RegisterRoute(nameof(EditRecipePage), typeof(EditRecipePage));
		Routing.RegisterRoute(nameof(NewRecipePage), typeof(NewRecipePage));

		var tabBar = new TabBar();
		var searchTab = new ShellContent
		{
			Title = "Recipe Search",
			Route = "StartingPage",
			ContentTemplate = new DataTemplate(typeof(StartingPage))
		};
		var recipesTab = new ShellContent
		{
			Title = "My Recipes",
			Route = "MyRecipesPage",
			ContentTemplate = new DataTemplate(typeof(MyRecipesPage))
		};
		tabBar.Items.Add(searchTab);
		tabBar.Items.Add(recipesTab);
		Items.Add(tabBar);
	}
}

public class StartingPage : ContentPage
{
	public StartingPage()
	{
		Title = "Recipe Search";
		Content = new StackLayout
		{
			Children =
			{
				new Label { Text = "Welcome to the Recipe Search page!" }
			}
		};
	}
}

public class MyRecipesPage : ContentPage
{
	MyRecipesViewModel _viewModel;
	CarouselView vMyRecipesListView;
	Label _label;

	public MyRecipesPage()
	{
		var stackLayout = new StackLayout
		{
			Margin = 20,
			Children =
			{
				new Label
				{
					Text = "Your recipes",
					FontSize = 30,
					FontAttributes = FontAttributes.Bold,
				}
			}
		};
		var button = new Button();
		button.Text = "Go To Last Index";
		button.AutomationId = "GoToLastIndexButton";
		button.Clicked += (sender, e) =>
		{
			if (_viewModel.Items.Count > 0)
			{
				vMyRecipesListView.ScrollTo(_viewModel.Items.Count - 1, position: ScrollToPosition.MakeVisible);
			}
		};
		_label = new Label
		{
			AutomationId = "CarouselViewCountLabel",
			FontSize = 20,
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(0, 10, 0, 10)
		};
		_viewModel = new MyRecipesViewModel();
		BindingContext = _viewModel;

		vMyRecipesListView = new CarouselView2
		{
			AutomationId = "MyCarouselView",
			WidthRequest = 350,
			HeightRequest = 570,
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill,
			Loop = false
		};
		vMyRecipesListView.SetBinding(ItemsView.ItemsSourceProperty, "Items");

		vMyRecipesListView.ItemTemplate = new DataTemplate(() =>
		{
			var grid = new Grid
			{
				Padding = new Thickness(0, 0, 10, 0),
				WidthRequest = 340,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill
			};
			var tapGesture = new TapGestureRecognizer
			{
				NumberOfTapsRequired = 1
			};
			tapGesture.Tapped += MyRecipesPage_Tapped;
			grid.GestureRecognizers.Add(tapGesture);

			var recipeNameLabel = new Label
			{
				Padding = 10,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				LineBreakMode = LineBreakMode.WordWrap,
				MaxLines = 3,
				FontSize = 16
			};
			recipeNameLabel.SetBinding(Label.TextProperty, "RecipeName");
			grid.Children.Add(recipeNameLabel);

			return grid;
		});
		stackLayout.Children.Add(button);
		stackLayout.Children.Add(_label);
		stackLayout.Children.Add(vMyRecipesListView);
		_label.SetBinding(Label.TextProperty, new Binding("Items.Count", stringFormat: "Items: {0}"));
		
		Content = stackLayout;
	}
	async void MyRecipesPage_Tapped(object sender, EventArgs e)
	{
		if (sender is Grid grid && grid.BindingContext is Item item)
		{
			await Shell.Current.GoToAsync($"{nameof(RecipeDetailPage)}?{nameof(RecipeDetailViewModel.ItemId)}={item.Id}");
		}
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_viewModel.OnAppearing();
		_label.Text = $"Items Count: {_viewModel.Items.Count}";
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
	}
}
public partial class RecipeDetailPage : ContentPage
{
	public RecipeDetailPage()
	{
		var viewModel = new RecipeDetailViewModel();
		BindingContext = viewModel;

		var stackLayout = new StackLayout
		{
			Padding = 20,
			Children =
			{
				new Label
				{
					Text = "Recipe Name:",
					FontAttributes = FontAttributes.Bold,
					Margin = new Thickness(0, 20, 0, 5)
				},
				new Button
				{
					Text = "Edit Recipe",
					AutomationId = "EditRecipeButton",
					Command = viewModel.EditRecipeCommand,
					Margin = new Thickness(0, 0, 0, 20)
				},
				
			}
		};
		var recipeNameLabel = stackLayout.Children.OfType<Label>().FirstOrDefault(l => l.AutomationId == "RecipeNameLabel");

		recipeNameLabel?.SetBinding(Label.TextProperty, nameof(RecipeDetailViewModel.RecipeName));

		Content = stackLayout;
	}
}

public class EditRecipePage : ContentPage
{
	public EditRecipePage()
	{
		var viewModel = new EditRecipeViewModel();
		BindingContext = viewModel;

		var stackLayout = new StackLayout
		{
			Padding = 20,
			Children =
			{
				new Button
				{
					Text = "Delete Recipe",
					AutomationId = "DeleteRecipeButton",
					BackgroundColor = Colors.Red,
					TextColor = Colors.White,
					Margin = new Thickness(0, 30, 0, 0)
				}
			}
		};
		var recipeNameLabel = stackLayout.Children.OfType<Label>().FirstOrDefault(l => l.AutomationId == "RecipeNameLabel");
		var recipeRatingLabel = stackLayout.Children.OfType<Label>().FirstOrDefault(l => l.AutomationId == "RecipeRatingLabel");
		var deleteButton = stackLayout.Children.OfType<Button>().FirstOrDefault(b => b.AutomationId == "DeleteRecipeButton");

		recipeNameLabel?.SetBinding(Label.TextProperty, nameof(EditRecipeViewModel.RecipeName));
		deleteButton?.SetBinding(Button.CommandProperty, nameof(EditRecipeViewModel.DeleteCommand));

		Content = stackLayout;
	}
}

public class MyRecipesViewModel : BaseViewModel
{
	public ObservableCollection<Item> Items { get; }
	public Command NewRecipeCommand { get; }
	public Command<Item> ItemTapped { get; }
	public MyRecipesViewModel()
	{
		Title = "Recipes";
		Items = new ObservableCollection<Item>();
		ItemTapped = new Command<Item>(OnItemSelected);
		NewRecipeCommand = new Command(OnNewRecipe);
	}

	async void ExecuteLoadItemsCommand()
	{
		Items.Clear();
		var items = await DataStore.GetItemsAsync(true);
		foreach (var item in items)
		{
			Items.Add(item);
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
		await Shell.Current.GoToAsync($"{nameof(RecipeDetailPage)}?{nameof(RecipeDetailViewModel.ItemId)}={item.Id}");
	}

	public void OnAppearing()
	{
		ExecuteLoadItemsCommand();
	}

}
	public class NewRecipePage : ContentPage
	{
		public NewRecipePage()
		{
			Title = "New Recipe";
			Content = new StackLayout
			{
				Children =
				{
					new Label { Text = "Create a new recipe here." }
				}
			};
		}
}

	public class BaseViewModel : INotifyPropertyChanged
    {
        public IDataStore<Item> DataStore => DependencyService.Get<IDataStore<Item>>();

        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
			Action onChanged = null)
		{
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

	public class Item
    {
        public string Id { get; set; }
        public string RecipeName { get; set; }
    }

	public interface IDataStore<T>
    {
        Task<bool> AddItemAsync(T item);
        Task<bool> UpdateItemAsync(T item);
        Task<bool> DeleteItemAsync(string id);
        Task<T> GetItemAsync(string id);
        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);
    }
	
	[QueryProperty(nameof(ItemId), nameof(ItemId))]
	public class RecipeDetailViewModel : BaseViewModel
    {
        public Command EditRecipeCommand { get; }
        string _itemId;
        string _recipeName;
		public string Id { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
		public RecipeDetailViewModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
		{
            EditRecipeCommand = new Command(OnEditRecipe);
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
        public async void LoadItemId(string itemId)
        {
            try
            {
                var item = await DataStore.GetItemAsync(itemId);
                Id = item.Id;
                RecipeName = item.RecipeName;
                Title = RecipeName;

                var emptyFormattedString = new FormattedString();
                emptyFormattedString.Spans.Add(new Span { Text = "" });
            }
            catch (Exception) 
            {
                Debug.WriteLine("Failed to Load Item");
            }
        }

        private async void OnEditRecipe(object obj)
        {
            var editPage = new EditRecipePage();
            var editViewModel = (EditRecipeViewModel)editPage.BindingContext;
            editViewModel.Id = _itemId;
            
            await Shell.Current.Navigation.PushModalAsync(editPage);
        }

        public void OnAppearing()
        {
            IsBusy = true;
            LoadItemId(_itemId);
        }
    }

[QueryProperty(nameof(Id), nameof(Id))]
public class EditRecipeViewModel : BaseViewModel
{
	string _id;
	string _recipeName;
	public EditRecipeViewModel()
	{
		DeleteCommand = new Command(OnDelete);
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

	public async void LoadItemId(string itemId)
	{
		try
		{
			var item = await DataStore.GetItemAsync(itemId);
			RecipeName = item.RecipeName;
		}
		catch (Exception)
		{
			Debug.WriteLine("Failed to Load Item");
		}
	}
	public Command DeleteCommand { get; }

	private async void OnDelete()
	{
		await DataStore.DeleteItemAsync(_id);
		await Shell.Current.GoToAsync("../..");
	}
}
	public class MockDataStore : IDataStore<Item>
	{
		readonly List<Item> items;

		public MockDataStore()
		{
			items = new List<Item>()
			{
				new Item 
				{ 
					Id = "1", 
					RecipeName = "Spaghetti Carbonara",
				},
				new Item 
				{ 
					Id = "2", 
					RecipeName = "Chicken Tikka Masala",
					
				},
				new Item 
				{ 
					Id = "3", 
					RecipeName = "Chocolate Chip Cookies",
					
				},
				new Item 
				{ 
					Id = "4", 
					RecipeName = "Caesar Salad",
				},
				new Item 
				{ 
					Id = "5", 
					RecipeName = "Beef Tacos",
				}
			};
		}

		public async Task<bool> AddItemAsync(Item item)
		{
			items.Add(item);
			return await Task.FromResult(true);
		}

		public async Task<bool> UpdateItemAsync(Item item)
		{
			var oldItem = items.Where((Item arg) => arg.Id == item.Id).FirstOrDefault();
			if (oldItem != null)
			{
				items.Remove(oldItem);
				items.Add(item);
			}
			return await Task.FromResult(true);
		}

		public async Task<bool> DeleteItemAsync(string id)
		{
			var oldItem = items.Where((Item arg) => arg.Id == id).FirstOrDefault();
			if (oldItem != null)
			{
				items.Remove(oldItem);
				return await Task.FromResult(true);
			}
			return await Task.FromResult(false);
		}

		public async Task<Item> GetItemAsync(string id)
		{
			var item = items.FirstOrDefault(s => s.Id == id);
			return await Task.FromResult(item);
		}

		public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
		{
			return await Task.FromResult(items);
		}
	}

