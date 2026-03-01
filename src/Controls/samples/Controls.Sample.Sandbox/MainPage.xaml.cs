namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	void OnUpdateWidgetClicked(object? sender, EventArgs e)
	{
#if IOS || MACCATALYST
#pragma warning disable CA1416
		var emojis = new[] { "🟣", "🔵", "🟢", "🟠", "🔴", "⭐", "🎵", "🚀" };
		var emoji = emojis[Random.Shared.Next(emojis.Length)];
		var title = WidgetTitle.Text ?? "MAUI Sandbox";
		var message = WidgetMessage.Text ?? "Hello!";
		MauiProgram.UpdateWidgetData(title, message, emoji);
		WidgetStatus.Text = $"✅ Updated: {emoji} {title}";
#pragma warning restore CA1416
#endif
	}
}