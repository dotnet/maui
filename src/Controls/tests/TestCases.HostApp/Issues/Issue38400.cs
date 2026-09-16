using AndroidSpecific = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38400, "TabbedPage BarBackgroundColor should work with Theme change", PlatformAffected.Android)]
public class Issue38400 : TabbedPage
{
    public Issue38400()
    {
        this.SetAppThemeColor(
              SelectedTabColorProperty,
              Color.FromArgb("#512BD4"),
              Color.FromArgb("#D600AA"));

        this.SetAppThemeColor(
              UnselectedTabColorProperty,
              Color.FromArgb("#D4D12B"),
              Color.FromArgb("#D66000"));

        AndroidSpecific.TabbedPage.SetToolbarPlacement(this, AndroidSpecific.ToolbarPlacement.Bottom);

        for (int tabIndex = 1; tabIndex <= 3; tabIndex++)
        {
            Children.Add(new NavigationPage(CreateContentPage(tabIndex))
            {
                Title = $"Tab {tabIndex}"
            });
        }
    }

    static ContentPage CreateContentPage(int tabIndex)
    {
        var currentThemeLabel = new Label
        {
            AutomationId = $"CurrentThemeLabel{tabIndex}",
            HorizontalOptions = LayoutOptions.Center
        };

        void SetTheme(AppTheme theme)
        {
            Application.Current.UserAppTheme = theme;
            currentThemeLabel.Text = theme.ToString();
        }

        var lightThemeButton = new Button
        {
            AutomationId = $"LightThemeButton{tabIndex}",
            Text = "Light Theme",
            Command = new Command(() => SetTheme(AppTheme.Light))
        };

        var darkThemeButton = new Button
        {
            AutomationId = $"DarkThemeButton{tabIndex}",
            Text = "Dark Theme",
            Command = new Command(() => SetTheme(AppTheme.Dark))
        };

        currentThemeLabel.Text = Application.Current?.RequestedTheme.ToString() ?? AppTheme.Unspecified.ToString();

        var contentLayout = new VerticalStackLayout
        {
            Spacing = 12
        };
        contentLayout.Children.Add(lightThemeButton);
        contentLayout.Children.Add(darkThemeButton);
        contentLayout.Children.Add(currentThemeLabel);

        var contentPage = new ContentPage
        {
            Title = $"Page {tabIndex}",
            Content = contentLayout
        };
        contentPage.SetAppThemeColor(
              BackgroundColorProperty,
              Colors.White,
              Color.FromArgb("#1F1F1F"));

        return contentPage;
    }
}

