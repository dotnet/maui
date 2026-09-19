#if (UseSampleContent)
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Font = Microsoft.Maui.Font;
#endif
namespace MauiApp._1;

public partial class AppShell : Shell
{
#if (UseSampleContent)
	private bool _updatingThemeSelection = true;
#endif

	public AppShell()
	{
		InitializeComponent();
#if (UseSampleContent)
		UpdateThemeSelection(Application.Current!.RequestedTheme);
		Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
//-:cnd:noEmit
#if ANDROID || WINDOWS
		SemanticProperties.SetDescription(ThemeSegmentedControl, "Theme selection");
#endif
//+:cnd:noEmit
#endif
	}
#if (UseSampleContent)
	internal static void UpdateBackButtonAccessibility(ContentPage page)
	{
		var stack = page.Navigation.NavigationStack;
		var destination = stack.Count > 1 ? stack[^2] : null;
		// Shell's root stack entry can be null; its ShellContent has the stable destination title.
		var destinationTitle = destination is Pages.ProjectDetailPage
			? "project"
			: Current.CurrentItem?.CurrentItem?.CurrentItem?.Title;
		var behavior = GetBackButtonBehavior(page) ?? new BackButtonBehavior();
		behavior.AccessibilityLabel = string.IsNullOrEmpty(destinationTitle) ? "Back" : $"Back to {destinationTitle}";
		SetBackButtonBehavior(page, behavior);
	}

	private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e) =>
		UpdateThemeSelection(e.RequestedTheme);

	private void UpdateThemeSelection(AppTheme theme)
	{
		_updatingThemeSelection = true;
		try
		{
			ThemeSegmentedControl.SelectedIndex = theme == AppTheme.Dark ? 1 : 0;
		}
		finally
		{
			_updatingThemeSelection = false;
		}
	}

	public static async Task DisplaySnackbarAsync(string message)
	{
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

		var snackbarOptions = new SnackbarOptions
		{
			BackgroundColor = Color.FromArgb("#FF3300"),
			TextColor = Colors.White,
			ActionButtonTextColor = Colors.Yellow,
			CornerRadius = new CornerRadius(0),
			Font = Font.SystemFontOfSize(18),
			ActionButtonFont = Font.SystemFontOfSize(14)
		};

		var snackbar = Snackbar.Make(message, visualOptions: snackbarOptions);

		await snackbar.Show(cancellationTokenSource.Token);
	}

	public static async Task DisplayToastAsync(string message)
	{
		// Toast is currently not working in MCT on Windows
		if (OperatingSystem.IsWindows())
			return;

		var toast = Toast.Make(message, textSize: 18);

		var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
		await toast.Show(cts.Token);
	}

	private void SfSegmentedControl_SelectionChanged(object? sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
	{
		if (_updatingThemeSelection || e.NewIndex is not (0 or 1))
			return;

		Application.Current!.UserAppTheme = e.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
	}
#endif
}
