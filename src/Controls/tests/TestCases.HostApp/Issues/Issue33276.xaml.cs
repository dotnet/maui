namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33276, "Padding not restored after SoftInput closes", PlatformAffected.Android)]
public partial class Issue33276 : ContentPage
{
	public Issue33276()
	{
		InitializeComponent();
		
		// Log initial state
		System.Diagnostics.Debug.WriteLine($"[Issue33276] Initial Padding: {MainContainer.Padding}");
		Console.WriteLine($"[Issue33276] Initial Padding: {MainContainer.Padding}");
		
		// Update padding status when it changes
		MainContainer.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(Grid.Padding))
			{
				var padding = MainContainer.Padding;
				PaddingStatusLabel.Text = $"Padding: L={padding.Left:F0}, T={padding.Top:F0}, R={padding.Right:F0}, B={padding.Bottom:F0}";
				System.Diagnostics.Debug.WriteLine($"[Issue33276] Padding changed: {padding}");
				Console.WriteLine($"[Issue33276] Padding changed: {padding}");
			}
		};
		
		// Also update on layout
		MainContainer.SizeChanged += (s, e) =>
		{
			var padding = MainContainer.Padding;
			PaddingStatusLabel.Text = $"Padding: L={padding.Left:F0}, T={padding.Top:F0}, R={padding.Right:F0}, B={padding.Bottom:F0}";
			System.Diagnostics.Debug.WriteLine($"[Issue33276] SizeChanged - Padding: {padding}");
			Console.WriteLine($"[Issue33276] SizeChanged - Padding: {padding}");
		};
	}
}
