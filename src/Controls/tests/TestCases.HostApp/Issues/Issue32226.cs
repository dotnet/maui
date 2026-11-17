namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32226, "TapGestureRecognizer does not fire when an Android View.Touch handler sets Handled to false", PlatformAffected.Android)]
public class Issue32226 : ContentPage
{
	public Issue32226()
	{
		var nativeTouchLabel = new Label
		{
			Text = "Native touch not received",
			AutomationId = "NativeTouchLabel"
		};

		var resultLabel = new Label
		{
			Text = "Not tapped",
			AutomationId = "ResultLabel"
		};

		var tapTarget = new Label
		{
			Text = "Tap me (native Touch handler)",
			AutomationId = "TapTarget",
			BackgroundColor = Colors.LightBlue,
			Padding = 15
		};

		var tapGesture = new TapGestureRecognizer();
		tapGesture.Tapped += (s, e) => resultLabel.Text = "Tapped";
		tapTarget.GestureRecognizers.Add(tapGesture);
		ObserveNativeTouches(tapTarget, () => nativeTouchLabel.Text = "Native touch received");

		var doubleTapResultLabel = new Label
		{
			Text = "Not double tapped",
			AutomationId = "DoubleTapResultLabel"
		};

		var doubleTapTarget = new Label
		{
			Text = "Double tap me (native Touch handler)",
			AutomationId = "DoubleTapTarget",
			BackgroundColor = Colors.LightPink,
			Padding = 15
		};

		var doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
		doubleTapGesture.Tapped += (s, e) => doubleTapResultLabel.Text = "Double tapped";
		doubleTapTarget.GestureRecognizers.Add(doubleTapGesture);
		ObserveNativeTouches(doubleTapTarget);

		var plainResultLabel = new Label
		{
			Text = "Not tapped",
			AutomationId = "PlainResultLabel"
		};

		var plainTapTarget = new Label
		{
			Text = "Tap me (no native Touch handler)",
			AutomationId = "PlainTapTarget",
			BackgroundColor = Colors.LightGreen,
			Padding = 15
		};

		var plainTapGesture = new TapGestureRecognizer();
		plainTapGesture.Tapped += (s, e) => plainResultLabel.Text = "Tapped";
		plainTapTarget.GestureRecognizers.Add(plainTapGesture);

		// Once disabled, a view with its own tap gesture must let the parent's gesture handle the tap
		var parentResultLabel = new Label
		{
			Text = "Parent not tapped",
			AutomationId = "ParentResultLabel"
		};

		var childTapTarget = new Label
		{
			Text = "Tap me after disabling (inside a tappable parent)",
			AutomationId = "ChildTapTarget",
			Padding = 15
		};
		childTapTarget.GestureRecognizers.Add(new TapGestureRecognizer());

		var parent = new Grid
		{
			BackgroundColor = Colors.LightGray,
			Padding = 10,
			Children = { childTapTarget }
		};

		var parentTapGesture = new TapGestureRecognizer();
		parentTapGesture.Tapped += (s, e) => parentResultLabel.Text = "Parent tapped";
		parent.GestureRecognizers.Add(parentTapGesture);

		var disableChildButton = new Button
		{
			Text = "Disable the child",
			AutomationId = "DisableChildButton"
		};
		disableChildButton.Clicked += (s, e) => childTapTarget.IsEnabled = false;

		// A view whose ACTION_DOWN our Touch listener never consumes must keep passing taps to the parent.
		// It uses a drop gesture: a long press consumes ACTION_DOWN once #40219 is in, and a pointer
		// gesture does whenever no ancestor is clickable.
		var dropParentResultLabel = new Label
		{
			Text = "Parent not tapped",
			AutomationId = "DropParentResultLabel"
		};

		var dropOnlyChild = new Label
		{
			Text = "Tap me (only a drop gesture, inside a tappable parent)",
			AutomationId = "DropOnlyChild",
			Padding = 15
		};
		dropOnlyChild.GestureRecognizers.Add(new DropGestureRecognizer());

		var dropParent = new Grid
		{
			BackgroundColor = Colors.LightYellow,
			Padding = 10,
			Children = { dropOnlyChild }
		};

		var dropParentTapGesture = new TapGestureRecognizer();
		dropParentTapGesture.Tapped += (s, e) => dropParentResultLabel.Text = "Parent tapped";
		dropParent.GestureRecognizers.Add(dropParentTapGesture);

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children =
			{
				tapTarget,
				nativeTouchLabel,
				resultLabel,
				doubleTapTarget,
				doubleTapResultLabel,
				plainTapTarget,
				plainResultLabel,
				disableChildButton,
				parent,
				parentResultLabel,
				dropParent,
				dropParentResultLabel
			}
		};
	}

	// Same pattern as the workaround from https://github.com/dotnet/maui/issues/23901:
	// observe touches on the platform view without consuming them.
	static void ObserveNativeTouches(View view, Action onDown = null)
	{
#if ANDROID
		view.HandlerChanged += (s, e) =>
		{
			if (view.Handler?.PlatformView is Android.Views.View platformView)
			{
				platformView.Touch += (sender, args) =>
				{
					if (args.Event?.Action == Android.Views.MotionEventActions.Down)
						onDown?.Invoke();

					args.Handled = false;
				};
			}
		};
#endif
	}
}
