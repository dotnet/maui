using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
	[Category(UITestCategories.Gestures)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 59863, "TapGestureRecognizer extremely finicky", PlatformAffected.Android, 
		issueTestNumber: 2)]
	public class Bugzilla59863_2 : TestContentPage
	{
		int _mixedSingleTaps;
		int _mixedDoubleTaps;
		const string MixedTapBoxId = "mixedTapView";

		const string Singles = "singles(s)";
		const string Doubles = "double(s)";

		protected override void Init()
		{
			var instructions = new Label
			{
				Text = "Tap the box below once. The single tap counter should increment. " 
				+ "Double tap the box. The double tap counter should increment, " 
				+ "but the single tap counter should not."
			};

			var mixedSingleTapCounter = new Label {Text = $"{_mixedSingleTaps} {Singles}"};
			var mixedDoubleTapCounter = new Label {Text = $"{_mixedDoubleTaps} {Doubles}"};

			var mixedTapBox = new BoxView
			{
				WidthRequest = 100,
				HeightRequest = 100,
				BackgroundColor = Color.Coral,
				AutomationId = MixedTapBoxId
			};

			var mixedDoubleTap = new TapGestureRecognizer
			{
				NumberOfTapsRequired = 2,
				Command = new Command(() =>
				{
					_mixedDoubleTaps = _mixedDoubleTaps + 1;
					mixedDoubleTapCounter.Text = $"{_mixedDoubleTaps} {Doubles} on {MixedTapBoxId}";
				})
			};

			var mixedSingleTap = new TapGestureRecognizer
			{
				NumberOfTapsRequired = 1,
				Command = new Command(() =>
				{
					_mixedSingleTaps = _mixedSingleTaps + 1;
					mixedSingleTapCounter.Text = $"{_mixedSingleTaps} {Singles} on {MixedTapBoxId}";
				})
			};

			mixedTapBox.GestureRecognizers.Add(mixedDoubleTap);
			mixedTapBox.GestureRecognizers.Add(mixedSingleTap);

			Content = new StackLayout
			{	
				Margin = 40,
				HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill,
				Children = { instructions, mixedTapBox, mixedSingleTapCounter, mixedDoubleTapCounter }
			};
		}

#if UITEST
		[Test]
		public void DoubleTapWithMixedRecognizersShouldRegisterDoubleTap()
		{
			RunningApp.WaitForElement(MixedTapBoxId);
			RunningApp.DoubleTap(MixedTapBoxId);

			RunningApp.WaitForElement($"1 {Doubles} on {MixedTapBoxId}");
		}

		[Test]
		public void SingleTapWithMixedRecognizersShouldRegisterSingleTap()
		{
			RunningApp.WaitForElement(MixedTapBoxId);
			RunningApp.Tap(MixedTapBoxId);

			RunningApp.WaitForElement($"1 {Singles} on {MixedTapBoxId}");
		}
#endif
	}
}