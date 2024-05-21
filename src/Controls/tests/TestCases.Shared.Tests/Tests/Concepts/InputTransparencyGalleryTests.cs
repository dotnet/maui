using Maui.Controls.Sample;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
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
		public void InputTransparencySimple([Values] Test.InputTransparency test) =>
			RunTest(test.ToString(), !test.ToString().EndsWith("InputBlocked"));

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

			RunTest(key, clickable || passthru);
		}

		static void RunTest(string test, bool updatable)
		{
			var remote = new EventViewContainerRemote(UITestContext, test);
			remote.GoTo(test.ToString());

			var textBeforeClick = remote.GetEventLabel().GetText();
			ClassicAssert.AreEqual($"Event: {test} (none)", textBeforeClick);

			remote.TapView();

			var textAfterClick = remote.GetEventLabel().GetText();

			if (updatable)
			{
				// if the button is clickable or taps pass through to the base button
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
