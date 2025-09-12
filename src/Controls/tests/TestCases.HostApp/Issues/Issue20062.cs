using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20062, "CollectionView - SelectedItem visual state manager not working",
	PlatformAffected.Android)]
public class Issue20062 : TestContentPage
{
	CollectionView collection;
	
	List<string> list = new List<string>();
	protected override void Init()
	{
		  list.Add("a");
            list.Add("a");
            list.Add("a");
            list.Add("a");
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
				label.SetBinding(Label.TextProperty, ".");

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
		collection.ItemsSource = list;

		Content = new ScrollView
		{
			Content = layout
		};

	}
}

