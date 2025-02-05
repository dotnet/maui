using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 27427, "[MAUI] - iOS SearchBar ignores WidthRequest and HeightRequest property values", PlatformAffected.iOS | PlatformAffected.macOS)]
	public class Issue27427 : TestContentPage
	{
		protected override void Init()
		{
			StackLayout mainLayout = new StackLayout
			{
				AutomationId = "MainParentLayout",
				Spacing = 20
			};

			VerticalStackLayout verticalLayout = new VerticalStackLayout
			{
				Spacing = 10,
				Padding = new Thickness(20),
				Children =
				{
					new SearchBar { HeightRequest = 150, BackgroundColor = Colors.LightGray, Placeholder = "Vertical Layout SearchBar" }
				}
			};

			HorizontalStackLayout horizontalLayout = new HorizontalStackLayout
			{
				Spacing = 10,
				Padding = new Thickness(20),
				Children =
				{
					new SearchBar
					{
						WidthRequest = 300,
						BackgroundColor = Colors.LightBlue,
						Placeholder = "Horizontal Layout SearchBar"
					}
				}
			};

			FlexLayout flexLayout = new FlexLayout
			{
				Direction = FlexDirection.Row,
				Wrap = FlexWrap.Wrap,
				Padding = new Thickness(20),
				Children =
				{
					new SearchBar
					{
						HeightRequest = 80,
						BackgroundColor = Colors.LightPink,
						Placeholder = "Flex Layout SearchBar 1"
					},
					new SearchBar
					{
						HeightRequest = 100,
						BackgroundColor = Colors.LightPink,
						Placeholder = "Flex Layout SearchBar 2"
					}
				}
			};

			AbsoluteLayout absoluteLayout = new AbsoluteLayout
			{
				Padding = new Thickness(20),
				Children =
				{
					new SearchBar
					{
						BackgroundColor = Colors.LightGreen,
						Placeholder = "Absolute Layout SearchBar"
					}
				}
			};

			absoluteLayout.SetLayoutBounds(absoluteLayout.Children[0], new Rect(0, 0, 200, 50));

			Grid gridLayout = new Grid
			{
				Padding = new Thickness(20),
				RowDefinitions =
				{
					new RowDefinition(),
				},
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Star }
				}
			};

			SearchBar gridSearchBar1 = new SearchBar
			{
				HeightRequest = 80,
				BackgroundColor = Colors.LightYellow,
				Placeholder = "Grid SearchBar 1"
			};
			Grid.SetRow(gridSearchBar1, 0);
			Grid.SetColumn(gridSearchBar1, 0);

			SearchBar gridSearchBar2 = new SearchBar
			{
				WidthRequest = 150,
				BackgroundColor = Colors.LightYellow,
				Placeholder = "Grid SearchBar 2"
			};
			Grid.SetRow(gridSearchBar2, 0);
			Grid.SetColumn(gridSearchBar2, 1);

			gridLayout.Add(gridSearchBar1);
			gridLayout.Add(gridSearchBar2);

			mainLayout.Children.Add(verticalLayout);
			mainLayout.Children.Add(horizontalLayout);
			mainLayout.Children.Add(flexLayout);
			mainLayout.Children.Add(absoluteLayout);
			mainLayout.Children.Add(gridLayout);

			Content = mainLayout;
		}
	}
}

