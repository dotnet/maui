namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20596, "[Android] Button with corner radius shadow broken on Android device", PlatformAffected.Android)]
public partial class Issue20596 : ContentPage
{
	public Issue20596()
	{
		var button = new Button
		{
			TextColor = Colors.Black,
			HeightRequest = 200,
			WidthRequest = 200,
			BackgroundColor = Colors.Green,
			CornerRadius = 20,
			Shadow = new Shadow { Radius = 10 }
		};

		var imageButton = new ImageButton
		{
			HeightRequest = 200,
			WidthRequest = 200,
			BackgroundColor = Colors.Red,
			CornerRadius = 20,
			Shadow = new Shadow { Radius = 10 }
		};

		var updateButton = new Button
		{
			Text = "Update corner radius",
			HeightRequest = 50,
			AutomationId = "UpdateCornerRadiusButton"
		};
		updateButton.Clicked += (sender, e) =>
		{
			button.CornerRadius = 100;
			imageButton.CornerRadius = 100;
		};

		Content = new VerticalStackLayout
		{
			Children =
				{
					button,
					imageButton,
					updateButton
				}
		};
	}
}