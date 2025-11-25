using System.Collections.ObjectModel;
 
namespace Maui.Controls.Sample.Issues;
 
[Issue(IssueTracker.Github, 32724, "Applying Shadow property affects the properties in Visual Transform Matrix", PlatformAffected.iOS | PlatformAffected.macOS)]
 
public class Issue32724 : ContentPage
{
    Border _border;
    double _scale = 1;
    double _scaleX = 1;
    double _scaleY = 1;
    double _translationX = 0;
    double _translationY = 0;
    double _rotation = 0;
    double _rotationX = 0;
    double _rotationY = 0;
    double _anchorX = 0.5;
    double _anchorY = 0.5;
    bool _shadowApplied = false;
 
    public Issue32724()
    {
        _border = new Border
        {
            BackgroundColor = Colors.Red,
            HeightRequest = 120,
            WidthRequest = 120,
            HorizontalOptions = LayoutOptions.Center
        };
 
        var btnScale = CreateButton("Scale +", OnScaleClicked, "ScaleButton");
        var btnScaleX = CreateButton("ScaleX", OnScaleXClicked, "ScaleXButton");
        var btnScaleY = CreateButton("ScaleY", OnScaleYClicked, "ScaleYButton");
        var btnTranslationX = CreateButton("TranslationX", OnTranslationXClicked, "TranslationXButton");
        var btnTranslationY = CreateButton("TranslationY", OnTranslationYClicked, "TranslationYButton");
        var btnRot = CreateButton("Rotation +", OnRotationClicked, "RotationButton");
        var btnRotX = CreateButton("RotationX +", OnRotationXClicked, "RotationXButton");
        var btnRotY = CreateButton("RotationY +", OnRotationYClicked, "RotationYButton");
        var btnAnchorX = CreateButton("AnchorX +", OnAnchorXClicked, "AnchorXButton");
        var btnAnchorY = CreateButton("AnchorY +", OnAnchorYClicked, "AnchorYButton");
        var btnShadow = CreateButton("Toggle Shadow", OnToggleShadowClicked, "ToggleShadowButton");
        var btnReset = CreateButton("Reset", OnResetClicked, "ResetButton");
 
        var buttonsScrollView = new ScrollView
        {
            VerticalOptions = LayoutOptions.Center,
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 20,
                Children =
                    {
                        new HorizontalStackLayout
                        {
                            Spacing = 10,
                            HorizontalOptions = LayoutOptions.Center,
                            Children = {btnScale,btnScaleX, btnScaleY }
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
                            Children = {btnRot, btnRotX, btnRotY }
                        },
 
                        new HorizontalStackLayout
                        {
                            Spacing = 10,
                            HorizontalOptions = LayoutOptions.Center,
                            Children = { btnAnchorX, btnAnchorY }
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
 
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto }
            }
        };
 
        Grid.SetRow(_border, 0);
        Grid.SetRow(buttonsScrollView, 1);
        grid.Children.Add(_border);
        grid.Children.Add(buttonsScrollView);
        Content = grid;
    }
 
    Button CreateButton(string text, EventHandler clicked, string automationId)
    {
        var button = new Button { Text = text, Padding = 10, AutomationId = automationId };
        button.Clicked += clicked;
        return button;
    }
 
    void OnScaleClicked(object sender, EventArgs e)
    {
        _scale += 0.5;
        _border.Scale = _scale;
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
 
    void OnRotationClicked(object sender, EventArgs e)
    {
        _rotation += 45;
        if (_rotation >= 360)
            _rotation = 0;
        _border.Rotation = _rotation;
    }
 
    void OnRotationXClicked(object sender, EventArgs e)
    {
        _rotationX += 45;
        if (_rotationX >= 360)
            _rotationX = 0;
        _border.RotationX = _rotationX;
    }
 
    void OnRotationYClicked(object sender, EventArgs e)
    {
        _rotationY += 45;
        if (_rotationY >= 360)
            _rotationY = 0;
        _border.RotationY = _rotationY;
    }
 
    void OnAnchorXClicked(object sender, EventArgs e)
    {
        _anchorX += 0.25;
        if (_anchorX > 1)
            _anchorX = 0;
        _border.AnchorX = _anchorX;
    }
 
    void OnAnchorYClicked(object sender, EventArgs e)
    {
        _anchorY += 0.25;
        if (_anchorY > 1)
            _anchorY = 0;
        _border.AnchorY = _anchorY;
    }
 
    void OnToggleShadowClicked(object sender, EventArgs e)
    {
        if (_shadowApplied)
        {
            // Remove shadow completely
            _border.Shadow = null;  // reset to default
            _shadowApplied = false;
        }
        else
        {
            _border.Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.8f,
                Offset = new Point(10, 10)
            };
            _shadowApplied = true;
        }
    }
 
    void OnResetClicked(object sender, EventArgs e)
    {
        _scale = 1;
        _scaleX = 1;
        _scaleY = 1;
        _translationX = 0;
        _translationY = 0;
        _rotation = 0;
        _rotationX = 0;
        _rotationY = 0;
        _anchorX = 0.5;
        _anchorY = 0.5;
        _shadowApplied = false;
 
        _border.Scale = _scale;
        _border.ScaleX = _scaleX;
        _border.ScaleY = _scaleY;
        _border.TranslationX = _translationX;
        _border.TranslationY = _translationY;
        _border.Rotation = _rotation;
        _border.RotationX = _rotationX;
        _border.RotationY = _rotationY;
        _border.AnchorX = _anchorX;
        _border.AnchorY = _anchorY;
        _border.Shadow = new Shadow
        {
            Brush = Brush.Transparent,
            Opacity = 0f,
            Offset = new Point(0, 0)
        };
    }
}