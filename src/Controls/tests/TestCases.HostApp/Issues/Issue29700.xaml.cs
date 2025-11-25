namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29700, "TextTransform Property Does Not Apply at Runtime When TextType is equal to Html")]
public partial class Issue29700 : ContentPage
{
	public Issue29700()
	{
		InitializeComponent();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		if (TestLabel.TextTransform == TextTransform.Default)
			TestLabel.TextTransform = TextTransform.Uppercase;
		else if (TestLabel.TextTransform == TextTransform.Uppercase)
			TestLabel.TextTransform = TextTransform.Lowercase;
		else
			TestLabel.TextTransform = TextTransform.None;
	}

}
