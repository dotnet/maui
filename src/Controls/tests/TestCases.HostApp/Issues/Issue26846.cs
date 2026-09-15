namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 26846, "Shell PopToRootAsync doesn't happen instantly - previous pages flash quickly", PlatformAffected.Android)]
public class Issue26846 : TestShell
{
	protected override void Init()
	{
		AddContentPage(new ContentPage()
		{
			Content = new VerticalStackLayout
			{
				Children =
				{
					new Label { Text = "This is a modal page1" },
					new Button { Text = "Open modal page2", AutomationId="OpenModalPage2", Command = new Command(async () => await Navigation.PushModalAsync(new ContentPage()
						{
							Content = new VerticalStackLayout
							{
								Children = {
									new Label { Text = "This is a modal page2" },
									new Button { Text = "Open modal page3",  AutomationId="OpenModalPage3", Command = new Command(async () => await Navigation.PushModalAsync(new ContentPage()
									{
										Content = new VerticalStackLayout
										{
											Children = {
												new Label { Text = "This is a modal page3" },
												new Button { Text = "Close",  AutomationId="CloseModalPage3", Command = new Command(async () => await Navigation.PopModalAsync(false)) },
												new Button { Text = "PopRoot", AutomationId="PopRootPage3", Command = new Command(async () => await Navigation.PopToRootAsync(false)) },
												new Button { Text = "PopRoot Animated", AutomationId="PopRootPage4", Command = new Command(async () => await Navigation.PopToRootAsync()) }
											}
										}
									})) },
									new Button { Text = "Close", AutomationId="CloseModalPage2", Command = new Command(async () => await Navigation.PopModalAsync(false)) }
								}
							}
						})) },
				}
			}
		});
	}
}
