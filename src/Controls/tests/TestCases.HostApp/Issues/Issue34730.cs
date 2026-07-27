namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34730, "HideSoftInputOnTapped doesn't work on Modal Pages", PlatformAffected.Android)]
public class Issue34730 : TestContentPage
{
	protected override void Init()
	{
		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = 20,
			Children =
			{
				new Label
				{
					Text = "Tap the button to open a modal page with HideSoftInputOnTapped enabled.",
					AutomationId = "Instructions"
				},
				new Button
				{
					Text = "Open Modal Page",
					AutomationId = "OpenModalButton",
					Command = new Command(async () =>
					{
						var modalPage = new ContentPage
						{
							HideSoftInputOnTapped = true,
							Content = new VerticalStackLayout
							{
								Spacing = 20,
								Padding = 20,
								Children =
								{
									new Entry
									{
										Placeholder = "Tap here, then tap empty space",
										AutomationId = "ModalEntry"
									},
									new Label
									{
										Text = "Tap this area to dismiss the keyboard",
										AutomationId = "ModalEmptySpace",
										HeightRequest = 300,
										BackgroundColor = Colors.LightGray
									},
									new Button
									{
										Text = "Close Modal",
										AutomationId = "CloseModalButton",
										Command = new Command(async () =>
										{
											await Navigation.PopModalAsync();
										})
									}
								}
							}
						};

						await Navigation.PushModalAsync(new NavigationPage(modalPage));
					})
				}
			}
		};
	}
}
