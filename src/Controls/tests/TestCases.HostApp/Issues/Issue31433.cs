namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31433, "HorizontalOptions for content inside ScrollView does not work on Android", PlatformAffected.Android)]
public class Issue31433 : ContentPage
{
	public Issue31433()
	{
		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				CreateHorizontalStackScrollView(),
				CreateFlexLayoutScrollView()
			}
		};
	}

	static ScrollView CreateHorizontalStackScrollView()
	{
		var scrollView = new ScrollView
		{
			Margin = new Thickness(10),
			Background = new SolidColorBrush(Colors.Black),
			Content = new HorizontalStackLayout
			{
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Button
					{
						Text = "Button1",
						AutomationId = "HStackButton1",
						Background = new SolidColorBrush(Colors.Red),
						HeightRequest = 150,
						WidthRequest = 150,
						Margin = new Thickness(10)
					},
					new Button
					{
						Text = "Button2",
						Background = new SolidColorBrush(Colors.Green),
						HeightRequest = 150,
						WidthRequest = 150,
						Margin = new Thickness(10)
					}
				}
			}
		};

		Grid.SetRow(scrollView, 0);
		return scrollView;
	}

	static ScrollView CreateFlexLayoutScrollView()
	{
		var flex = new FlexLayout
		{
			Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
			AlignContent = Microsoft.Maui.Layouts.FlexAlignContent.Start,
			Direction = Microsoft.Maui.Layouts.FlexDirection.Row,
			AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Start,
			HorizontalOptions = LayoutOptions.Center,
			Children =
			{
				new Button
				{
					Text = "Button1",
					Background = new SolidColorBrush(Colors.Red),
					HeightRequest = 150,
					WidthRequest = 150,
					Margin = new Thickness(10)
				},
				new Button
				{
					Text = "Button2",
					Background = new SolidColorBrush(Colors.Green),
					HeightRequest = 150,
					WidthRequest = 150,
					Margin = new Thickness(10)
				}
			}
		};

		var scrollView = new ScrollView
		{
			Margin = new Thickness(10),
			Background = new SolidColorBrush(Colors.Black),
			Content = flex
		};

		Grid.SetRow(scrollView, 1);
		return scrollView;
	}
}
