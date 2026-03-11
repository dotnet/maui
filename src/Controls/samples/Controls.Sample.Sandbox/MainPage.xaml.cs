using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	int _clickCount;

	public MainPage()
	{
		InitializeComponent();
		UpdateVersionInfo();
	}

	void OnTestButtonClicked(object? sender, EventArgs e)
	{
		_clickCount++;
		ClickCountLabel.Text = $"Clicks: {_clickCount}";
	}

	void UpdateVersionInfo()
	{
		// Show whether incremental hot reload infrastructure is active
		var registryType = typeof(XamlComponentRegistry);
		var hasRegistry = registryType != null;

		// Check if __version field exists (generated when EnableMauiIncrementalHotReload=true)
		var versionField = GetType().GetField("__version",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var hasVersionField = versionField != null;
		var currentVersion = hasVersionField ? versionField!.GetValue(this) : "N/A";

		// Check if UpdateComponent method exists
		var ucMethod = GetType().GetMethod("UpdateComponent",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
			null, Type.EmptyTypes, null);
		var hasUC = ucMethod != null;

		VersionLabel.Text = $"Registry: {(hasRegistry ? "✅" : "❌")} | " +
			$"__version field: {(hasVersionField ? "✅" : "❌")} (={currentVersion}) | " +
			$"UpdateComponent: {(hasUC ? "✅" : "❌")}";
	}
}