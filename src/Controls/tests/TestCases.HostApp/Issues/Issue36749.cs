#if IOS || MACCATALYST
using Microsoft.Maui.Handlers;
using UIKit;
#endif

namespace Maui.Controls.Sample.Issues;

// Regression: ButtonHandler.iOS.cs (introduced in 10.0.80 via PR #33346) unconditionally reset
// BackgroundColor to UIColor.Clear when the cross-platform Background was null. This wiped native
// colors set by a custom UIButton subclass returned from CreatePlatformView().
[Issue(IssueTracker.Github, 36749,
    "ButtonHandler clears native platform-view styling set by custom handlers when Background/TextColor are null",
    PlatformAffected.iOS)]
public class Issue36749 : ContentPage
{
    public const string ResultLabelId = "Issue36749Result";

    readonly Label _resultLabel;
    readonly Button _button;

    public Issue36749()
    {
        _resultLabel = new Label
        {
            AutomationId = ResultLabelId,
            Text = "Checking...",
            HorizontalTextAlignment = TextAlignment.Center
        };

        // Issue36749Button routes to Issue36749ButtonHandler (registered in MauiProgram.cs).
        // The handler's CreatePlatformView() returns a UIButton subclass that self-styles with
        // BackgroundColor = UIColor.Cyan. No cross-platform Background is set on this button.
        _button = new Issue36749Button
        {
            Text = "Test Button",
            AutomationId = "Issue36749Button"
        };

        Content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 20,
            Children = { _button, _resultLabel }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

#if IOS || MACCATALYST
		if (_button.Handler?.PlatformView is UIButton platformButton)
		{
			// The native UIButton subclass sets BackgroundColor = UIColor.Cyan in its constructor.
			// With the regression: initial null-Background mapping resets it to UIColor.Clear.
			// With the fix:        the Window-null check skips the reset, preserving Cyan.
			platformButton.BackgroundColor.GetRGBA(out var r, out var g, out var b, out var a);

			// UIColor.Cyan = R:0, G:1, B:1, A:1
			bool isCyan = r < 0.01 && g > 0.99 && b > 0.99 && a > 0.99;
			_resultLabel.Text = isCyan ? "PASS" : "FAIL";
		}
#endif
    }
}

// Custom Button subtype used purely to route to Issue36749ButtonHandler.
public class Issue36749Button : Button { }

#if IOS || MACCATALYST

// Simulates a design-system UIButton subclass that self-styles in its constructor.
// The cross-platform Button intentionally leaves Background and TextColor unset.
class Issue36749NativeStyledButton : UIButton
{
	public Issue36749NativeStyledButton() : base(UIButtonType.System)
	{
		BackgroundColor = UIColor.Cyan;
		SetTitleColor(UIColor.DarkGray, UIControlState.Normal);
	}
}

class Issue36749ButtonHandler : ButtonHandler
{
	protected override UIButton CreatePlatformView() => new Issue36749NativeStyledButton();
}

#endif
