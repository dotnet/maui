namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 25829, "ScrollView starts at the position of first Entry control on the bottom rather than at 0",
	PlatformAffected.Android)]
public partial class Issue25829 : ContentPage
{
	public Issue25829()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		if (this.Navigation.ModalStack.Contains(this))
		{
			testButton.AutomationId = "Success";
		}
		else
		{
			testButton.AutomationId = "PushModal";
		}
	}

	public async void OnButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PushModalAsync(new Issue25829());
	}
}