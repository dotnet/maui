namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21331, "InputTransparent should not affect background color on Windows layouts", PlatformAffected.UWP)]
public class Issue17389 : TestContentPage
{
    Grid redGrid;
    Grid greenGrid;
    Border blueBorder;
    ContentView purpleContent;
    Label tapCountLabel;
    Label redGridLabel;
    Label greenGridLabel;
    Label blueBorderLabel;
    Label purpleContentLabel;
    int tapCount;

    protected override void Init()
    {
        tapCountLabel = new Label { Text = "Tap count: 0", HorizontalOptions = LayoutOptions.Center };

        redGrid = CreateBackgroundTestGrid(Colors.Red, false, "RedGrid", out redGridLabel);
        greenGrid = CreateBackgroundTestGrid(Colors.Green, false, "GreenGrid", out greenGridLabel);

        blueBorderLabel = new Label { Text = "Blue Border (InputTransparent=False)", HorizontalOptions = LayoutOptions.Center, AutomationId = "BlueBorder" };
        blueBorder = new Border
        {
            BackgroundColor = Colors.Blue,
            InputTransparent = false,
            WidthRequest = 200,
            HeightRequest = 100,
            Content = blueBorderLabel
        };
        AddTapGesture(blueBorder);

        purpleContentLabel = new Label { Text = "Purple Content (InputTransparent=False)", AutomationId = "PurpleContent" };
        purpleContent = new ContentView
        {
            BackgroundColor = Colors.Purple,
            InputTransparent = false,
            WidthRequest = 200,
            HeightRequest = 100,
            Content = purpleContentLabel
        };
        AddTapGesture(purpleContent);

        Content = CreateMainContent();
    }

    ScrollView CreateMainContent()
    {
        return new ScrollView
        {
            Content = new StackLayout
            {
                Spacing = 20,
                Children =
                {
                    new Label
                    {
                        Text = "InputTransparent Background Test",
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    tapCountLabel,
                    new Button
                    {
                        Text = "Toggle InputTransparent",
                        Command = new Command(ToggleInputTransparent),
                        AutomationId = "ToggleInputTransparentButton"
                    },
                    new Button
                    {
                        Text = "Toggle Background Colors",
                        Command = new Command(ToggleBackgroundColors),
                        AutomationId = "ToggleBackgroundColorsButton"
                    },
                    redGrid,
                    greenGrid,
                    blueBorder,
                    purpleContent
                }
            }
        };
    }

    Grid CreateBackgroundTestGrid(Color bgColor, bool inputTransparent, string labelText, out Label label)
    {
        label = new Label { Text = $"{labelText} (InputTransparent=False)", HorizontalOptions = LayoutOptions.Center, AutomationId = $"{labelText}" };

        Grid childGrid = new Grid
        {
            BackgroundColor = bgColor,
            InputTransparent = inputTransparent,
            Children = { label }
        };

        AddTapGesture(childGrid);

        Grid parentGrid = new Grid
        {
            WidthRequest = 200,
            HeightRequest = 100,
            InputTransparent = inputTransparent,
            BackgroundColor = Colors.LightGray,
            Children = { childGrid }
        };

        AddTapGesture(parentGrid);
        return parentGrid;
    }

    void AddTapGesture(View view)
    {
        view.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() =>
            {
                tapCount++;
                tapCountLabel.Text = $"Tap count: {tapCount}";
            })
        });
    }

    void ToggleInputTransparent()
    {
        tapCountLabel.Text = $"Tap count: {0}";
        redGrid.InputTransparent = !redGrid.InputTransparent;
        greenGrid.InputTransparent = !greenGrid.InputTransparent;
        blueBorder.InputTransparent = !blueBorder.InputTransparent;
        purpleContent.InputTransparent = !purpleContent.InputTransparent;

        redGridLabel.Text = $"Red Grid (InputTransparent={redGrid.InputTransparent})";
        greenGridLabel.Text = $"Green Grid (InputTransparent={greenGrid.InputTransparent})";
        blueBorderLabel.Text = $"Blue Border (InputTransparent={blueBorder.InputTransparent})";
        purpleContentLabel.Text = $"Purple Content (InputTransparent={purpleContent.InputTransparent})";
    }

    void ToggleBackgroundColors()
    {
        redGrid.BackgroundColor = Colors.Orange;
        greenGrid.BackgroundColor = Colors.LightGreen;
        blueBorder.BackgroundColor = Colors.LightBlue;
        purpleContent.BackgroundColor = Colors.Pink;
    }
}