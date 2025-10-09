namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 18011, "RadioButton TextColor for plain Content not working on iOS", PlatformAffected.iOS | PlatformAffected.macOS)]
public partial class Issue18011 : ContentPage
{
    RadioButton radioButton;
    VerticalStackLayout verticalStackLayout;
    public Issue18011()
    {
        var labelStyle = new Style(typeof(Label))
        {
            Setters =
            {
                new Setter { Property = Label.TextColorProperty, Value = Colors.Blue }
            }
        };
        Application.Current.Resources.Add(labelStyle);

        radioButton = new RadioButton
        {
            AutomationId = "Issue18011_RadioButton",
            Content = "The color of this text should be Brown",
            TextColor = Colors.Brown
        };

        verticalStackLayout = new VerticalStackLayout
        {
            Children =
            {
                radioButton
            }
        };

        Content = verticalStackLayout;

    }
}