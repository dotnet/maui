using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6813, "Reusing the same page for formsheet Modal causes measuring issues", PlatformAffected.iOS)]
public partial class Issue6813 : ContentPage
{
	private static int _presentationCount = 0;
	private double _lastWidth = 0;
	private double _lastHeight = 0;

	public Issue6813()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		
		_presentationCount++;
		StatusLabel.Text = $"Presentation #{_presentationCount}";
		
		// Capture size after a short delay to ensure layout is complete
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
		{
			CaptureSize();
		});
	}

	private void CaptureSize()
	{
		var currentWidth = Width;
		var currentHeight = Height;
		
		Console.WriteLine($"=== MODAL SIZE CAPTURE (Presentation #{_presentationCount}) ===");
		Console.WriteLine($"Width: {currentWidth}, Height: {currentHeight}");
		Console.WriteLine($"View Bounds: {Bounds}");
		
		if (_presentationCount > 1)
		{
			var widthDiff = currentWidth - _lastWidth;
			var heightDiff = currentHeight - _lastHeight;
			Console.WriteLine($"Difference from previous: Width {widthDiff:F2}, Height {heightDiff:F2}");
			
			if (widthDiff < -10 || heightDiff < -10)
			{
				Console.WriteLine("⚠️ WARNING: Modal size decreased significantly!");
			}
		}
		
		_lastWidth = currentWidth;
		_lastHeight = currentHeight;
		
		SizeLabel.Text = $"Size: {currentWidth:F0} x {currentHeight:F0}";
		Console.WriteLine("=== END SIZE CAPTURE ===");
	}

	private async void OnShowModalClicked(object sender, EventArgs e)
	{
		Console.WriteLine($"=== SHOWING MODAL AGAIN (Presentation #{_presentationCount + 1}) ===");
		
		// Reuse the SAME page instance - this should cause the bug
		await Navigation.PushModalAsync(this, true);
	}

	private async void OnCloseModalClicked(object sender, EventArgs e)
	{
		Console.WriteLine("=== CLOSING MODAL ===");
		await Navigation.PopModalAsync(true);
	}
}
