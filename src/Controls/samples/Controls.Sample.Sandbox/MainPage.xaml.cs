namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		Application.Current!.UserAppTheme = AppTheme.Light;
		ThemeStatus.Text = "Light theme";
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Console.WriteLine($"SANDBOX 40201: MainPage appeared; Controls={typeof(Border).Assembly.FullName}; OS={DeviceInfo.VersionString}");
		Console.WriteLine($"SANDBOX 40201: zero={ZeroStrokeBorder.StrokeThickness}; positive={PositiveStrokeBorder.StrokeThickness}");
		LogBorder(ZeroStrokeBorder);
		LogBorder(PositiveStrokeBorder);
	}

	void OnToggleTheme(object? sender, EventArgs e)
	{
		var app = Application.Current!;
		app.UserAppTheme = app.UserAppTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
		ThemeStatus.Text = $"{app.UserAppTheme} theme";
		Console.WriteLine($"SANDBOX 40201: theme switched to {app.UserAppTheme}");
		LogBorder(ZeroStrokeBorder);
		LogBorder(PositiveStrokeBorder);
	}

	static void LogBorder(Border border)
	{
		Console.WriteLine($"SANDBOX 40201: {border.AutomationId}; background={border.Background}; color={(border.Background as SolidColorBrush)?.Color}; backgroundColor={border.BackgroundColor}; shape={border.StrokeShape}");
	}
}