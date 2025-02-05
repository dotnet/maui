namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 39530, "Frames do not handle pan or pinch gestures under AppCompat", PlatformAffected.Android)]
public class Bugzilla39530 : TestContentPage
{
	protected override void Init()
	{
		var taps = new Label { Text = "Taps: 0" };
		var pans = new Label();
		var pinches = new Label();

		var pangr = new PanGestureRecognizer();
		var tapgr = new TapGestureRecognizer();
		var pinchgr = new PinchGestureRecognizer();

		var frameLabel = new Label
		{
			Text = "Frame Content",
			AutomationId = "frameLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		var frame = new Frame
		{
			HasShadow = false,
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill,
			BackgroundColor = Colors.White,
			Padding = new Thickness(5),
			HeightRequest = 300,
			WidthRequest = 300,
			AutomationId = "frame",
			Content = frameLabel
		};

		var tapCount = 0;

		tapgr.Command = new Command(() =>
		{
			tapCount += 1;
			taps.Text = $"Taps: {tapCount}";
		});

		pangr.PanUpdated += (sender, args) => pans.Text = $"Panning: {args.StatusType}";

		pinchgr.PinchUpdated += (sender, args) => pinches.Text = $"Pinching: {args.Status}";

		frame.GestureRecognizers.Add(tapgr);
		frame.GestureRecognizers.Add(pangr);
		frame.GestureRecognizers.Add(pinchgr);

		Content = new StackLayout
		{
			BackgroundColor = Colors.Olive,
			Children = { taps, pans, pinches, frame }
		};
	}
}