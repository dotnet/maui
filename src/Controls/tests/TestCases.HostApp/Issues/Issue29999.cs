namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29729, "RadioButton TextTransform Property Does Not Apply on Android and Windows Platforms", PlatformAffected.All)]
public class Issue29729 : ContentPage
{
	RadioButton radioButton;

    public Issue29729()
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