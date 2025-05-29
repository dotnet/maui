namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29999, "RadioButton TextTransform Property Does Not Apply on Android and Windows Platforms", PlatformAffected.All)]
public class Issue29999 : ContentPage
{
	RadioButton radioButton;

    public Issue29999()
    {
        radioButton = new RadioButton
		{
			Content = "HelloWorld",
			AutomationId = "radioButton"
        };

        var button = new Button
		{
			Text = "Lower",
			AutomationId = "LowerCaseButton"
        };
        button.Clicked += OnButtonClicked;

        var layout = new VerticalStackLayout
        {
            Children =
            {
                radioButton,
                button
            }
        };

        Content = layout;
    }

    void OnButtonClicked(object sender, EventArgs e)
    {
        radioButton.TextTransform = TextTransform.Lowercase;
    }
}