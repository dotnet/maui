using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31897, "CollectionView card height appears larger in Developer Balance sample", PlatformAffected.iOS | PlatformAffected.macOS)]
public partial class Issue31897 : ContentPage
{
	ObservableCollection<string> Countries = new();
	Button getHeight;
	Label HeightLabel;
	CollectionView2 ProjectsCollectionView;

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
		innerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
		innerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
		innerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

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

		getHeight.SetValue(SemanticProperties.HeadingLevelProperty, SemanticHeadingLevel.Level1);
		Grid.SetRow(getHeight, 0);

		// Create the height label
		HeightLabel = new Label();
		HeightLabel.AutomationId = "HeightLabel";
		HeightLabel.SetValue(SemanticProperties.HeadingLevelProperty, SemanticHeadingLevel.Level1);
		Grid.SetRow(HeightLabel, 1);

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
		var viewModel = new Issue31897ViewModel();
		BindingContext = viewModel;
		ProjectsCollectionView.SetBinding(CollectionView.ItemsSourceProperty, "Projects");

		Grid.SetRow(ProjectsCollectionView, 2);

		// Add all controls to the inner grid
		innerGrid.Children.Add(getHeight);
		innerGrid.Children.Add(HeightLabel);
		innerGrid.Children.Add(ProjectsCollectionView);

		// Set up the hierarchy
		scrollView.Content = innerGrid;
		mainGrid.Children.Add(scrollView);
		Content = mainGrid;
	}

	public partial class Issue31897ViewModel : INotifyPropertyChanged
	{

		List<Issue31897Project> _projects = new();

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