namespace Maui.Controls.Sample;

public partial class TabbedPageTopTabsTestPage : TabbedPage
{
    private readonly Random _random = new();

    // Color palettes for testing
    private static readonly Color[] BarColors =
    [
        Color.FromArgb("#2196F3"), // Blue
        Color.FromArgb("#6200EE"), // Purple
        Color.FromArgb("#FF5722"), // Deep Orange
        Color.FromArgb("#4CAF50"), // Green
        Color.FromArgb("#E91E63"), // Pink
        Color.FromArgb("#009688"), // Teal
    ];

    private static readonly (Color Selected, Color Unselected)[] TabColorPairs =
    [
        (Color.FromArgb("#FFEB3B"), Color.FromArgb("#90CAF9")), // Yellow / Light Blue
        (Color.FromArgb("#FFFFFF"), Color.FromArgb("#80FFFFFF")), // White / Semi-transparent
        (Color.FromArgb("#FF5722"), Color.FromArgb("#FFCCBC")), // Deep Orange / Light Orange
        (Color.FromArgb("#00FF00"), Color.FromArgb("#90EE90")), // Green / Light Green
        (Color.FromArgb("#E91E63"), Color.FromArgb("#F8BBD9")), // Pink / Light Pink
    ];

    public TabbedPageTopTabsTestPage()
    {
        InitializeComponent();
    }

    async void OnInfoClicked(object? sender, EventArgs e)
    {
        await DisplayAlertAsync("TabbedPage Top Tabs Info",
            "This page tests the TOP tab placement.\n\n" +
            "Top tabs use TabLayout + TabLayoutMediator.\n" +
            "Bottom tabs use BottomNavigationManager (shared with Shell).\n\n" +
            "TabbedPageManager handles both placements, but only bottom tabs share code with Shell's ShellItemHandler.",
            "OK");
    }

    void OnChangeBarColorClicked(object? sender, EventArgs e)
    {
        var newColor = BarColors[_random.Next(BarColors.Length)];
        BarBackgroundColor = newColor;

        Console.WriteLine($"SANDBOX TabbedPage (Top): BarBackgroundColor changed to {newColor.ToHex()}");
    }

    void OnChangeTabColorsClicked(object? sender, EventArgs e)
    {
        var colorPair = TabColorPairs[_random.Next(TabColorPairs.Length)];
        SelectedTabColor = colorPair.Selected;
        UnselectedTabColor = colorPair.Unselected;

        Console.WriteLine($"SANDBOX TabbedPage (Top): SelectedTabColor={colorPair.Selected.ToHex()}, UnselectedTabColor={colorPair.Unselected.ToHex()}");
    }

    void OnBottomTabsClicked(object? sender, EventArgs e)
    {
        Console.WriteLine("SANDBOX TabbedPage: Switching to Bottom Tabs");
        if (Application.Current?.Windows.Count > 0)
        {
            Application.Current.Windows[0].Page = new TabbedPageTestPage();
        }
    }

    void OnTopTabsClicked(object? sender, EventArgs e)
    {
        Console.WriteLine("SANDBOX TabbedPage: Already on Top Tabs");
        // Already on top tabs, just refresh
        if (Application.Current?.Windows.Count > 0)
        {
            Application.Current.Windows[0].Page = new TabbedPageTopTabsTestPage();
        }
    }

    void OnBackToShellClicked(object? sender, EventArgs e)
    {
        Console.WriteLine("SANDBOX TabbedPage: Going back to Shell");
        if (Application.Current?.Windows.Count > 0)
        {
            Application.Current.Windows[0].Page = new SandboxShell();
        }
    }
}
