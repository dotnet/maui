namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36505,
    "[iOS] Span TapGestureRecognizer hitbox is mispositioned inside FormattedString when the font contains a STAT table (line-height/leading mismatch)",
    PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue36505 : ContentPage
{
    public Issue36505()
    {
        var statusLabel = new Label
        {
            AutomationId = "StatusLabel",
            Text = "Not tapped",
        };

        void OnSpanTapped(object sender, TappedEventArgs e)
        {
            statusLabel.Text = "Success";
        }

        var fs = new FormattedString();

        fs.Spans.Add(new Span
        {
            Text = "Line 1.\nLine 2.\nLine 3.\nLine 4.\nLine 5.\n"
                 + "Line 6.\nLine 7.\nLine 8.\nLine 9.\nLine 10.\n"
                 + "Line 11.\nLine 12.\nLine 13.\nLine 14.\nLine 15.\n",
            FontFamily = "MyCustomFont",
            FontSize = 16,
        });

        var tappableSpan = new Span
        {
            Text = "Click me",
            FontFamily = "MyCustomFont",
            FontSize = 16,
            TextColor = Colors.Blue,
            TextDecorations = TextDecorations.Underline,
        };
        var tapRecognizer = new TapGestureRecognizer();
        tapRecognizer.Tapped += OnSpanTapped;
        tappableSpan.GestureRecognizers.Add(tapRecognizer);
        fs.Spans.Add(tappableSpan);

        var spanLabel = new Label
        {
            AutomationId = "SpanLabel",
            FormattedText = fs,
            LineBreakMode = LineBreakMode.WordWrap,
        };

        var lineRefLabel = new Label
        {
            AutomationId = "LineRef",
            Text = "Reference line",
            FontFamily = "MyCustomFont",
            FontSize = 16,
        };

        Content = new VerticalStackLayout
        {
            Padding = new Thickness(20),
            Spacing = 20,
            Children = { spanLabel, lineRefLabel, statusLabel }
        };
    }
}
