#nullable enable

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20903, "Double-tap behavior should work correctly when adding a new double-tap handler", PlatformAffected.UWP)]
public partial class Issue20903 : ContentPage
{
	private int _callbackCalledCount;

	public Issue20903()
	{
		InitializeComponent();
	}

	private void OnLabelBeingDoubleTapped(object? sender, TappedEventArgs e)
	{
		_callbackCalledCount++;
		eventCountLabel.Text = _callbackCalledCount.ToString();
	}

	private void addDoubleTapHandlerButton_Clicked(object? sender, EventArgs e)
	{
		TapGestureRecognizer doubleTapGestureRecognizer = new();
		doubleTapGestureRecognizer.Tapped += OnLabelBeingDoubleTapped;
		doubleTapGestureRecognizer.NumberOfTapsRequired = 2;
		myLabel.GestureRecognizers.Add(doubleTapGestureRecognizer);
	}
}
