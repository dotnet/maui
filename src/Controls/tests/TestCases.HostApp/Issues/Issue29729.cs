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
        button.Clicked += OnLowerButtonClicked;

		var button1 = new Button
		{
			Text = "Upper",
			AutomationId = "UpperCaseButton"
		};

		button1.Clicked += OnUpperButtonClicked;

        var layout = new VerticalStackLayout
        {
            Children =
            {
                radioButton,
                button,
				button1
            }
        };

        Content = layout;
    }

    void OnLowerButtonClicked(object sender, EventArgs e)
    {
        radioButton.TextTransform = TextTransform.Lowercase;
    }

	void OnUpperButtonClicked(object sender, EventArgs e)
	{
		radioButton.TextTransform = TextTransform.Uppercase;
	}
}