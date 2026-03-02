namespace Maui.Controls.Sample;

public partial class TabbedPageTestPage : TabbedPage
{
    private readonly Random _random = new();

    // Color palettes for testing
    private static readonly Color[] BarColors =
    [
        Color.FromArgb("#6200EE"), // Purple
        Color.FromArgb("#03DAC5"), // Teal
        Color.FromArgb("#FF5722"), // Deep Orange
        Color.FromArgb("#2196F3"), // Blue
        Color.FromArgb("#4CAF50"), // Green
        Color.FromArgb("#E91E63"), // Pink
    ];

    private static readonly (Color Selected, Color Unselected)[] TabColorPairs =
    [
        (Color.FromArgb("#03DAC5"), Color.FromArgb("#BB86FC")), // Teal / Light Purple
        (Color.FromArgb("#FFFFFF"), Color.FromArgb("#80FFFFFF")), // White / Semi-transparent
        (Color.FromArgb("#FFD700"), Color.FromArgb("#FFA500")), // Gold / Orange
        (Color.FromArgb("#00FF00"), Color.FromArgb("#90EE90")), // Green / Light Green
        (Color.FromArgb("#FF1493"), Color.FromArgb("#FFB6C1")), // Deep Pink / Light Pink
    ];

    public TabbedPageTestPage()
    {
        InitializeComponent();
    }

    async void OnInfoClicked(object? sender, EventArgs e)
    {
        await DisplayAlertAsync("TabbedPage Test Info",
            "This page tests the BottomNavigationManager refactoring.\n\n" +
            "Both Shell's ShellItemHandler and TabbedPage's TabbedPageManager now use the shared BottomNavigationManager class for:\n" +
            "• Tab setup and menu creation\n" +
            "• Tab selection callbacks\n" +
            "• Color management\n" +
            "• Background updates",
            "OK");
    }

    void OnChangeBarColorClicked(object? sender, EventArgs e)
    {
        var newColor = BarColors[_random.Next(BarColors.Length)];
        BarBackgroundColor = newColor;

        Console.WriteLine($"SANDBOX TabbedPage: BarBackgroundColor changed to {newColor.ToHex()}");
    }

    void OnChangeTabColorsClicked(object? sender, EventArgs e)
    {
        var colorPair = TabColorPairs[_random.Next(TabColorPairs.Length)];
        SelectedTabColor = colorPair.Selected;
        UnselectedTabColor = colorPair.Unselected;

        Console.WriteLine($"SANDBOX TabbedPage: SelectedTabColor={colorPair.Selected.ToHex()}, UnselectedTabColor={colorPair.Unselected.ToHex()}");
    }

    void OnBottomTabsClicked(object? sender, EventArgs e)
    {
        Console.WriteLine("SANDBOX TabbedPage: Switching to Bottom Tabs");
        if (Application.Current?.Windows.Count > 0)
        {
            // Create new TabbedPage with Bottom placement (default in XAML)
            Application.Current.Windows[0].Page = new TabbedPageTestPage();
        }
    }

    void OnTopTabsClicked(object? sender, EventArgs e)
    {
        Console.WriteLine("SANDBOX TabbedPage: Switching to Top Tabs");
        if (Application.Current?.Windows.Count > 0)
        {
            // Create TabbedPage with Top placement
            Application.Current.Windows[0].Page = new TabbedPageTopTabsTestPage();
        }
    }
}
