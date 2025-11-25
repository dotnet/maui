using Microsoft.Maui.Controls.Shapes;
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32731, "Applying Shadow property affects the properties in Visual Transform Matrix", PlatformAffected.Android)]
public class Issue32731 : ContentPage
{
    Border _border;
    double _scaleX = 2;
    double _scaleY = 2;
    double _translationX = 50;
    double _translationY = 50;
    bool _shadowApplied = false;

    public Issue32731()
    {
        _border = new Border
        {
            BackgroundColor = Colors.DeepSkyBlue,
            HeightRequest = 100,
            WidthRequest = 100,
            HorizontalOptions = LayoutOptions.Center,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Content = new Label
            {
                Text = "Test",
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };

        var btnScaleX = CreateButton("ScaleX", OnScaleXClicked, "ScaleXButton");
        var btnScaleY = CreateButton("ScaleY", OnScaleYClicked, "ScaleYButton");
        var btnTranslationX = CreateButton("TranslationX", OnTranslationXClicked, "TranslationXButton");
        var btnTranslationY = CreateButton("TranslationY", OnTranslationYClicked, "TranslationYButton");
        var btnShadow = CreateButton("Shadow", OnToggleShadowClicked, "ToggleShadowButton");
        var btnReset = CreateButton("Reset", OnResetClicked, "ResetButton");

        _border.HorizontalOptions = LayoutOptions.Center;
        _border.VerticalOptions = LayoutOptions.Center;
        Grid.SetRow(_border, 0);

        var buttonsScrollView = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 10,
                Children =
                {
                    new HorizontalStackLayout
                    {
                        Spacing = 10,
                        HorizontalOptions = LayoutOptions.Center,
                        Children = {btnScaleX, btnScaleY }
                    },

                    new HorizontalStackLayout
                    {
                        Spacing = 10,
                        HorizontalOptions = LayoutOptions.Center,
                        Children = { btnTranslationX, btnTranslationY }
                    },

                    new HorizontalStackLayout
                    {
                        Spacing = 10,
                        HorizontalOptions = LayoutOptions.Center,
                        Children = { btnShadow, btnReset }
                    }
                }
            }
        };
        Grid.SetRow(buttonsScrollView, 1);

        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto }
            },
            Children = { _border, buttonsScrollView }
        };
    }

    Button CreateButton(string text, EventHandler clicked, string automationId)
    {
        var button = new Button
        {
            Text = text,
            Padding = 5,
            FontSize = 12,
            AutomationId = automationId
        };
        button.Clicked += clicked;
        return button;
    }

    void OnScaleXClicked(object sender, EventArgs e)
    {
        _scaleX += 0.5;
        _border.ScaleX = _scaleX;
    }

    void OnScaleYClicked(object sender, EventArgs e)
    {
        _scaleY += 0.5;
        _border.ScaleY = _scaleY;
    }

    void OnTranslationXClicked(object sender, EventArgs e)
    {
        _translationX += 50;
        _border.TranslationX = _translationX;
    }

    void OnTranslationYClicked(object sender, EventArgs e)
    {
        _translationY += 50;
        _border.TranslationY = _translationY;
    }

    void OnToggleShadowClicked(object sender, EventArgs e)
    {
        if (_shadowApplied)
        {
            _border.Shadow = null;
            _shadowApplied = false;
        }
        else
        {
            _border.Shadow = new Shadow
            {
                Brush = Brush.Red,
                Opacity = 0.9f,
            };
            _shadowApplied = true;
        }
    }

    void OnResetClicked(object sender, EventArgs e)
    {
        _scaleX = 1;
        _scaleY = 1;
        _translationX = 0;
        _translationY = 0;
        _shadowApplied = false;

        _border.ScaleX = _scaleX;
        _border.ScaleY = _scaleY;
        _border.TranslationX = _translationX;
        _border.TranslationY = _translationY;
        _border.Shadow = new Shadow
        {
            Brush = Brush.Transparent,
            Opacity = 0f,
        };
    }
}
