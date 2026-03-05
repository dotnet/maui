using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20062, "CollectionView - SelectedItem visual state manager not working",
	PlatformAffected.Android)]
public class Issue20062 : TestContentPage
{
	CollectionView collection;
	Issue20062ViewModel viewModel;
	protected override void Init()
	{

		collection = new CollectionView
		{
			SelectionMode = SelectionMode.Single,
			ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
			{
				HorizontalItemSpacing = 12,
				VerticalItemSpacing = 12
			},
			ItemTemplate = new DataTemplate(() =>
			{
				var border = new Border
				{
					Padding = 4,
					BackgroundColor = Colors.White,
					Stroke = Colors.Gray,
					StrokeShape = new RoundRectangle { CornerRadius = 12 },
					StrokeThickness = 1
				};

				var label = new Label
				{
					FontFamily = "SemiBold",
					TextColor = Colors.Black,
					FontSize = 14,
					HorizontalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, "Name");
				label.SetBinding(Label.AutomationIdProperty, "Id");

				var grid = new Grid
				{
					RowDefinitions =
					{
						new RowDefinition { Height = new GridLength(80) },
						new RowDefinition { Height = new GridLength(40) }
					}
				};

				grid.Add(label, 0, 1);

				border.Content = grid;
				VisualStateManager.SetVisualStateGroups(border, new VisualStateGroupList
				{
					new VisualStateGroup
					{
						Name = "CommonStates",
						States =
						{
							new VisualState { Name = "Normal" },
							new VisualState
							{
								Name = "Selected",
								Setters =
								{
									new Setter { Property = Border.BackgroundColorProperty, Value = Colors.Blue },
									new Setter { Property = Border.StrokeProperty, Value = Colors.Red }
								}
							}
						}
					}
				});

				return border;
			})
		};
		collection.AutomationId = "CollectionView";

		var layout = new VerticalStackLayout
		{
			Padding = new Thickness(30, 0),
			Spacing = 25,
			Children = { collection }
		};
		BindingContext = viewModel = new Issue20062ViewModel();
		collection.ItemsSource = viewModel.Items;

		Content = new ScrollView
		{
			Content = layout
		};

	}
}

public class Issue20062_Item
{
	public string Name { get; set; }
	public string Id { get; set; }
}

public class Issue20062ViewModel
{
	public List<Issue20062_Item> Items { get; set; }

	public Issue20062ViewModel()
	{
		Items = new List<Issue20062_Item>
		{
			new Issue20062_Item { Name = "a", Id = "FirstItem" },
			new Issue20062_Item { Name = "a", Id = "SecondItem" },
			new Issue20062_Item { Name = "a", Id = "ThirdItem" },
			new Issue20062_Item { Name = "a", Id = "FourthItem" }
		};
	}
}

