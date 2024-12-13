namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6484, "[iOS] Shell - Go back two pages crashes the app with a NullReferenceException",
	PlatformAffected.iOS)]

public class Issue6484 : TestShell
{
	StackLayout layout = null;
	ContentPage page = null;
	protected override void Init()
	{
		layout = new StackLayout();
		page = new ContentPage()
		{
			Content = layout
		};

		page.Appearing += OnPageAppearing;

		AddContentPage(page);
	}

	async void OnPageAppearing(object sender, EventArgs e)
	{
		page.Appearing -= OnPageAppearing;

		var removeMe = new ContentPage();
		await Navigation.PushAsync(removeMe);
		await Navigation.PushAsync(new ContentPage());

		await Task.Delay(1);

		Navigation.RemovePage(removeMe);
		await Navigation.PopAsync();


		await Task.Delay(1);
		layout.Children.Add(
			new Label()
			{
				Text = "If app hasn't crashed test has succeeded",
				AutomationId = "Success"
			});

	}
}
