namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 23575, "Gradient never returns to the correct colour", PlatformAffected.iOS)]
	public class Issue23575 : ContentPage
	{
		public Issue23575()
		{
			BackgroundColor = Colors.Red;

			Content = new VerticalStackLayout
			{
				Children =
				{
					new Label()
					{
						AutomationId = "Label",
						Text = "Hello, Maui!"
					},
					new Border
					{
						HeightRequest = 200,
						WidthRequest = 200,
						Background= new LinearGradientBrush
						{
							EndPoint = new Point(0, 1),
							GradientStops = new GradientStopCollection
							{
								new GradientStop { Offset = 0.5f, Color = Colors.Red },
								new GradientStop { Offset = 1.0f, Color = Colors.Blue }
							}
						}
					}
				}
			};
		}
	}
}

