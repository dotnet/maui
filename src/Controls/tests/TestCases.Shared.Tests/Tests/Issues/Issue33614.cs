using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue33614 : _IssuesUITest
    {
        public override string Issue => "CollectionView Scrolled event reports incorrect FirstVisibleItemIndex after programmatic ScrollTo";

        public Issue33614(TestDevice device) : base(device) { }

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void FirstVisibleItemIndexShouldBeCorrectAfterScrollTo()
        {
            App.WaitForElement("ScrollToButton");
            App.Tap("ScrollToButton");
            var firstIndexText = App.FindElement("FirstIndexLabel").GetText();
            Assert.That(firstIndexText, Is.EqualTo("FirstVisibleItemIndex: 15"));
        }
    }
}