namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36697, "Button with CharacterSpacing does not restore TextColor to platform default when reset to null", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue36697 : ContentPage
{
    Button _buttonWithCharacterSpacing;
    public Issue36697()
    {
        _buttonWithCharacterSpacing = new Button
        {
            Text = "Sample Button",
            AutomationId = "ButtonWithCharacterSpacing",

        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(24),
                Spacing = 14,
                Children =
                {
                    _buttonWithCharacterSpacing,
                    new Button
                    {
                        Text = "Dynamically change the TextColor",
                        AutomationId = "SetTextColorButton",
                        Command = new Command(() =>
                        {
                            _buttonWithCharacterSpacing.TextColor = Colors.DarkOrange;
                        })
                    },

                    new Button
                    {
                        Text = "Dynamically change the CharacterSpacing",
                        AutomationId = "SetCharacterSpacingButton",
                        Command = new Command(() =>
                        {
                            _buttonWithCharacterSpacing.CharacterSpacing = 15;
                        })
                    },
                    new Button
                    {
                        Text = "Reset TextColor to Null",
                        AutomationId = "ResetTextColorButton",
                        Command = new Command(() =>
                        {
                            _buttonWithCharacterSpacing.TextColor = null;
                        })
                    }
                }
            }
        };
    }
}
