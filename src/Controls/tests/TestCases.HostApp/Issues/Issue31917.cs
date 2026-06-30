namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31917, "SwipeItemView and SwipeItem background doesn't update on AppTheme change (Light/Dark)", PlatformAffected.All)]
public class Issue31917 : ContentPage
{
	public Issue31917()
	{
		this.SetAppThemeColor(BackgroundProperty, Colors.White, Colors.Black);

		Content = new StackLayout
		{
			Children =
			{
				new Label
				{
					Text = "SwipeItem AppTheme Background Test",
					HorizontalOptions = LayoutOptions.Center,
					Margin = new Thickness(10)
				},
				new CollectionView
				{
					ItemsSource = new[] { "Item 1", "Item 2", "Item 3" },
					ItemTemplate = new DataTemplate(() =>
					{
						var label = new Label();
						label.SetBinding(Label.TextProperty, ".");
						label.Padding = new Thickness(15);
						label.BackgroundColor = Colors.LightGray;

						var swipeItem = new SwipeItem
						{
							Text = "SwipeItem",
							AutomationId = "TestSwipeItem"
						};
						swipeItem.SetAppThemeColor(SwipeItem.BackgroundColorProperty, Colors.Yellow, Colors.Purple);

						var swipeItemView = new SwipeItemView
						{
							AutomationId = "TestSwipeItemView",
							Content = new Label
							{
								Text = "SwipeItemView",
								TextColor = Colors.White,
								HorizontalOptions = LayoutOptions.Center,
								VerticalOptions = LayoutOptions.Center
							}
						};
						swipeItemView.SetAppThemeColor(SwipeItemView.BackgroundColorProperty, Colors.Blue, Colors.Red);

						var swipeView = new SwipeView
						{
							AutomationId = "TestSwipeView",
							LeftItems = new SwipeItems
							{
								swipeItem,
								swipeItemView
							},
							Content = label
						};

						return swipeView;
					})
				},
				new Button
				{
					Text = "Change Theme",
					AutomationId = "changeThemeButton",
					Command = new Command(() =>
					{
						Application.Current!.UserAppTheme = Application.Current!.UserAppTheme != AppTheme.Dark ? AppTheme.Dark : AppTheme.Light;
					})
				},
				new Label
				{
					Text = "1. Swipe left on any item to reveal swipe actions\n2. Note the background colors\n3. Tap 'Change Theme'\n4. Background colors should update immediately",
					Margin = new Thickness(10),
					FontSize = 12
				}
			}
		};
	}
}