#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue34563 : _IssuesUITest
	{
		public override string Issue => "[iOS] Vertical SafeAreaEdge isn't respected when Top and Bottom constraints mismatch";

		public Issue34563(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.SafeAreaEdges)]
		public void SafeAreaEdgesRespectedWhenTopAndBottomMismatch()
		{
			const string initialStatus = "Parent: Top=None, Bottom=Container | Child: Top=Container";
			App.WaitForElement("TopMarker");
			Assert.That(App.WaitForTextToBePresentInElement("SafeAreaStatusLabel", initialStatus), Is.True);

			var rootTop = App.WaitForElement("RootGrid").GetRect().Y;
			var singleTopInset = App.WaitForElement("TopMarker").GetRect().Y - rootTop;
			Assert.That(singleTopInset, Is.GreaterThan(5),
				"The child must apply the top safe area even though its parent handles only Bottom.");

			App.Tap("ToggleChildTopButton");
			Assert.That(
				App.WaitForTextToBePresentInElement(
					"SafeAreaStatusLabel",
					"Parent: Top=None, Bottom=Container | Child: Top=None"),
				Is.True);

			var noTopInset = App.WaitForElement("TopMarker").GetRect().Y - rootTop;
			Assert.That(singleTopInset - noTopInset, Is.GreaterThan(5),
				"Removing the child's top safe area must move the marker to the no-inset position.");

			App.Tap("ToggleChildTopButton");
			Assert.That(App.WaitForTextToBePresentInElement("SafeAreaStatusLabel", initialStatus), Is.True);

			var restoredSingleInset = App.WaitForElement("TopMarker").GetRect().Y - rootTop;
			Assert.That(restoredSingleInset, Is.EqualTo(singleTopInset).Within(3),
				"Restoring child-only top handling must restore exactly one safe-area inset.");

			App.Tap("ToggleParentTopButton");
			Assert.That(
				App.WaitForTextToBePresentInElement(
					"SafeAreaStatusLabel",
					"Parent: Top=Container, Bottom=Container | Child: Top=Container"),
				Is.True);

			var parentAndChildInset = App.WaitForElement("TopMarker").GetRect().Y - rootTop;
			Assert.That(parentAndChildInset, Is.EqualTo(singleTopInset).Within(3),
				"When both parent and child request Top, ancestor arbitration must prevent double padding.");
		}
	}
}
#endif
