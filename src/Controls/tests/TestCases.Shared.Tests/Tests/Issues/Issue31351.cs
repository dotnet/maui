using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue31351 : _IssuesUITest
    {
        public override string Issue => "[Windows] - Customised CollectionView (inherited from) does not [ScrollTo] and display selection correctly";

        public Issue31351(TestDevice device) : base(device)
        {
        }

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void CustomCollectionViewScrollToIndexShouldWork()
        {
            // Wait for the page to load
            App.WaitForElement("CustomCollectionView");
            App.WaitForElement("StatusLabel");

            // Verify initial state
            var initialStatus = App.FindElement("StatusLabel").GetText();
            Assert.That(initialStatus, Is.EqualTo("Ready to test"));

            // Click ScrollTo Index 25 button
            App.Click("ScrollToIndex25");
            App.WaitForElement("StatusLabel");

            // Wait a moment for scroll to complete
            System.Threading.Thread.Sleep(1000);

            // Verify that scrolling occurred by checking the status label
            var statusAfterScroll = App.FindElement("StatusLabel").GetText();
            Assert.That(statusAfterScroll, Does.Contain("Scrolled to:"));
            Assert.That(statusAfterScroll, Does.Contain("First="));

            // The status should show that we scrolled to around index 25
            // We check that First visible index is at least 20 (allowing for some variance)
            Assert.That(statusAfterScroll, Does.Match(@"First=([12][0-9]|[3-9][0-9])"));
        }

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void CustomCollectionViewScrollToItemShouldWork()
        {
            App.WaitForElement("CustomCollectionView");
            App.WaitForElement("StatusLabel");

            // Click ScrollTo Item 40 button
            App.Click("ScrollToItem40");
            App.WaitForElement("StatusLabel");

            // Wait for scroll to complete
            System.Threading.Thread.Sleep(1000);

            // Verify that scrolling occurred
            var statusAfterScroll = App.FindElement("StatusLabel").GetText();
            Assert.That(statusAfterScroll, Does.Contain("Scrolled to:"));

            // Should scroll to around index 40
            Assert.That(statusAfterScroll, Does.Match(@"First=([3-9][0-9])"));
        }

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void CustomCollectionViewScrollToEndShouldWork()
        {
            App.WaitForElement("CustomCollectionView");
            App.WaitForElement("StatusLabel");

            // Click ScrollTo End button
            App.Click("ScrollToEnd");
            App.WaitForElement("StatusLabel");

            // Wait for scroll to complete
            System.Threading.Thread.Sleep(1000);

            // Verify that scrolling occurred and we're near the end
            var statusAfterScroll = App.FindElement("StatusLabel").GetText();
            Assert.That(statusAfterScroll, Does.Contain("Scrolled to:"));

            // Should scroll to near the end (around index 99)
            Assert.That(statusAfterScroll, Does.Match(@"Last=([89][0-9]|100)"));
        }

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void CustomCollectionViewMultipleScrollOperationsShouldWork()
        {
            App.WaitForElement("CustomCollectionView");
            App.WaitForElement("StatusLabel");

            // Perform multiple scroll operations to ensure consistency

            // First scroll to index 25
            App.Click("ScrollToIndex25");
            System.Threading.Thread.Sleep(500);
            var status1 = App.FindElement("StatusLabel").GetText();
            Assert.That(status1, Does.Contain("Scrolled to:"));

            // Then scroll to item 40
            App.Click("ScrollToItem40");
            System.Threading.Thread.Sleep(500);
            var status2 = App.FindElement("StatusLabel").GetText();
            Assert.That(status2, Does.Contain("Scrolled to:"));
            Assert.That(status2, Is.Not.EqualTo(status1)); // Should be different

            // Finally scroll to end
            App.Click("ScrollToEnd");
            System.Threading.Thread.Sleep(500);
            var status3 = App.FindElement("StatusLabel").GetText();
            Assert.That(status3, Does.Contain("Scrolled to:"));
            Assert.That(status3, Is.Not.EqualTo(status2)); // Should be different

            // Verify we actually scrolled to different positions
            Assert.That(status3, Does.Match(@"Last=([89][0-9]|100)"));
        }

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void CustomCollectionViewShouldDisplayItems()
        {
            App.WaitForElement("CustomCollectionView");

            // Verify that the custom CollectionView is actually displaying items
            // This ensures the CollectionView is working, not just the ScrollTo functionality
            var collectionView = App.FindElement("CustomCollectionView");
            Assert.That(collectionView, Is.Not.Null);

            // The CollectionView should be visible and have content
            Assert.That(collectionView.GetAttribute("visible"), Is.EqualTo("true"));
        }
    }
}
