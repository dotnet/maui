using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Gestures)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 59863, "TapGestureRecognizer extremely finicky", PlatformAffected.Android,
		issueTestNumber: 1)]
	public class Bugzilla59863_1 : TestContentPage
	{
		int _doubleTaps;
		const string DoubleTapBoxId = "doubleTapView";

		const string Doubles = "double(s)";

		protected override void Init()
		{
			var instructions = new Label
			{
				Text = "Tap the box below once. The counter should not increment. "
						+ "Double tap the box. The counter should increment."
			};

			var doubleTapCounter = new Label { Text = $"{_doubleTaps} {Doubles} on {DoubleTapBoxId}" };

			var doubleTapBox = new BoxView
			{
				WidthRequest = 100,
				HeightRequest = 100,
				BackgroundColor = Colors.Chocolate,
				AutomationId = DoubleTapBoxId
			};

			var doubleTap = new TapGestureRecognizer
			{
				NumberOfTapsRequired = 2,
				Command = new Command(() =>
				{
					_doubleTaps = _doubleTaps + 1;
					doubleTapCounter.Text = $"{_doubleTaps} {Doubles} on {DoubleTapBoxId}";
				})
			};

			doubleTapBox.GestureRecognizers.Add(doubleTap);

			Content = new StackLayout
			{
				Margin = 40,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				Children = { instructions, doubleTapBox, doubleTapCounter }
			};
		}

#if UITEST
		[Test]
		public void SingleTapWithOnlyDoubleTapRecognizerShouldRegisterNothing()
		{
			RunningApp.WaitForElement(DoubleTapBoxId);
			RunningApp.Tap(DoubleTapBoxId);

			RunningApp.WaitForElement($"0 {Doubles} on {DoubleTapBoxId}");
		}

		[Test]
		public void DoubleTapWithOnlyDoubleTapRecognizerShouldRegisterOneDoubleTap()
		{
			RunningApp.WaitForElement(DoubleTapBoxId);
			RunningApp.DoubleTap(DoubleTapBoxId);

			RunningApp.WaitForElement($"1 {Doubles} on {DoubleTapBoxId}");
		}
#endif
	}
}