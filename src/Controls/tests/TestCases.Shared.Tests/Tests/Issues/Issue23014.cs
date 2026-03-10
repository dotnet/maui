using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23014 : _IssuesUITest
	{
		public Issue23014(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "App crashes when calling ItemsView.ScrollTo on unloaded CollectionView";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ScrollToOnUnloadedCollectionViewShouldNotCrash()
		{
			App.WaitForElement("ScrollToRemovedButton");
			App.Click("ScrollToRemovedButton");
			App.WaitForElement("StatusLabel");
			Assert.That(App.FindElement("StatusLabel").GetText(), Is.EqualTo("Success"));
		}
	}
}

