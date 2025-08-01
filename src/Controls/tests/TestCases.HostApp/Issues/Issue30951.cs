namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30951, "Fix Android ScrollView to measure content correctly", PlatformAffected.Android)]
public class Issue30951 : ContentPage
{
	private Label mealTypeLabel;
	private ScrollView mealTypeScrollView;
	private StackLayout mealTypeStack;
	private ScrollView mainScrollView;
	private Grid mainGrid;

	public Issue30951()
	{
		Title = "Issue 30951 - ScrollView Content Measurement Fix";

		mainScrollView = new ScrollView
		{
			AutomationId = "Issue30951_MainScrollView",
			HeightRequest = 500
		};

		mainGrid = new Grid
		{
			Margin = new Thickness(20, 5),
			AutomationId = "Issue30951_MainGrid",
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Star },
				new RowDefinition { Height = GridLength.Star } // Changed to 4.5*
			},
			ColumnDefinitions =
			{
				new ColumnDefinition { Width = GridLength.Star }
			}
		};


		mealTypeLabel = new Label
		{
			Text = "Search by meal type",
			FontSize = 24,
			FontAttributes = FontAttributes.Bold,
			AutomationId = "Issue30951_MealTypeLabel"
		};
		mealTypeLabel.SetValue(Grid.RowProperty, 0);
		mealTypeLabel.SetValue(Grid.ColumnProperty, 0);

		mealTypeStack = new StackLayout
		{
			Orientation = StackOrientation.Horizontal,
			Spacing = 10,
			Padding = new Thickness(0, 5),
			AutomationId = "Issue30951_MealTypeStack"
		};

		string[] mealTypes = { "Breakfast", "Lunch", "Dinner", "Snack", "Dessert", "Drinks" };
		for (int i = 0; i < mealTypes.Length; i++)
		{
			var button = new Button
			{
				Text = mealTypes[i],
				AutomationId = $"Issue30951_Button_{mealTypes[i]}"
			};
			mealTypeStack.Children.Add(button);
		}
		mealTypeScrollView = new ScrollView
		{
			Orientation = ScrollOrientation.Horizontal,
			Content = mealTypeStack,
			Margin = new Thickness(0, 0, 0, 10),
			AutomationId = "Issue30951_HorizontalScrollView"
		};
		mealTypeScrollView.SetValue(Grid.RowProperty, 1);
		mealTypeScrollView.SetValue(Grid.ColumnProperty, 0);

		mainGrid.Children.Add(mealTypeLabel);
		mainGrid.Children.Add(mealTypeScrollView);
		mainScrollView.Content = mainGrid;

		Content = mainScrollView;
	}
}
