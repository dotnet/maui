using AppTheme = Microsoft.Maui.ApplicationModel.AppTheme;

namespace Microsoft.Maui.Controls.HybridWebApps.UI;

/// <summary>Native application theme reads/preferences; the annotated Application event supplies notifications.</summary>
public sealed class ApplicationProvider(HybridWindowHost host)
{
	[HybridAction("application.theme", MainThread = true)]
	internal ApplicationTheme Theme() => Read();

	[HybridAction("application.setTheme", MainThread = true)]
	internal ApplicationTheme SetTheme(AppTheme theme)
	{
		host.Application.UserAppTheme = theme;
		return Read();
	}
	ApplicationTheme Read()
	{
		var app = host.Application;
		return new(app.RequestedTheme, app.UserAppTheme);
	}
}
sealed record ApplicationTheme(AppTheme Effective, AppTheme Preference);
