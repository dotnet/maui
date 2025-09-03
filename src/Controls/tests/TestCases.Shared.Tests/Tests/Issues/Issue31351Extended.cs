using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue31351Extended : _IssuesUITest
    {
        public override string Issue => "[Windows] - Extended test for Customised CollectionView ScrollTo";

        public Issue31351Extended(TestDevice device) : base(device)
        {
        }

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void ExtendedCustomCollectionViewScrollToTopShouldWork()
        {
            App.WaitForElement("ExtendedCustomCollectionView");
            App.WaitForElement("ScrollStatusLabel");

            // First scroll to bottom, then to top to ensure scrolling works
            App.Click("ScrollToBottom");
            System.Threading.Thread.Sleep(500);

            App.Click("ScrollToTop");
            System.Threading.Thread.Sleep(1000);

            var scrollStatus = App.FindElement("ScrollStatusLabel").GetText();
            Assert.That(scrollStatus, Does.Contain("✅ Scrolled:"));
            Assert.That(scrollStatus, Does.Contain("First=0"));
        }

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void ExtendedCustomCollectionViewScrollTo10WithSelectionShouldWork()
        {
            App.WaitForElement("ExtendedCustomCollectionView");
            App.WaitForElement("ScrollStatusLabel");
            App.WaitForElement("SelectionStatusLabel");

            App.Click("ScrollTo10");
            System.Threading.Thread.Sleep(1500);

            // Verify scrolling
            var scrollStatus = App.FindElement("ScrollStatusLabel").GetText();
            Assert.That(scrollStatus, Does.Contain("✅ Scrolled:"));

            // Verify selection (this tests that the custom method works)
            var selectionStatus = App.FindElement("SelectionStatusLabel").GetText();
            Assert.That(selectionStatus, Does.Contain("✅ Selected:"));
            Assert.That(selectionStatus, Does.Contain("Extended Item 10"));
            Assert.That(selectionStatus, Does.Contain("Index 10"));
        }

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void ExtendedCustomCollectionViewScrollTo30ShouldWork()
        {
            App.WaitForElement("ExtendedCustomCollectionView");
            App.WaitForElement("ScrollStatusLabel");

            App.Click("ScrollTo30");
            System.Threading.Thread.Sleep(1500);

            var scrollStatus = App.FindElement("ScrollStatusLabel").GetText();
            Assert.That(scrollStatus, Does.Contain("✅ Scrolled:"));

            // Should scroll to around index 30, checking center item
            Assert.That(scrollStatus, Does.Match(@"Center=([23][0-9]|[34][0-9])"));
        }

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void ExtendedCustomCollectionViewScrollToBottomShouldWork()
        {
            App.WaitForElement("ExtendedCustomCollectionView");
            App.WaitForElement("ScrollStatusLabel");

            App.Click("ScrollToBottom");
            System.Threading.Thread.Sleep(1000);

            var scrollStatus = App.FindElement("ScrollStatusLabel").GetText();
            Assert.That(scrollStatus, Does.Contain("✅ Scrolled:"));

            // Should scroll to the end (around index 49)
            Assert.That(scrollStatus, Does.Match(@"Last=([4][0-9]|50)"));
        }

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void ExtendedCustomCollectionViewSequentialScrollsShouldWork()
        {
            App.WaitForElement("ExtendedCustomCollectionView");
            App.WaitForElement("ScrollStatusLabel");
            App.WaitForElement("SelectionStatusLabel");

            // Perform a sequence of scrolls to test consistency

            // Start at top
            App.Click("ScrollToTop");
            System.Threading.Thread.Sleep(500);
            var status1 = App.FindElement("ScrollStatusLabel").GetText();
            Assert.That(status1, Does.Contain("✅ Scrolled:"));

            // Scroll to middle
            App.Click("ScrollTo30");
            System.Threading.Thread.Sleep(1000);
            var status2 = App.FindElement("ScrollStatusLabel").GetText();
            Assert.That(status2, Does.Contain("✅ Scrolled:"));
            Assert.That(status2, Is.Not.EqualTo(status1));

            // Scroll with selection
            App.Click("ScrollTo10");
            System.Threading.Thread.Sleep(1000);
            var status3 = App.FindElement("ScrollStatusLabel").GetText();
            var selectionStatus = App.FindElement("SelectionStatusLabel").GetText();

            Assert.That(status3, Does.Contain("✅ Scrolled:"));
            Assert.That(selectionStatus, Does.Contain("✅ Selected:"));

            // End at bottom
            App.Click("ScrollToBottom");
            System.Threading.Thread.Sleep(500);
            var status4 = App.FindElement("ScrollStatusLabel").GetText();
            Assert.That(status4, Does.Contain("✅ Scrolled:"));
            Assert.That(status4, Is.Not.EqualTo(status3));
        }

        [Test]
        [Category(UITestCategories.CollectionView)]
        public void ExtendedCustomCollectionViewShouldMaintainCustomProperties()
        {
            App.WaitForElement("ExtendedCustomCollectionView");

            // Verify the extended CollectionView is working
            var collectionView = App.FindElement("ExtendedCustomCollectionView");
            Assert.That(collectionView, Is.Not.Null);
            Assert.That(collectionView.GetAttribute("visible"), Is.EqualTo("true"));

            // Test that scrolling doesn't break the custom functionality
            App.Click("ScrollTo10");
            System.Threading.Thread.Sleep(1000);

            // CollectionView should still be functional after custom scroll method
            var scrollStatus = App.FindElement("ScrollStatusLabel").GetText();
            Assert.That(scrollStatus, Does.Contain("✅ Scrolled:"));
        }
    }
}
