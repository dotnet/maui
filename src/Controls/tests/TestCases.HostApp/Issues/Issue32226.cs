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
			Text = "Tap me (native Touch handler sets Handled = false)",
			AutomationId = "TapTarget",
			BackgroundColor = Colors.LightBlue,
			Padding = 20
		};

		var tapGesture = new TapGestureRecognizer();
		tapGesture.Tapped += (s, e) => resultLabel.Text = "Tapped";
		tapTarget.GestureRecognizers.Add(tapGesture);

#if ANDROID
		// Same pattern as the workaround from https://github.com/dotnet/maui/issues/23901:
		// observe touches on the platform view without consuming them.
		tapTarget.HandlerChanged += (s, e) =>
		{
			if (tapTarget.Handler?.PlatformView is Android.Views.View platformView)
			{
				platformView.Touch += (sender, args) =>
				{
					if (args.Event?.Action == Android.Views.MotionEventActions.Down)
						nativeTouchLabel.Text = "Native touch received";

					args.Handled = false;
				};
			}
		};
#endif

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
			Padding = 20
		};

		var plainTapGesture = new TapGestureRecognizer();
		plainTapGesture.Tapped += (s, e) => plainResultLabel.Text = "Tapped";
		plainTapTarget.GestureRecognizers.Add(plainTapGesture);

		// A disabled view with its own tap gesture must still let the parent's gesture handle the tap
		var parentResultLabel = new Label
		{
			Text = "Parent not tapped",
			AutomationId = "ParentResultLabel"
		};

		var disabledTapTarget = new Label
		{
			Text = "Tap me (disabled, inside a tappable parent)",
			AutomationId = "DisabledTapTarget",
			IsEnabled = false,
			Padding = 20
		};
		disabledTapTarget.GestureRecognizers.Add(new TapGestureRecognizer());

		var parent = new Grid
		{
			BackgroundColor = Colors.LightGray,
			Padding = 10,
			Children = { disabledTapTarget }
		};

		var parentTapGesture = new TapGestureRecognizer();
		parentTapGesture.Tapped += (s, e) => parentResultLabel.Text = "Parent tapped";
		parent.GestureRecognizers.Add(parentTapGesture);

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				tapTarget,
				nativeTouchLabel,
				resultLabel,
				plainTapTarget,
				plainResultLabel,
				parent,
				parentResultLabel
			}
		};
	}
}
