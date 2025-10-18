#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.MenuBarTests
{
    public class MenuBarTrackerTests : BaseTestFixture
    {
        [Fact]
        public void Constructor()
        {
            var tracker = new MenuBarTracker();
            Assert.Null(tracker.Target);
            Assert.False(tracker.ToolbarItems.Any());
        }

        [Fact]
        public void SimpleTrackEmpty()
        {
            var tracker = new MenuBarTracker();

            var page = new ContentPage();
            tracker.Target = page;

            Assert.False(tracker.ToolbarItems.Any());
        }

        [Fact]
        public void SimpleTrackWithItems()
        {
            var tracker = new MenuBarTracker();

            ToolbarItem item1, item2;
            var page = new ContentPage
            {
                MenuBarItems = {
                    new MenuBarItem (),
                    new MenuBarItem ()
                }
            };
            tracker.Target = page;

            Assert.True(tracker.ToolbarItems.Contains(page.MenuBarItems[0]));
            Assert.True(tracker.ToolbarItems.Contains(page.MenuBarItems[1]));
        }

        [Fact]
        public void TrackPreConstructedTabbedPage()
        {
            var tracker = new MenuBarTracker();

            var menubaritem1 = new MenuBarItem();
            var menubarItem2 = new MenuBarItem();
            var menubarItem3 = new MenuBarItem();

            var subPage1 = new ContentPage
            {
                MenuBarItems = { menubaritem1 }
            };

            var subPage2 = new ContentPage
            {
                MenuBarItems = { menubarItem2, menubarItem3 }
            };

            var tabbedpage = new TabbedPage
            {
                Children = {
                    subPage1,
                    subPage2
                }
            };

            tabbedpage.CurrentPage = subPage1;

            tracker.Target = tabbedpage;

            Assert.True(tracker.ToolbarItems.Count() == 1);
            Assert.True(tracker.ToolbarItems.First() == subPage1.MenuBarItems[0]);

            bool changed = false;
            tracker.CollectionChanged += (sender, args) => changed = true;

            tabbedpage.CurrentPage = subPage2;

            Assert.True(tracker.ToolbarItems.Count() == 2);
            Assert.True(tracker.ToolbarItems.First() == subPage2.MenuBarItems[0]);
            Assert.True(tracker.ToolbarItems.Last() == subPage2.MenuBarItems[1]);
            Assert.True(changed);
        }

        [Fact]
        public void AdditionalTargets()
        {
            var tracker = new MenuBarTracker();

            var menubaritem1 = new MenuBarItem();
            var menubarItem2 = new MenuBarItem();

            var page = new ContentPage
            {
                MenuBarItems = {
                    menubaritem1
                }
            };

            var additionalPage = new ContentPage
            {
                MenuBarItems = { menubarItem2 }
            };

            tracker.Target = page;
            tracker.AdditionalTargets = new[] { additionalPage };

            Assert.True(tracker.ToolbarItems.Contains(menubaritem1));
            Assert.True(tracker.ToolbarItems.Contains(menubarItem2));
        }

        [Fact]
        public async Task PushAfterTrackingStarted()
        {
            var tracker = new MenuBarTracker();

            var menubaritem1 = new MenuBarItem();
            var menubarItem2 = new MenuBarItem();

            var page = new NavigationPage
            {
                MenuBarItems = {
                    menubaritem1
                }
            };

            var firstPage = new ContentPage
            {
                MenuBarItems = { menubarItem2 }
            };

            tracker.Target = page;

            Assert.True(tracker.ToolbarItems.Contains(menubaritem1));
            Assert.False(tracker.ToolbarItems.Contains(menubarItem2));

            await page.Navigation.PushAsync(firstPage);

            Assert.True(tracker.ToolbarItems.Contains(menubaritem1));
            Assert.True(tracker.ToolbarItems.Contains(menubarItem2));
        }

        [Fact]
        public async Task PopAfterTrackingStarted()
        {
            var tracker = new MenuBarTracker();

            var menubaritem1 = new MenuBarItem();
            var menubarItem2 = new MenuBarItem();

            var page = new NavigationPage(new ContentPage())
            {
                MenuBarItems = {
                    menubaritem1
                }
            };

            var firstPage = new ContentPage
            {
                MenuBarItems = { menubarItem2 }
            };

            tracker.Target = page;

            await page.Navigation.PushAsync(firstPage);

            Assert.True(tracker.ToolbarItems.Contains(menubaritem1));
            Assert.True(tracker.ToolbarItems.Contains(menubarItem2));

            await page.Navigation.PopAsync();

            Assert.True(tracker.ToolbarItems.Contains(menubaritem1));
            Assert.False(tracker.ToolbarItems.Contains(menubarItem2));
        }

        [Fact]
        public void UnsetTarget()
        {
            var tracker = new MenuBarTracker();

            MenuBarItem item1, item2;
            var page = new ContentPage
            {
                MenuBarItems = {
                    new MenuBarItem (),
                    new MenuBarItem ()
                }
            };
            tracker.Target = page;

            Assert.True(tracker.ToolbarItems.Count() == 2);

            tracker.Target = null;

            Assert.False(tracker.ToolbarItems.Any());
        }

        [Fact]
        public void AddingMenuBarItemsFireCollectionChanged()
        {
            var tracker = new MenuBarTracker();

            var menubaritem1 = new MenuBarItem();
            var menubarItem2 = new MenuBarItem();

            var subPage1 = new ContentPage
            {
                MenuBarItems = { menubaritem1 }
            };

            tracker.Target = new NavigationPage(subPage1);

            bool changed = false;
            tracker.CollectionChanged += (sender, args) => changed = true;

            subPage1.MenuBarItems.Add(menubarItem2);
            Assert.True(changed);
        }

        /// <summary>
        /// Tests that MenuBar property returns null when ToolbarItems collection is empty.
        /// This verifies the early return condition when no menu bar items are present.
        /// </summary>
        [Fact]
        public void MenuBar_WhenToolbarItemsEmpty_ReturnsNull()
        {
            // Arrange
            var tracker = new MenuBarTracker();

            // Act
            var result = tracker.MenuBar;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that MenuBar property synchronizes items and returns the menu bar when ToolbarItems collection has items.
        /// This verifies that SyncMenuBarItemsFromPages is called and the internal menu bar is returned.
        /// </summary>
        [Fact]
        public void MenuBar_WhenToolbarItemsNotEmpty_SynchronizesAndReturnsMenuBar()
        {
            // Arrange
            var tracker = new MenuBarTracker();
            var page = new ContentPage
            {
                MenuBarItems = {
                    new MenuBarItem { Text = "Item1" },
                    new MenuBarItem { Text = "Item2" }
                }
            };
            tracker.Target = page;

            // Act
            var result = tracker.MenuBar;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Item1", result[0].Text);
            Assert.Equal("Item2", result[1].Text);
        }

        /// <summary>
        /// Tests that MenuBar property returns the same MenuBar instance on subsequent calls when items exist.
        /// This verifies that the internal _menuBar field is consistently returned.
        /// </summary>
        [Fact]
        public void MenuBar_WhenCalledMultipleTimes_ReturnsSameInstance()
        {
            // Arrange
            var tracker = new MenuBarTracker();
            var page = new ContentPage
            {
                MenuBarItems = {
                    new MenuBarItem { Text = "Item1" }
                }
            };
            tracker.Target = page;

            // Act
            var result1 = tracker.MenuBar;
            var result2 = tracker.MenuBar;

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that MenuBar property handles transition from empty to non-empty ToolbarItems correctly.
        /// This verifies the behavior when Target changes from null to a page with menu items.
        /// </summary>
        [Fact]
        public void MenuBar_WhenTransitionFromEmptyToNonEmpty_ReturnsCorrectResult()
        {
            // Arrange
            var tracker = new MenuBarTracker();

            // Act & Assert - Initially empty
            var emptyResult = tracker.MenuBar;
            Assert.Null(emptyResult);

            // Arrange - Add items
            var page = new ContentPage
            {
                MenuBarItems = {
                    new MenuBarItem { Text = "NewItem" }
                }
            };
            tracker.Target = page;

            // Act & Assert - Now has items
            var nonEmptyResult = tracker.MenuBar;
            Assert.NotNull(nonEmptyResult);
            Assert.Single(nonEmptyResult);
            Assert.Equal("NewItem", nonEmptyResult[0].Text);
        }

        /// <summary>
        /// Tests that MenuBar property handles transition from non-empty to empty ToolbarItems correctly.
        /// This verifies the behavior when Target changes from a page with items to null.
        /// </summary>
        [Fact]
        public void MenuBar_WhenTransitionFromNonEmptyToEmpty_ReturnsNull()
        {
            // Arrange
            var tracker = new MenuBarTracker();
            var page = new ContentPage
            {
                MenuBarItems = {
                    new MenuBarItem { Text = "Item1" }
                }
            };
            tracker.Target = page;

            // Act & Assert - Initially has items
            var nonEmptyResult = tracker.MenuBar;
            Assert.NotNull(nonEmptyResult);

            // Arrange - Remove target
            tracker.Target = null;

            // Act & Assert - Now empty
            var emptyResult = tracker.MenuBar;
            Assert.Null(emptyResult);
        }
    }
}