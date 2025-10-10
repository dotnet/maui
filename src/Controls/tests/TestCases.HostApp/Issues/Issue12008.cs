using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 12008, "CollectionView Drag and Drop Reordering Can't Drop in Empty Group", PlatformAffected.Android)]
public partial class Issue12008 : ContentPage
{
	private Label statusLabel;
	private CollectionView collectionView;
	private Button btnCreateEmptyGroup;
	public Issue12008()
	{
		this.BindingContext = new Issue12008ViewModel();
		var grid = new Grid
		{
			RowDefinitions = new RowDefinitionCollection
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		// Top StackLayout
		var stackLayout = new StackLayout
		{
			Padding = 10
		};

		var titleLabel = new Label
		{
			Text = "Drag and Drop Test - Empty Groups",
			FontSize = 18,
			FontAttributes = FontAttributes.Bold,
			Margin = new Thickness(0, 0, 0, 10)
		};

		var instructionsLabel = new Label
		{
			Text = "Instructions: Try dragging items between groups, including into empty groups.",
			FontSize = 14,
			Margin = new Thickness(0, 0, 0, 10)
		};

		btnCreateEmptyGroup = new Button
		{
			Text = "Create Empty Group",
			AutomationId = "CreateEmptyGroupButton12008"
		};
		btnCreateEmptyGroup.Clicked += OnCreateEmptyGroupClicked;

		statusLabel = new Label
		{
			Text = "Status: Ready",
			FontSize = 12,
			AutomationId = "StatusLabel12008"
		};

		stackLayout.Children.Add(titleLabel);
		stackLayout.Children.Add(instructionsLabel);
		stackLayout.Children.Add(btnCreateEmptyGroup);
		stackLayout.Children.Add(statusLabel);

		Grid.SetRow(stackLayout, 0);
		grid.Children.Add(stackLayout);

		// CollectionView
		collectionView = new CollectionView
		{
			IsGrouped = true,
			CanReorderItems = true,
			CanMixGroups = true,
			AutomationId = "CollectionView12008"
		};
		collectionView.SetBinding(ItemsView.ItemsSourceProperty, "GroupedItems");

		// Group Header Template
		collectionView.GroupHeaderTemplate = new DataTemplate(() =>
		{
			var headerGrid = new Grid
			{
				BackgroundColor = Colors.LightBlue,
				Padding = 10,
				ColumnDefinitions = new ColumnDefinitionCollection
				{
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Auto }
				}
			};

			var headerLabel = new Label
			{
				FontAttributes = FontAttributes.Bold,
				FontSize = 16
			};
			headerLabel.SetBinding(Label.TextProperty, "GroupName");
			headerLabel.SetBinding(AutomationIdProperty, new Binding("GroupName", stringFormat: "GroupHeader12008{0}"));

			var countLabel = new Label
			{
				FontSize = 12
			};
			countLabel.SetBinding(Label.TextProperty, new Binding("Count", stringFormat: "Count: {0}"));
			countLabel.SetBinding(AutomationIdProperty, new Binding("GroupName", stringFormat: "GroupCount12008{0}"));

			headerGrid.Children.Add(headerLabel);
			headerGrid.Children.Add(countLabel);
			Grid.SetColumn(headerLabel, 0);
			Grid.SetColumn(countLabel, 1);
			return headerGrid;
		});

		// Item Template
		collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var itemGrid = new Grid
			{
				Padding = 10,
				BackgroundColor = Colors.LightGray,
				Margin = new Thickness(2)
			};

			var itemLabel = new Label
			{
				FontSize = 14
			};
			itemLabel.SetBinding(Label.TextProperty, "Name");
			itemLabel.SetBinding(AutomationIdProperty, new Binding("Name", stringFormat: "Item12008{0}"));

			itemGrid.Children.Add(itemLabel);
			return itemGrid;
		});

		Grid.SetRow(collectionView, 1);
		grid.Children.Add(collectionView);

		Content = grid;
	}


	private void OnCreateEmptyGroupClicked(object sender, EventArgs e)
	{
		if (this.BindingContext is Issue12008ViewModel viewModel)
		{
			viewModel.CreateEmptyGroup();
			statusLabel.Text = "Status: Empty group created";
		}
	}
}

public class Issue12008ViewModel : INotifyPropertyChanged
{
	private ObservableCollection<Issue12008Group> _groupedItems;

	public ObservableCollection<Issue12008Group> GroupedItems
	{
		get => _groupedItems;
		set
		{
			_groupedItems = value;
			OnPropertyChanged();
		}
	}

	public Issue12008ViewModel()
	{
		InitializeData();
	}

	private void InitializeData()
	{
		GroupedItems = new ObservableCollection<Issue12008Group>
			{
				new Issue12008Group("GroupA", new List<Issue12008Item>
				{
					new Issue12008Item("ItemA1"),
					new Issue12008Item("ItemA2"),
					new Issue12008Item("ItemA3")
				}),
				new Issue12008Group("GroupB", new List<Issue12008Item>
				{
					new Issue12008Item("ItemB1"),
					new Issue12008Item("ItemB2")
				}),
				new Issue12008Group("GroupC", new List<Issue12008Item>
				{
					new Issue12008Item("ItemC1")
				})
			};
	}

	public void CreateEmptyGroup()
	{
		var emptyGroup = new Issue12008Group("EmptyGroup", new List<Issue12008Item>());
		GroupedItems.Add(emptyGroup);
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

public class Issue12008Group : ObservableCollection<Issue12008Item>
{
	public string GroupName { get; }

	public Issue12008Group(string groupName, IEnumerable<Issue12008Item> items) : base(items)
	{
		GroupName = groupName;
	}
}

public class Issue12008Item
{
	public string Name { get; }

	public Issue12008Item(string name)
	{
		Name = name;
	}
}

