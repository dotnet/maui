#if (IncludeSampleContent)
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Font = Microsoft.Maui.Font;
#endif
namespace MauiApp._1;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
#if (IncludeSampleContent)
		var currentTheme = Application.Current!.RequestedTheme;		
		ThemeSegmentedControl.SelectedIndex = currentTheme == AppTheme.Light ? 0 : 1;
#endif
#if ANDROID || WINDOWS
		SemanticProperties.SetDescription(ThemeSegmentedControl, "Theme selection");
#endif
	}
#if (IncludeSampleContent)
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
		Application.Current!.UserAppTheme = e.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
    }
#endif
}
