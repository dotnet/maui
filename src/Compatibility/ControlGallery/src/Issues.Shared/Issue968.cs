
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 968, "StackLayout does not relayout on device rotation", PlatformAffected.iOS, NavigationBehavior.PushModalAsync)]
	public class Issue968 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Nav Bar";

			var layout = new StackLayout
			{
				Padding = new Thickness(20),
				BackgroundColor = Colors.Gray,
				AutomationId = "TestReady"
			};

			layout.Children.Add(new BoxView
			{
				BackgroundColor = Colors.Red,
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand
			});

			layout.Children.Add(new Label
			{
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
			RunningApp.WaitForElement("TestReady");
			RunningApp.SetOrientationLandscape ();
			RunningApp.Screenshot ("Rotated to Landscape");
			RunningApp.WaitForElement (q => q.Marked ("You should see me after rotating"));
			RunningApp.Screenshot ("StackLayout in Modal respects rotation");
			RunningApp.SetOrientationPortrait ();
		}
#endif

	}
}
