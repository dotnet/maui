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
		DependencyService.Register<Issue16020IDataStore<Issue16020Item>, Issue16020MockDataStore>();
		Routing.RegisterRoute(nameof(Issue16020EditRecipePage), typeof(Issue16020EditRecipePage));

		var tabBar = new TabBar();
		var searchTab = new ShellContent
		{
			Title = "Recipe Search",
			Route = "StartingPage",
			ContentTemplate = new DataTemplate(typeof(Issue16020StartingPage))
		};
		var recipesTab = new ShellContent
		{
			Title = "My Recipes",
			Route = "MyRecipesPage",
			ContentTemplate = new DataTemplate(typeof(Issue16020MyRecipesPage))
		};
		tabBar.Items.Add(searchTab);
		tabBar.Items.Add(recipesTab);
		Items.Add(tabBar);
	}
}

public class Issue16020StartingPage : ContentPage
{
	public Issue16020StartingPage()
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

public class Issue16020MyRecipesPage : ContentPage
{
	Issue16020MyRecipesViewModel _viewModel;
	CarouselView vMyRecipesListView;
	Label _label;

	public Issue16020MyRecipesPage()
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
		var button2 = new Button();
		button2.Text ="Add new Recipe";
		button2.AutomationId = "AddNewRecipeButton";
		button2.Clicked +=  (sender, e) =>
		{
			Issue16020Item NewRecipe = new Issue16020Item()
			{
				Id = 5.ToString(),
				RecipeName = "Beef Tacos",
			};
		_viewModel.Items.Add(NewRecipe);
		};
		stackLayout.Children.Add(button2);
		_label = new Label
		{
			AutomationId = "CarouselViewCountLabel",
			FontSize = 20,
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(0, 10, 0, 10)
		};
		_viewModel = new Issue16020MyRecipesViewModel();
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
		if (sender is Grid grid && grid.BindingContext is Issue16020Item item)
		{
			var editRecipePage = new Issue16020EditRecipePage();
			var editViewModel = new Issue16020EditRecipeViewModel();
			editRecipePage.BindingContext = editViewModel;
			editViewModel.Id = item.Id;
			await Shell.Current.Navigation.PushModalAsync(editRecipePage);
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

public class Issue16020EditRecipePage : ContentPage
{
	public Issue16020EditRecipePage()
	{
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
		var deleteButton = stackLayout.Children.OfType<Button>().FirstOrDefault(b => b.AutomationId == "DeleteRecipeButton");

		recipeNameLabel?.SetBinding(Label.TextProperty, nameof(Issue16020EditRecipeViewModel.RecipeName));
		deleteButton?.SetBinding(Button.CommandProperty, nameof(Issue16020EditRecipeViewModel.DeleteCommand));

		Content = stackLayout;
	}
}

public class Issue16020MyRecipesViewModel : Issue16020BaseViewModel
{
	public ObservableCollection<Issue16020Item> Items { get; }
	public Issue16020MyRecipesViewModel()
	{
		Title = "Recipes";
		Items = new ObservableCollection<Issue16020Item>();
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

	public void OnAppearing()
	{
		ExecuteLoadItemsCommand();
	}
}
	

	public class Issue16020BaseViewModel : INotifyPropertyChanged
    {
        public Issue16020IDataStore<Issue16020Item> DataStore => DependencyService.Get<Issue16020IDataStore<Issue16020Item>>();

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

	public class Issue16020Item
    {
        public string Id { get; set; }
        public string RecipeName { get; set; }
    }

public interface Issue16020IDataStore<T>
{
	Task<bool> AddItemAsync(T item);
	Task<bool> UpdateItemAsync(T item);
	Task<bool> DeleteItemAsync(string id);
	Task<T> GetItemAsync(string id);
	Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);
}

[QueryProperty(nameof(Id), nameof(Id))]
public class Issue16020EditRecipeViewModel : Issue16020BaseViewModel
{
	string _id;
	string _recipeName;
	public Issue16020EditRecipeViewModel()
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
			if (string.IsNullOrEmpty(itemId))
				return;
			var item = await DataStore.GetItemAsync(itemId);
			if (item != null)
			{
				RecipeName = item.RecipeName;
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Error loading item: {ex.Message}");
		}
	}
	public Command DeleteCommand { get; }

	private async void OnDelete()
	{

		await DataStore.DeleteItemAsync(_id);
		await Shell.Current.Navigation.PopModalAsync();
	}
	
}
	public class Issue16020MockDataStore : Issue16020IDataStore<Issue16020Item>
	{
		readonly List<Issue16020Item> items;

		public Issue16020MockDataStore()
		{
			items = new List<Issue16020Item>()
			{
				new Issue16020Item
				{ 
					Id = "1", 
					RecipeName = "Spaghetti Carbonara",
				},
				new Issue16020Item
				{ 
					Id = "2", 
					RecipeName = "Chicken Tikka Masala",
					
				},
				new Issue16020Item
				{ 
					Id = "3", 
					RecipeName = "Chocolate Chip Cookies",
					
				},
				new Issue16020Item
				{ 
					Id = "4", 
					RecipeName = "Caesar Salad",
				},
			};
		}

		public async Task<bool> AddItemAsync(Issue16020Item item)
		{
			items.Add(item);
			return await Task.FromResult(true);
		}

		public async Task<bool> UpdateItemAsync(Issue16020Item item)
		{
			var oldItem = items.Where((Issue16020Item arg) => arg.Id == item.Id).FirstOrDefault();
			if (oldItem != null)
			{
				items.Remove(oldItem);
				items.Add(item);
			}
			return await Task.FromResult(true);
		}

		public async Task<bool> DeleteItemAsync(string id)
		{
			var oldItem = items.Where((Issue16020Item arg) => arg.Id == id).FirstOrDefault();
			if (oldItem != null)
			{
				items.Remove(oldItem);
				return await Task.FromResult(true);
			}
			return await Task.FromResult(false);
		}

		public async Task<Issue16020Item> GetItemAsync(string id)
		{
			var item = items.FirstOrDefault(s => s.Id == id);
			return await Task.FromResult(item);
		}

		public async Task<IEnumerable<Issue16020Item>> GetItemsAsync(bool forceRefresh = false)
		{
			return await Task.FromResult(items);
		}
	}

