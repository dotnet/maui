namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30970, "Alert popup may be displayed on wrong window when modal page navigation is in progress", PlatformAffected.iOS)]
public partial class Issue30970 : ContentPage
{
	private bool _modalPageWasShown;

	public Issue30970()
	{
		Content = new VerticalStackLayout
		{
			Children =
			{
				new Button()
				{
					Text = "Click",
					AutomationId = "OpenModalButton",
					Command = new Command(() =>
					{
						Navigation.PushModalAsync(new ContentPage()
						{
							Content = new VerticalStackLayout
							{
								Children =
								{
									new Button
									{
										Text = "Close Modal",
										AutomationId= "CloseModalButton",
										Command = new Command(() => Navigation.PopModalAsync())
									}
								}
							}
						});
						_modalPageWasShown = true;
					})
				}
			}
		};
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (_modalPageWasShown)
			await Application.Current.Windows[0].Page!.DisplayAlert("My alert", "Can you see this alert?", "Yes");
	}
}