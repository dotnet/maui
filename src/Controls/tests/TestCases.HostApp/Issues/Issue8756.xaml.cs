namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8756, "3 dots in menu is in black in dark theme in Windows 10, but white in Windows 11", PlatformAffected.UWP | PlatformAffected.Android)]
public partial class Issue8756 : ContentPage
{
	bool _useCustomIconColor = false;

	public Issue8756()
	{
		InitializeComponent();
	}

	void OnToggleIconColorClicked(object sender, EventArgs e)
	{
		_useCustomIconColor = !_useCustomIconColor;

		if (_useCustomIconColor)
		{
			// Set a custom icon color - this should make the overflow button visible in dark themes
			if (Parent is NavigationPage navPage)
			{
				NavigationPage.SetIconColor(navPage, Colors.White);
				StatusLabel.Text = "IconColor: White";
			}
		}
		else
		{
			// Clear icon color to use default
			if (Parent is NavigationPage navPage)
			{
				NavigationPage.SetIconColor(navPage, null);
				StatusLabel.Text = "IconColor: Default";
			}
		}
	}
}