#if IOS || MACCATALYST
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
			const string initialStatus = "Parent: Top=None, Bottom=Container | Child: Top=Container, Bottom=Container";
			App.WaitForElement("TopMarker");
			App.WaitForElement("BottomMarker");
			Assert.That(App.WaitForTextToBePresentInElement("SafeAreaStatusLabel", initialStatus), Is.True);

			var root = App.WaitForElement("RootGrid").GetRect();
			var rootTop = root.Y;
			var rootBottom = root.Y + root.Height;
			var singleTopInset = App.WaitForElement("TopMarker").GetRect().Y - rootTop;
			var bottomMarker = App.WaitForElement("BottomMarker").GetRect();
			var singleBottomInset = rootBottom - (bottomMarker.Y + bottomMarker.Height);
#if MACCATALYST
			if (singleTopInset <= 5)
				Assert.Ignore("This MacCatalyst environment does not expose a measurable title-bar safe area.");
#endif
			Assert.That(singleTopInset, Is.GreaterThan(5),
				"The child must apply the top safe area even though its parent handles only Bottom.");
#if !MACCATALYST
			Assert.That(singleBottomInset, Is.GreaterThan(5),
				"The parent must apply the bottom safe area while the child defers that edge.");
#endif

			App.Tap("ToggleParentEdgeButton");
			Assert.That(
				App.WaitForTextToBePresentInElement(
					"SafeAreaStatusLabel",
					"Parent: Top=Container, Bottom=None | Child: Top=Container, Bottom=Container"),
				Is.True);

			App.RetryAssert(() =>
			{
				var parentAndChildInset = App.WaitForElement("TopMarker").GetRect().Y - rootTop;
				Assert.That(parentAndChildInset, Is.EqualTo(singleTopInset).Within(3),
					"When both parent and child request Top, ancestor arbitration must prevent double padding.");
			});
#if !MACCATALYST
			App.RetryAssert(() =>
			{
				var childBottomMarker = App.WaitForElement("BottomMarker").GetRect();
				var childBottomInset = rootBottom - (childBottomMarker.Y + childBottomMarker.Height);
				Assert.That(childBottomInset, Is.EqualTo(singleBottomInset).Within(3),
					"When the parent stops handling Bottom, the child must apply that newly unblocked edge.");
			});
#endif
		}
	}
}
#endif
