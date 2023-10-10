using Maui.Controls.Sample;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public class InputTransparencyGalleryTests : CoreGalleryBasePageTest
	{
		const string ButtonGallery = "Input Transparency Gallery";

		public InputTransparencyGalleryTests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(ButtonGallery);
		}

		[Test]
		public void Simple([Values] Test.InputTransparency test) => RunTest(test.ToString());

		[Test]
		[Combinatorial]
		public void Matrix([Values] bool rootTrans, [Values] bool rootCascade, [Values] bool nestedTrans, [Values] bool nestedCascade, [Values] bool trans)
		{
			var (clickable, passthru) = Test.InputTransparencyMatrix.States[(rootTrans, rootCascade, nestedTrans, nestedCascade, trans)];
			var key = Test.InputTransparencyMatrix.GetKey(rootTrans, rootCascade, nestedTrans, nestedCascade, trans, clickable, passthru);

			RunTest(key, clickable, passthru);
		}

		void RunTest(string test, bool? clickable = null, bool? passthru = null)
		{
			var remote = new EventViewContainerRemote(UITestContext, test);
			remote.GoTo(test.ToString());

			var textBeforeClick = remote.GetEventLabel().GetText();
			Assert.AreEqual($"Event: {test} (none)", textBeforeClick);

			remote.TapView();

			var textAfterClick = remote.GetEventLabel().GetText();

			if (clickable is null || passthru is null)
			{
				// some tests are really basic so have no need for fancy checks
				Assert.AreEqual($"Event: {test} (SUCCESS 1)", textAfterClick);
			}
			else if (clickable == true || passthru == true)
			{
				// if the button is clickable or taps pass through to the base button
				Assert.AreEqual($"Event: {test} (SUCCESS 1)", textAfterClick);
			}
			else if (Device == TestDevice.Android)
			{
				// TODO: Android is broken with everything passing through so we just use that
				// to test the bottom button was clickable
				// https://github.com/dotnet/maui/issues/10252
				Assert.AreEqual($"Event: {test} (SUCCESS 1)", textAfterClick);
			}
			else
			{
				// sometimes nothing can happen, so try test that
				Task.Delay(500).Wait(); // just make sure that nothing happened

				textAfterClick = remote.GetEventLabel().GetText();
				Assert.AreEqual($"Event: {test} (none)", textBeforeClick);
			}
		}
	}
}
