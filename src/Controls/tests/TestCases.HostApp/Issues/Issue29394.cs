namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29394, "On Android Shadows should not be rendered over fully transparent areas of drawn shapes", PlatformAffected.Android)]
public class Issue29394 : TestContentPage
{
    protected override void Init()
    {
        var verticalStackLayout = new VerticalStackLayout()
        {
            Padding = new Thickness(30, 0),
            Spacing = 25,
            VerticalOptions = LayoutOptions.Center
        };

        var graphicsView = new GraphicsView()
        {
            HeightRequest = 500,
            WidthRequest = 400,
            Drawable = new Issue29394_Drawable()
        };
        var label = new Label()
        {
            Text = "This is a test for Shadows should not be rendered over fully transparent areas of drawn shapes.",
            AutomationId = "label",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        verticalStackLayout.Children.Add(graphicsView);
        verticalStackLayout.Children.Add(label);
        Content = verticalStackLayout;
    }
}

public class Issue29394_Drawable : IDrawable
{
    Rect fillRect = new Rect(0, 0, 300, 300);

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();

        float centerX = (dirtyRect.Width - (float)fillRect.Width) / 2;
        float centerY = (dirtyRect.Height - (float)fillRect.Height) / 2;
        var outerRect = new RectF(centerX, centerY, (float)fillRect.Width, (float)fillRect.Height);

        canvas.SetFillPaint(Colors.Transparent.AsPaint(), outerRect);
        canvas.SetShadow(new SizeF(0, 15), 4, Color.FromArgb("#59000000"));
        canvas.FillEllipse(outerRect.X, outerRect.Y, outerRect.Width, outerRect.Height);

        float arcSize = 250f;
        float arcX = outerRect.X + (outerRect.Width - arcSize) / 2;
        float arcY = outerRect.Y + (outerRect.Height - arcSize) / 2;

        canvas.FillColor = Colors.Blue;
        canvas.FillCircle(arcX + arcSize / 2, arcY + arcSize / 2, 15);
        canvas.DrawArc(arcX, arcY, arcSize, arcSize, 45, 90, true, false);

        canvas.StrokeColor = Colors.Transparent;
        canvas.StrokeSize = 2;
        canvas.DrawLine(0, 25, dirtyRect.Width, 25);

        canvas.FontColor = Colors.Transparent;
        canvas.DrawString("Shadow should not be Rendered", dirtyRect.Width / 2, 10, HorizontalAlignment.Center);

        canvas.RestoreState();
    }
}