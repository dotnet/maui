using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6407, "NumberOfTapsRequired fails when greater than 2", PlatformAffected.Android | PlatformAffected.UWP)]
public partial class Issue6407 : ContentPage
{
	int _singleTapCount = 0;
	int _doubleTapCount = 0;
	int _tripleTapCount = 0;
	int _quadrupleTapCount = 0;
	int _quintupleTapCount = 0;

	public Issue6407()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		// Initialization is handled in XAML and constructor
	}

	void OnSingleTap(object sender, EventArgs e)
	{
		_singleTapCount++;
		SingleTapCounter.Text = $"Single taps: {_singleTapCount}";
	}

	void OnDoubleTap(object sender, EventArgs e)
	{
		_doubleTapCount++;
		DoubleTapCounter.Text = $"Double taps: {_doubleTapCount}";
	}

	void OnTripleTap(object sender, EventArgs e)
	{
		_tripleTapCount++;
		TripleTapCounter.Text = $"Triple taps: {_tripleTapCount}";
	}

	void OnQuadrupleTap(object sender, EventArgs e)
	{
		_quadrupleTapCount++;
		QuadrupleTapCounter.Text = $"Quadruple taps: {_quadrupleTapCount}";
	}

	void OnQuintupleTap(object sender, EventArgs e)
	{
		_quintupleTapCount++;
		QuintupleTapCounter.Text = $"Quintuple taps: {_quintupleTapCount}";
	}

	void OnResetCounters(object sender, EventArgs e)
	{
		_singleTapCount = 0;
		_doubleTapCount = 0;
		_tripleTapCount = 0;
		_quadrupleTapCount = 0;
		_quintupleTapCount = 0;

		SingleTapCounter.Text = "Single taps: 0";
		DoubleTapCounter.Text = "Double taps: 0";
		TripleTapCounter.Text = "Triple taps: 0";
		QuadrupleTapCounter.Text = "Quadruple taps: 0";
		QuintupleTapCounter.Text = "Quintuple taps: 0";
	}
}