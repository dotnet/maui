using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.InputTransparent)]
	public class InputTransparencyGalleryTests : CoreGalleryBasePageTest
	{
		const string ButtonGallery = "Input Transparency Gallery";

		public override string GalleryPageName => ButtonGallery;
		
		public InputTransparencyGalleryTests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(ButtonGallery);
		}

		[Test]
		public void InputTransparencySimple([Values] Test.InputTransparency test) => RunTest(test.ToString());

		[Test]
		[Combinatorial]
		public void InputTransparencyWhenRootIsTransparentMatrix([Values] bool rootCascade, [Values] bool nestedTrans, [Values] bool nestedCascade, [Values] bool trans)
		{
			var (clickable, passthru) = Test.InputTransparencyMatrix.States[(true, rootCascade, nestedTrans, nestedCascade, trans)];
			var key = Test.InputTransparencyMatrix.GetKey(true, rootCascade, nestedTrans, nestedCascade, trans, clickable, passthru);

			RunTest(key, clickable, passthru);
		}

		[Test]
		[Combinatorial]
		public void InputTransparencyWhenRootIsNotTransparentMatrix([Values] bool rootCascade, [Values] bool nestedTrans, [Values] bool nestedCascade, [Values] bool trans)
		{
			var (clickable, passthru) = Test.InputTransparencyMatrix.States[(false, rootCascade, nestedTrans, nestedCascade, trans)];
			var key = Test.InputTransparencyMatrix.GetKey(false, rootCascade, nestedTrans, nestedCascade, trans, clickable, passthru);

			RunTest(key, clickable, passthru);
		}

#if !WINDOWS

		[Test]
		[Combinatorial]
		public void ScrollViewInputTransparencySimpleMatrix([Values] bool rootTrans, [Values] bool rootCascade, [Values] bool trans)
		{
			var (clickable, passthru) = Test.InputTransparencyMatrix.SimpleStates[(rootTrans, rootCascade, trans)];
			var key = Test.InputTransparencyMatrix.GetSimpleKey("ScrollView", rootTrans, rootCascade, trans, clickable, passthru);

			// ScrollView is not really a layout so the "broken" rules don't apply
			RunTest(key, clickable, passthru, androidIsBroken: false);
		}

		[Test]
		[Combinatorial]
		public void ContentViewInputTransparencySimpleMatrix([Values] bool rootTrans, [Values] bool rootCascade, [Values] bool trans)
		{
			var (clickable, passthru) = Test.InputTransparencyMatrix.SimpleStates[(rootTrans, rootCascade, trans)];
			var key = Test.InputTransparencyMatrix.GetSimpleKey("ContentView", rootTrans, rootCascade, trans, clickable, passthru);

			RunTest(key, clickable, passthru);
		}

#endif

		void RunTest(string test, bool? clickable = null, bool? passthru = null, bool androidIsBroken = true)
		{
			var remote = new EventViewContainerRemote(UITestContext, test);
			remote.GoTo(test.ToString());

			var textBeforeClick = remote.GetEventLabel().GetText();
			ClassicAssert.AreEqual($"Event: {test} (none)", textBeforeClick);

			remote.TapView();

			var textAfterClick = remote.GetEventLabel().GetText();

			if (clickable is null || passthru is null)
			{
				// some tests are really basic so have no need for fancy checks
				ClassicAssert.AreEqual($"Event: {test} (SUCCESS 1)", textAfterClick);
			}
			else if (clickable == true || passthru == true)
			{
				// if the button is clickable or taps pass through to the base button
				ClassicAssert.AreEqual($"Event: {test} (SUCCESS 1)", textAfterClick);
			}
			else if (androidIsBroken && Device == TestDevice.Android)
			{
				// TODO: Android is broken with everything passing through so we just use that
				// to test the bottom button was clickable
				// https://github.com/dotnet/maui/issues/10252
				ClassicAssert.AreEqual($"Event: {test} (SUCCESS 1)", textAfterClick);
			}
			else
			{
				// sometimes nothing can happen, so try test that
				Task.Delay(500).Wait(); // just make sure that nothing happened

				textAfterClick = remote.GetEventLabel().GetText();
				ClassicAssert.AreEqual($"Event: {test} (none)", textBeforeClick);
			}
		}
	}
}
