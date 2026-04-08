namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 18011,
    "RadioButton TextColor for plain Content not working on iOS when Label styles present", PlatformAffected.iOS)]
public class Issue18011 : ContentPage
{
    ResourceDictionary _styleDictionary;

    public Issue18011()
    {
        // Add an implicit Label style to Application resources to simulate global App.xaml styles.
        // The bug only reproduces when global Label styles exist that set TextColor.
        _styleDictionary = new ResourceDictionary();
        _styleDictionary.Add(new Style(typeof(Label))
        {
            Setters =
            {
                new Setter { Property = Label.TextColorProperty, Value = Colors.Green }
            }
        });

        Application.Current.Resources.MergedDictionaries.Add(_styleDictionary);

        Content = new VerticalStackLayout
        {
            Padding = new Thickness(20),
            Spacing = 20,
            Children =
            {
                new RadioButton
                {
                    AutomationId = "RadioButtonWithTextColor",
                    Content = "Red RadioButton Text",
                    TextColor = Colors.Red,
                    FontSize = 20,
                    IsChecked = true
                },
                new RadioButton
                {
                    AutomationId = "RadioButtonDefault",
                    Content = "Default RadioButton Text",
                    FontSize = 20
                }
            }
        };
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Clean up global styles to avoid affecting other tests
        Application.Current?.Resources?.MergedDictionaries?.Remove(_styleDictionary);
    }
}
