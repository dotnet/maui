namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7290, "[Android] DisplayActionSheet or DisplayAlert in OnAppearing does not work on Shell",
	PlatformAffected.Android)]
public class Issue7290 : TestShell
{
	protected override void Init()
	{
		ContentPage displayAlertPage = new ContentPage()
		{
			Content = new StackLayout()
			{
				new Label{ Text = "If you did not see an alert this test has failed."},
			},
			Title = "Display Alert"
		};

		displayAlertPage.Appearing += async (_, __) =>
		{
			await displayAlertPage.DisplayAlertAsync("Title", "Close Alert", "Cancel");
			this.CurrentItem = Items[1];
		};


		ContentPage actionSheetPage = new ContentPage()
		{
			Content = new StackLayout()
			{
				new Label{ Text = "If you did not see an Alert then an Action Sheet this test has failed"},
			},
			Title = "Display Action Sheet"
		};

		actionSheetPage.Appearing += async (_, __) =>
		{
			await actionSheetPage.DisplayActionSheetAsync("Title", "Cancel", "Close Action Sheet", "Button");
		};

		AddContentPage(displayAlertPage);
		AddContentPage(actionSheetPage);
	}
}
