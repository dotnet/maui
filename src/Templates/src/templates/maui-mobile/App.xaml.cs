namespace MauiApp._1;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
#if (UseSampleContent)
		RequestedThemeChanged += OnRequestedThemeChanged;
#endif
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
#if (UseSampleContent)
		return new Window(new AppShell())
		{
			StatusBarTheme = GetStatusBarTheme(RequestedTheme)
		};
#else
		return new Window(new AppShell());
#endif
	}

#if (UseSampleContent)
	private static StatusBarTheme GetStatusBarTheme(AppTheme theme) => theme switch
	{
		AppTheme.Light => StatusBarTheme.Light,
		AppTheme.Dark => StatusBarTheme.Dark,
		_ => StatusBarTheme.Default
	};

	private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
	{
		foreach (var window in Windows)
			window.StatusBarTheme = GetStatusBarTheme(e.RequestedTheme);
	}
#endif
}