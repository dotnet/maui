namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 35477, "Tapped event does not fire when added to Frame in Android AppCompat",
		PlatformAffected.Android)]
	public class Bugzilla35477 : TestContentPage
	{
		protected override void Init()
		{
			var instructions = new Label
			{
				Text = "Tap the frame below. The label with the text 'No taps yet' should change its text to 'Frame was tapped'."
			};
			var frame = new Frame() { };
			var frameLabel = new Label() { AutomationId = "TapHere", Text = "Tap here" };

			frame.Content = new StackLayout() { Children = { frameLabel } };

			var label = new Label { Text = "No taps yet" };

			var rec = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
			rec.Tapped += (s, e) => { label.Text = "Frame was tapped"; };
			frame.GestureRecognizers.Add(rec);

			Content = new StackLayout
			{
				Children = { instructions, frame, label }
			};
		}
	}
}