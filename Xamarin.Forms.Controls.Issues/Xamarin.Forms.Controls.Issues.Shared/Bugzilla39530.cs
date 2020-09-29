using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest.Queries;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Gestures)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif

	[Preserve(AllMembers = true)]
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

			var frame = new Frame
			{
				HasShadow = false,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				BackgroundColor = Color.White,
				Padding = new Thickness(5),
				HeightRequest = 300,
				WidthRequest = 300,
				AutomationId = "frame"
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
				BackgroundColor = Color.Olive,
				Children = { taps, pans, pinches, frame }
			};
		}

#if UITEST
		[Test]
#if __MACOS__
		[Ignore("UITest.Desktop doesn't return empty NSView yet so it can't find the frame")]
#endif
		public void Bugzilla39530PanTest()
		{
			// Got to wait for the element to be visible to the UI test framework, otherwise we get occasional 
			// index out of bounds exceptions if the query for the frame and its Rect run quickly enough
			RunningApp.WaitForElement(q => q.Marked("frame"));
			AppRect frameBounds = RunningApp.Query (q => q.Marked ("frame"))[0].Rect;
			RunningApp.Pan (new Drag (frameBounds, frameBounds.CenterX, frameBounds.Y + 10, frameBounds.X + 100, frameBounds.Y + 100, Drag.Direction.LeftToRight));

			RunningApp.WaitForElement (q => q.Marked ("Panning: Completed"));
		}

		[Test]
#if __MACOS__
		[Ignore("UITest.Desktop doesn't return empty NSView yet so it can't find the frame")]
#endif
		public void Bugzilla39530PinchTest()
		{
			RunningApp.PinchToZoomIn ("frame");
			RunningApp.WaitForElement(q => q.Marked("Pinching: Completed"));
		}

		[Test]
#if __MACOS__
		[Ignore("UITest.Desktop doesn't return empty NSView yet so it can't find the frame")]
#endif
		public void Bugzilla39530TapTest()
		{
			RunningApp.WaitForElement (q => q.Marked ("frame"));
			RunningApp.Tap ("frame");
			RunningApp.WaitForElement (q => q.Marked ("Taps: 1"));
			RunningApp.Tap ("frame");
			RunningApp.WaitForElement (q => q.Marked ("Taps: 2"));
		}
#endif
	}
}