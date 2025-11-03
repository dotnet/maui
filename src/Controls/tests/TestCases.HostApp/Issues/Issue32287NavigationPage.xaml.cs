namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32287, "Using custom TitleView in NavigationPage causes content to be covered in iOS 26", PlatformAffected.iOS)]
public partial class Issue32287NavigationPage : NavigationPage
{
	public Issue32287NavigationPage()
	{
		InitializeComponent();
	}

	private void OnIncreaseFontSizeClicked(object sender, EventArgs e)
	{
		if (TitleLabel != null)
		{
			TitleLabel.FontSize += 5;
		}
	}

	private void OnDecreaseFontSizeClicked(object sender, EventArgs e)
	{
		if (TitleLabel != null && TitleLabel.FontSize > 10)
		{
			TitleLabel.FontSize -= 5;
		}
	}
}
