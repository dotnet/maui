namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33904, "CharacterSpacing applied to Label is not inherited by Span elements in FormattedString", PlatformAffected.All)]
public class Issue33904 : ContentPage
{
    public Issue33904()
    {
        Label directLabel = new Label
        {
            Text = "DIRECT CHARACTER SPACING",
            CharacterSpacing = 4,
            FontAttributes = FontAttributes.Bold
        };

        Label overrideLabel = new Label
        {
            CharacterSpacing = 14,
            FontSize = 16,
            FormattedText = new FormattedString
            {
                Spans =
                {
                    new Span { Text = "Individual ", CharacterSpacing = 0 },
                    new Span { Text = "span overrides", CharacterSpacing = 6 }
                }
            }
        };

        Label inheritedLabel = new Label
        {
            AutomationId="InheritedCharacterSpacingLabel",
            CharacterSpacing = 4,
            TextColor = Colors.Purple,
            FontSize = 16,
            FormattedText = new FormattedString
            {
                Spans =
                {
                    new Span { Text = "Inherited " },
                    new Span { Text = "character ", FontAttributes = FontAttributes.Italic },
                    new Span { Text = "spacing ", TextColor = Colors.Orange },
                    new Span { Text = "from " },
                    new Span { Text = "parent ", FontAttributes = FontAttributes.Bold },
                    new Span { Text = "label"}
                }
            }
        };
        
        VerticalStackLayout stackLayout = new VerticalStackLayout
        {
            Spacing = 16,
            Padding = new Thickness(20)
        };

        stackLayout.Add(directLabel);
        stackLayout.Add(overrideLabel);
        stackLayout.Add(inheritedLabel);

        Content = stackLayout;
    }
}