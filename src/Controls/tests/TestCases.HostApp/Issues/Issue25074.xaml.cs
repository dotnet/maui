namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 25074, "Buttons update size when text or image change", PlatformAffected.iOS)]
public partial class Issue25074 : ContentPage
{
	bool toggle = true;
	public Issue25074()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		if (toggle)
		{
			Button1.Text = "Give the button a much longer title";
			Button2.Text = "Give the button a much longer title";
			Button3.Text = "small";
			Button4.Text = "small";
			Button5.ImageSource = "dotnet_bot_resized3.png";
			Button6.ImageSource = "dotnet_bot_resized.png";
			Button7.ImageSource = "dotnet_bot_resized3.png";
			Button8.ImageSource = "dotnet_bot_resized.png";

			Button9.Text = "Give the button a much longer title";
			Button9.ImageSource = "dotnet_bot_resized3.png";

			Button10.Text = "small";
			Button10.ImageSource = "dotnet_bot_resized.png";
		}
		else
		{
			Button1.Text = "small";
			Button2.Text = "small";
			Button3.Text = "Start with an even longer title";
			Button4.Text = "Start with an even longer title";
			Button5.ImageSource = "dotnet_bot_resized.png";
			Button6.ImageSource = "dotnet_bot_resized3.png";
			Button7.ImageSource = "dotnet_bot_resized.png";
			Button8.ImageSource = "dotnet_bot_resized3.png";

			Button9.Text = "small";
			Button9.ImageSource = "dotnet_bot_resized.png";

			Button10.Text = "Start with an even longer title";
			Button10.ImageSource = "dotnet_bot_resized3.png";
		}

		toggle = !toggle;
	}
}
