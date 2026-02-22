using Font = Microsoft.Maui.Graphics.Font;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 18679, "Canvas.GetStringSize() is not consistent with actual string size in GraphicsView", PlatformAffected.iOS | PlatformAffected.macOS)]
    public class Issue18679 : TestContentPage
    {
        const string ShortText = "ShortText";
        const string LongText = "CiaomondorowfdskleCiaomondorowfdsk";
        const string MultiLineText = "HELLO, WORLD!\nCiao mondo row 2\nGuten tag!?àèìòù@";

        protected override void Init()
        {
            var layout = new VerticalStackLayout
            {
                Spacing = 10,
            };
            var label = new Label
            {
                Text = "The drawn text should not overflow the bounding rectangle.",
                FontSize = 16,
                AutomationId = "18679DescriptionLabel",
            };
            layout.Add(label);

            // Test Case 1: Single long line (basic case)
            layout.Add(CreateDrawable("Test Case 1: Short text", ShortText));

            // Test Case 2: Multi-line text
            layout.Add(CreateDrawable("Test Case 2: Multi-line Text", MultiLineText));

            // Test Case 3: Unicode/non-Latin text
            layout.Add(CreateDrawable("Test Case 3: Long Text", LongText));

            Content = layout;
        }

        private Border CreateDrawable(string testName, string text)
        {
            var graphicsView = new GraphicsView
            {
                HeightRequest = 150,
                Drawable = new Issue18679_Drawable(text, testName)
            };

            return new Border
            {
                Content = graphicsView,
                Stroke = Colors.LightGray,
                StrokeThickness = 1,
            };
        }
    }

    public class Issue18679_Drawable : IDrawable
    {
        readonly string _text;
        readonly string _label;
        static readonly Font Font = Font.Default;
        const int FontSize = 20;


        public Issue18679_Drawable(string text, string label)
        {
            _text = text;
            _label = label;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.SaveState();

            // Draw label
            canvas.FontSize = 14;
            canvas.Font = Font;
            canvas.FontColor = Colors.Blue;
            canvas.DrawString(_label, 10, 10, dirtyRect.Width - 20, 20,
                HorizontalAlignment.Left, VerticalAlignment.Top);

            // Set up for text measurement
            canvas.FontSize = FontSize;
            canvas.Font = Font;

            // Get text size and create bounds
            var stringSize = canvas.GetStringSize(_text, Font, FontSize);

            // Draw the actual text first
            canvas.FontColor = Colors.Black;
            canvas.DrawString(_text, 2, 40, dirtyRect.Width, dirtyRect.Height,
                HorizontalAlignment.Left, VerticalAlignment.Top);

            // Draw the measured bounds rectangle
            canvas.StrokeColor = Colors.Red;
            canvas.StrokeSize = 1;
            canvas.DrawRectangle(2, 40, stringSize.Width, stringSize.Height);

            canvas.RestoreState();
        }
    }
}