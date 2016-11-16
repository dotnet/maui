
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 968, "StackLayout does not relayout on device rotation", PlatformAffected.iOS, NavigationBehavior.PushModalAsync)]
	public class Issue968 : TestContentPage
	{
		protected override void Init ()
		{
			Title = "Nav Bar";

			var layout = new StackLayout {
				Padding = new Thickness (20),
				BackgroundColor = Color.Gray
			};

			layout.Children.Add (new BoxView {
				BackgroundColor = Color.Red,
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand
			});

			layout.Children.Add (new Label {
				Text = "You should see me after rotating"
			});

			Content = layout;
		}

#if UITEST
		[Test]
		[Description ("Verify the layout lays out on rotations")]
		[UiTest (typeof(StackLayout))]
		public void Issue968TestsRotationRelayoutIssue ()
		{
			RunningApp.SetOrientationLandscape ();
			RunningApp.Screenshot ("Rotated to Landscape");
			RunningApp.WaitForElement (q => q.Marked ("You should see me after rotating"));
			RunningApp.Screenshot ("StackLayout in Modal respects rotation");
			RunningApp.SetOrientationPortrait ();
		}
#endif

	}
}
