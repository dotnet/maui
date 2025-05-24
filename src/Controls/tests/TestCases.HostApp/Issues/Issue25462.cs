namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25462, "StackLayout inside Scrollview with horizontal orientation not expanding", PlatformAffected.Android)]
	public class Issue25462 : TestContentPage
	{
		protected override void Init()
		{
			Content = new VerticalStackLayout()
			{
				Children = {
					new ScrollView
					{
						Orientation = ScrollOrientation.Horizontal,
						HeightRequest = 120,
						HorizontalOptions = LayoutOptions.Fill,
						BackgroundColor = Colors.Green,
						Content = new HorizontalStackLayout
						{
							HorizontalOptions = LayoutOptions.Fill,
							BackgroundColor = Colors.Pink,
							Children = {
								new Label
								{
									AutomationId="label",
									Text = "Hello"
								},
							}
						}
					},
					new ScrollView
					{
						Orientation = ScrollOrientation.Vertical,
						VerticalOptions = LayoutOptions.Fill,
						HorizontalOptions = LayoutOptions.Fill,
						BackgroundColor = Colors.Green,
						Content =
						new VerticalStackLayout
						{
						 	new StackLayout
							{
								HorizontalOptions = LayoutOptions.Fill,
								VerticalOptions = LayoutOptions.Center,
								BackgroundColor = Colors.Red,
								Children = {
									new Label
									{
										Text = "Fill"
									},
								}
							},
							new StackLayout
							{
								HorizontalOptions = LayoutOptions.End,
								VerticalOptions = LayoutOptions.Center,
								BackgroundColor = Colors.Red,
								Children = {
									new Label
									{
										Text = "End"
									},
								}
							},
							new StackLayout
							{
								HorizontalOptions = LayoutOptions.Start,
								VerticalOptions = LayoutOptions.Center,
								BackgroundColor = Colors.Red,
								Children = {
									new Label
									{
										Text = "Start"
									},
								}
							},
						}
					}
			}
			};
		}
	}
}
