#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class MenuItemTrackerTests
    {
        /// <summary>
        /// Tests that HaveFlyoutPage returns false when no FlyoutPage instances are tracked.
        /// Input condition: Initial state with _flyoutDetails = 0.
        /// Expected result: HaveFlyoutPage should return false.
        /// </summary>
        [Fact]
        public void HaveFlyoutPage_InitialState_ReturnsFalse()
        {
            // Arrange
            var tracker = new TestMenuItemTracker();

            // Act & Assert
            Assert.False(tracker.HaveFlyoutPage);
        }

        /// <summary>
        /// Tests that HaveFlyoutPage returns true when tracking one FlyoutPage.
        /// Input condition: One FlyoutPage set as Target.
        /// Expected result: HaveFlyoutPage should return true.
        /// </summary>
        [Fact]
        public void HaveFlyoutPage_WithOneFlyoutPageAsTarget_ReturnsTrue()
        {
            // Arrange
            var tracker = new TestMenuItemTracker();
            var flyoutPage = new FlyoutPage();

            // Act
            tracker.Target = flyoutPage;

            // Assert
            Assert.True(tracker.HaveFlyoutPage);
        }

        /// <summary>
        /// Tests that HaveFlyoutPage returns true when tracking multiple FlyoutPage instances.
        /// Input condition: Multiple FlyoutPage instances in AdditionalTargets.
        /// Expected result: HaveFlyoutPage should return true.
        /// </summary>
        [Fact]
        public void HaveFlyoutPage_WithMultipleFlyoutPages_ReturnsTrue()
        {
            // Arrange
            var tracker = new TestMenuItemTracker();
            var flyoutPage1 = new FlyoutPage();
            var flyoutPage2 = new FlyoutPage();

            // Act
            tracker.Target = flyoutPage1;
            tracker.AdditionalTargets = new[] { flyoutPage2 };

            // Assert
            Assert.True(tracker.HaveFlyoutPage);
        }

        /// <summary>
        /// Tests that HaveFlyoutPage returns false after removing all FlyoutPage instances.
        /// Input condition: FlyoutPage set as Target then removed.
        /// Expected result: HaveFlyoutPage should return false after removal.
        /// </summary>
        [Fact]
        public void HaveFlyoutPage_AfterRemovingFlyoutPage_ReturnsFalse()
        {
            // Arrange
            var tracker = new TestMenuItemTracker();
            var flyoutPage = new FlyoutPage();
            tracker.Target = flyoutPage;

            // Act
            tracker.Target = null;

            // Assert
            Assert.False(tracker.HaveFlyoutPage);
        }

        /// <summary>
        /// Tests that HaveFlyoutPage returns false when tracking non-FlyoutPage instances.
        /// Input condition: Regular Page instances set as Target and AdditionalTargets.
        /// Expected result: HaveFlyoutPage should return false.
        /// </summary>
        [Fact]
        public void HaveFlyoutPage_WithNonFlyoutPages_ReturnsFalse()
        {
            // Arrange
            var tracker = new TestMenuItemTracker();
            var regularPage = new ContentPage();
            var anotherPage = new ContentPage();

            // Act
            tracker.Target = regularPage;
            tracker.AdditionalTargets = new[] { anotherPage };

            // Assert
            Assert.False(tracker.HaveFlyoutPage);
        }

        /// <summary>
        /// Tests that HaveFlyoutPage returns true when mixing FlyoutPage and regular Page instances.
        /// Input condition: FlyoutPage as Target with regular Page in AdditionalTargets.
        /// Expected result: HaveFlyoutPage should return true.
        /// </summary>
        [Fact]
        public void HaveFlyoutPage_WithMixedPageTypes_ReturnsTrue()
        {
            // Arrange
            var tracker = new TestMenuItemTracker();
            var flyoutPage = new FlyoutPage();
            var regularPage = new ContentPage();

            // Act
            tracker.Target = flyoutPage;
            tracker.AdditionalTargets = new[] { regularPage };

            // Assert
            Assert.True(tracker.HaveFlyoutPage);
        }

        /// <summary>
        /// Tests that HaveFlyoutPage correctly handles replacement of Target page types.
        /// Input condition: Regular Page replaced with FlyoutPage as Target.
        /// Expected result: HaveFlyoutPage should change from false to true.
        /// </summary>
        [Fact]
        public void HaveFlyoutPage_ReplacingRegularPageWithFlyoutPage_ReturnsTrue()
        {
            // Arrange
            var tracker = new TestMenuItemTracker();
            var regularPage = new ContentPage();
            var flyoutPage = new FlyoutPage();
            tracker.Target = regularPage;

            // Act
            tracker.Target = flyoutPage;

            // Assert
            Assert.True(tracker.HaveFlyoutPage);
        }

        private class TestMenuItem : BaseMenuItem, IComparable<TestMenuItem>
        {
            public int CompareTo(TestMenuItem other)
            {
                return 0;
            }
        }

        /// <summary>
        /// Tests that the MenuItemTracker constructor properly initializes the object with default values.
        /// Verifies that all properties are in their expected initial state after construction.
        /// </summary>
        [Fact]
        public void Constructor()
        {
            // Arrange & Act
            var tracker = new TestMenuItemTracker();

            // Assert
            Assert.False(tracker.HaveFlyoutPage);
            Assert.Null(tracker.Target);
            Assert.NotNull(tracker.AdditionalTargets);
            Assert.Empty(tracker.AdditionalTargets);
            Assert.False(tracker.SeparateFlyoutPage);
            Assert.NotNull(tracker.ToolbarItems);
            Assert.Empty(tracker.ToolbarItems);
        }

    }
}