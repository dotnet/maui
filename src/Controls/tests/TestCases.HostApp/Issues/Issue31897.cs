using System.ComponentModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31897, "CollectionView card height appears larger in Developer Balance sample", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue31897 : ContentPage
{
	Button getHeight;
	Button addItemsButton;
	Button removeItemsButton;
	Button updateNewItemsButton;
	Label HeightLabel;
	Label ItemCountLabel;
	CollectionView2 ProjectsCollectionView;
	Issue31897ViewModel viewModel;

	public Issue31897()
	{
		var mainGrid = new Grid();

		var scrollView = new ScrollView();

		// Create the inner grid with padding and row spacing
		var innerGrid = new Grid
		{
			Padding = new Thickness(10),
			RowSpacing = 10
		};

		// Add row definitions
		innerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Controls row
		innerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Height label
		innerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Item count label
		innerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // CollectionView

		// Create a horizontal stack for control buttons
		var controlsStack = new HorizontalStackLayout
		{
			Spacing = 10
		};

		// Create the "Get CV Height" button
		getHeight = new Button
		{
			Text = "Get CV Height",
			AutomationId = "GetHeightButton"
		};

		getHeight.Clicked += (object sender, EventArgs e) =>
		{
			HeightLabel.Text = Math.Round(ProjectsCollectionView.Height).ToString();
		};

		// Create the "Add Items" button
		addItemsButton = new Button
		{
			Text = "Add Items",
			AutomationId = "AddItemsButton"
		};

		addItemsButton.Clicked += (object sender, EventArgs e) =>
		{
			viewModel.AddItems(1); // Add 1 item
			UpdateItemCount();
		};

		// Create the "Remove Items" button
		removeItemsButton = new Button
		{
			Text = "Remove Items",
			AutomationId = "RemoveItemsButton"
		};

		removeItemsButton.Clicked += (object sender, EventArgs e) =>
		{
			viewModel.RemoveItems(2); // Remove 2 items
			UpdateItemCount();
		};

		// Create the "Update New Items" button
		updateNewItemsButton = new Button
		{
			Text = "Update New",
			AutomationId = "UpdateNewItemsButton"
		};

		updateNewItemsButton.Clicked += (object sender, EventArgs e) =>
		{
			viewModel.UpdateWithNewItems();
			UpdateItemCount();
		};

		// Add buttons to horizontal stack
		controlsStack.Children.Add(getHeight);
		controlsStack.Children.Add(addItemsButton);
		controlsStack.Children.Add(removeItemsButton);
		controlsStack.Children.Add(updateNewItemsButton);

		getHeight.SetValue(SemanticProperties.HeadingLevelProperty, SemanticHeadingLevel.Level1);
		Grid.SetRow(controlsStack, 0);

		// Create the height label
		HeightLabel = new Label();
		HeightLabel.AutomationId = "HeightLabel";
		HeightLabel.SetValue(SemanticProperties.HeadingLevelProperty, SemanticHeadingLevel.Level1);
		Grid.SetRow(HeightLabel, 1);

		// Create the item count label
		ItemCountLabel = new Label
		{
			AutomationId = "ItemCountLabel",
			Text = "Items: 0"
		};
		ItemCountLabel.SetValue(SemanticProperties.HeadingLevelProperty, SemanticHeadingLevel.Level1);
		Grid.SetRow(ItemCountLabel, 2);

		// Create the CollectionView
		ProjectsCollectionView = new CollectionView2
		{
			Margin = new Thickness(-7.5, 0),
			MinimumHeightRequest = 250,
			SelectionMode = SelectionMode.Single
		};

		// Set up the ItemsLayout
		ProjectsCollectionView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
		{
			ItemSpacing = 7.5
		};

		// Create the DataTemplate for items
		var itemTemplate = new DataTemplate(() =>
		{
			var border = new Border
			{
				WidthRequest = 200,
				Background = Colors.GreenYellow
			};

			var contentView = new ContentView();

			var verticalStackLayout = new VerticalStackLayout
			{
				Spacing = 15,
				Background = Colors.Red,
				VerticalOptions = LayoutOptions.Start
			};

			// Name label
			var nameLabel = new Label
			{
				TextColor = Colors.Gray,
				FontSize = 14,
				TextTransform = TextTransform.Uppercase
			};
			nameLabel.SetBinding(Label.TextProperty, "Name");

			// Description label
			var descriptionLabel = new Label
			{
				LineBreakMode = LineBreakMode.WordWrap
			};
			descriptionLabel.SetBinding(Label.TextProperty, "Description");

			verticalStackLayout.Children.Add(nameLabel);
			verticalStackLayout.Children.Add(descriptionLabel);
			contentView.Content = verticalStackLayout;
			border.Content = contentView;

			return border;
		});

		ProjectsCollectionView.ItemTemplate = itemTemplate;

		// Set up data binding
		viewModel = new Issue31897ViewModel();
		BindingContext = viewModel;
		ProjectsCollectionView.SetBinding(CollectionView.ItemsSourceProperty, "Projects");

		Grid.SetRow(ProjectsCollectionView, 3);

		// Add all controls to the inner grid
		innerGrid.Children.Add(controlsStack);
		innerGrid.Children.Add(HeightLabel);
		innerGrid.Children.Add(ItemCountLabel);
		innerGrid.Children.Add(ProjectsCollectionView);

		// Set up the hierarchy
		scrollView.Content = innerGrid;
		mainGrid.Children.Add(scrollView);
		Content = mainGrid;

		// Initialize item count display
		UpdateItemCount();
	}

	void UpdateItemCount()
	{
		ItemCountLabel.Text = $"Items: {viewModel.Projects.Count}";
	}

	public class Issue31897ViewModel : INotifyPropertyChanged
	{
		List<Issue31897Project> _projects = new();
		int _nextId = 0;

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public List<Issue31897Project> Projects
		{
			get { return _projects; }
			set
			{
				_projects = value;
				OnPropertyChanged(nameof(Projects));
			}
		}

		public Issue31897ViewModel()
		{
			LoadData();
		}

		void LoadData()
		{
			var projects = new List<Issue31897Project>();
			for (int i = 0; i < 4; i++)
			{
				projects.Add(new Issue31897Project
				{
					ID = i,
					Name = "Developer balance card view " + (i + 1),
					Description = "This is a sample description for project " + (i + 1)
				});
				_nextId = i + 1;
			}

			Projects = projects;
		}

		public void AddItems(int count)
		{
			var currentProjects = new List<Issue31897Project>(_projects);
			
			for (int i = 0; i < count; i++)
			{
				currentProjects.Add(new Issue31897Project
				{
					ID = _nextId,
					Name = "Added card view " + (_nextId + 1),
					Description = "This is a dynamically added description for project " + (_nextId + 1)
				});
				_nextId++;
			}

			Projects = currentProjects;
		}

		public void RemoveItems(int count)
		{
			var currentProjects = new List<Issue31897Project>(_projects);
			
			int itemsToRemove = Math.Min(count, currentProjects.Count);
			for (int i = 0; i < itemsToRemove; i++)
			{
				if (currentProjects.Count > 0)
				{
					currentProjects.RemoveAt(currentProjects.Count - 1);
				}
			}

			if (currentProjects.Count == 0)
			{
				_nextId = 0; // Reset ID counter if all items are removed
			}

			Projects = currentProjects;
		}

		public void ClearItems()
		{
			Projects = new List<Issue31897Project>();
		}

		public void UpdateWithNewItems()
		{
			var projects = new List<Issue31897Project>();
			for (int i = 0; i < 6; i++)
			{
				projects.Add(new Issue31897Project
				{
					ID = i + 500,
					Name = "New updated item " + (i + 1),
					Description = "This is a completely new item set with fresh content " + (i + 1)
				});
			}
			Projects = projects;
		}
	}

	public class Issue31897Project
	{
		public int ID { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
	}
}