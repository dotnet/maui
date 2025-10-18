#nullable disable

#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using NSubstitute;
#nullable enable
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
    [Category("Layout")]
    public class AbsoluteLayoutTests : BaseTestFixture
    {
        [Fact]
        public void BoundsDefaultsConsistentForNewChildren()
        {
            var layout = new AbsoluteLayout();

            var child1 = new Label { }; // BindableObject
            var child2 = NSubstitute.Substitute.For<IView>(); // Not a BindableObject

            layout.Add(child1);
            layout.Add(child2);

            var bounds1 = layout.GetLayoutBounds(child1);
            var bounds2 = layout.GetLayoutBounds(child2);

            // The default layout bounds given to each of these child IViews _should_ be the same
            Assert.Equal(bounds1.X, bounds2.X);
            Assert.Equal(bounds1.Y, bounds2.Y);
            Assert.Equal(bounds1.Width, bounds2.Width);
            Assert.Equal(bounds1.Height, bounds2.Height);
        }

        [Fact]
        public void BoundsDefaultsAttachedProperty()
        {
            var layout = new AbsoluteLayout();

            var child = new Label { }; // BindableObject

            layout.Add(child);

            var bounds = layout.GetLayoutBounds(child);

            Assert.Equal(0, bounds.X);
            Assert.Equal(0, bounds.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Height);
        }

        [Fact]
        public void BoundsDefaultsRegularProperty()
        {
            var layout = new AbsoluteLayout();

            var child = NSubstitute.Substitute.For<IView>(); // Not a BindableObject

            layout.Add(child);

            var bounds = layout.GetLayoutBounds(child);

            Assert.Equal(0, bounds.X);
            Assert.Equal(0, bounds.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Height);
        }

        /// <summary>
        /// Tests that On method returns a non-null configuration for a valid platform type
        /// </summary>
        [Fact]
        public void On_ValidPlatformType_ReturnsNonNullConfiguration()
        {
            // Arrange
            var layout = new AbsoluteLayout();

            // Act
            var result = layout.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that On method returns the same instance when called multiple times with the same platform type
        /// </summary>
        [Fact]
        public void On_SamePlatformTypeCalledTwice_ReturnsSameInstance()
        {
            // Arrange
            var layout = new AbsoluteLayout();

            // Act
            var result1 = layout.On<TestPlatform>();
            var result2 = layout.On<TestPlatform>();

            // Assert
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that On method returns different instances for different platform types
        /// </summary>
        [Fact]
        public void On_DifferentPlatformTypes_ReturnsDifferentInstances()
        {
            // Arrange
            var layout = new AbsoluteLayout();

            // Act
            var result1 = layout.On<TestPlatform>();
            var result2 = layout.On<AnotherTestPlatform>();

            // Assert
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that On method returns correct generic type
        /// </summary>
        [Fact]
        public void On_ValidPlatformType_ReturnsCorrectGenericType()
        {
            // Arrange
            var layout = new AbsoluteLayout();

            // Act
            var result = layout.On<TestPlatform>();

            // Assert
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, AbsoluteLayout>>(result);
        }

        /// <summary>
        /// Test platform implementation for testing purposes
        /// </summary>
        private class TestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Another test platform implementation for testing purposes
        /// </summary>
        private class AnotherTestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Tests that OnChildRemoved properly unsubscribes from PropertyChanged event and calls base implementation
        /// with a valid Element child and positive oldLogicalIndex.
        /// </summary>
        [Fact]
        public void OnChildRemoved_ValidElementChild_UnsubscribesFromPropertyChangedAndCallsBase()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var child = new Label();
            var oldLogicalIndex = 1;

            // First add the child to subscribe to PropertyChanged
            layout.TestOnChildAdded(child);

            // Verify child is subscribed to PropertyChanged by checking if layout responds to property changes
            bool layoutInvalidated = false;
            layout.InvalidateMeasureRequested += (sender, args) => layoutInvalidated = true;

            // Act - Remove the child
            layout.TestOnChildRemoved(child, oldLogicalIndex);

            // Change a property that would trigger ChildOnPropertyChanged if still subscribed
            AbsoluteLayout.SetLayoutFlags(child, AbsoluteLayoutFlags.PositionProportional);

            // Assert - The layout should not be invalidated since PropertyChanged was unsubscribed
            Assert.False(layoutInvalidated);
            Assert.True(layout.BaseOnChildRemovedCalled);
            Assert.Equal(child, layout.LastRemovedChild);
            Assert.Equal(oldLogicalIndex, layout.LastRemovedIndex);
        }

        /// <summary>
        /// Tests that OnChildRemoved handles null child parameter without throwing exceptions
        /// and still calls base implementation.
        /// </summary>
        [Fact]
        public void OnChildRemoved_NullChild_CallsBaseWithoutException()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            Element child = null;
            var oldLogicalIndex = 0;

            // Act & Assert - Should not throw
            layout.TestOnChildRemoved(child, oldLogicalIndex);

            Assert.True(layout.BaseOnChildRemovedCalled);
            Assert.Null(layout.LastRemovedChild);
            Assert.Equal(oldLogicalIndex, layout.LastRemovedIndex);
        }

        /// <summary>
        /// Tests OnChildRemoved with various oldLogicalIndex values including negative, zero, and positive values.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void OnChildRemoved_VariousLogicalIndexValues_CallsBaseWithCorrectParameters(int oldLogicalIndex)
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var child = new Label();

            // Act
            layout.TestOnChildRemoved(child, oldLogicalIndex);

            // Assert
            Assert.True(layout.BaseOnChildRemovedCalled);
            Assert.Equal(child, layout.LastRemovedChild);
            Assert.Equal(oldLogicalIndex, layout.LastRemovedIndex);
        }

        /// <summary>
        /// Tests that OnChildRemoved properly unsubscribes from PropertyChanged event for View elements.
        /// </summary>
        [Fact]
        public void OnChildRemoved_ViewChild_UnsubscribesFromPropertyChanged()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var view = new Button();
            var oldLogicalIndex = 0;

            // Subscribe to PropertyChanged first
            layout.TestOnChildAdded(view);

            // Track if ChildOnPropertyChanged would be called
            bool childPropertyChangedCalled = false;
            layout.ChildOnPropertyChangedCalled += () => childPropertyChangedCalled = true;

            // Act - Remove the child
            layout.TestOnChildRemoved(view, oldLogicalIndex);

            // Trigger PropertyChanged event
            AbsoluteLayout.SetLayoutBounds(view, new Rect(10, 10, 100, 100));

            // Assert - ChildOnPropertyChanged should not be called since we unsubscribed
            Assert.False(childPropertyChangedCalled);
        }

        private class TestableAbsoluteLayout : AbsoluteLayout
        {
            public bool BaseOnChildRemovedCalled { get; private set; }
            public Element LastRemovedChild { get; private set; }
            public int LastRemovedIndex { get; private set; }

            public event Action ChildOnPropertyChangedCalled;
            public event EventHandler<InvalidationEventArgs> InvalidateMeasureRequested;

            public void TestOnChildRemoved(Element child, int oldLogicalIndex)
            {
                OnChildRemoved(child, oldLogicalIndex);
            }

            public void TestOnChildAdded(Element child)
            {
                OnChildAdded(child);
            }

            protected override void OnChildRemoved(Element child, int oldLogicalIndex)
            {
                // Call the actual implementation we're testing
                base.OnChildRemoved(child, oldLogicalIndex);

                // Track that base was called
                BaseOnChildRemovedCalled = true;
                LastRemovedChild = child;
                LastRemovedIndex = oldLogicalIndex;
            }

            private void OnChildOnPropertyChangedCalled()
            {
                ChildOnPropertyChangedCalled?.Invoke();
            }
        }

        private class InvalidationEventArgs : EventArgs
        {
            public InvalidationTrigger Trigger { get; set; }
        }
    }
}

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public partial class IAbsoluteListAddTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that Add method correctly adds a view with specified bounds and default flags.
        /// Verifies that layout bounds are set correctly and flags default to None.
        /// </summary>
        [Fact]
        public void Add_WithBoundsAndDefaultFlags_SetsCorrectBoundsAndDefaultFlags()
        {
            // Arrange
            var layout = new Compatibility.AbsoluteLayout { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();
            var bounds = new Rect(10, 20, 30, 40);

            // Act
            layout.Children.Add(view, bounds);

            // Assert
            Assert.Equal(bounds, Compatibility.AbsoluteLayout.GetLayoutBounds(view));
            Assert.Equal(AbsoluteLayoutFlags.None, Compatibility.AbsoluteLayout.GetLayoutFlags(view));
            Assert.Contains(view, layout.Children);
        }

        /// <summary>
        /// Tests that Add method correctly adds a view with specified bounds and flags.
        /// Verifies that both layout bounds and flags are set correctly on the view.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 10, 10, AbsoluteLayoutFlags.None)]
        [InlineData(5.5, 7.2, 15.8, 20.3, AbsoluteLayoutFlags.XProportional)]
        [InlineData(-10, -20, 30, 40, AbsoluteLayoutFlags.YProportional)]
        [InlineData(100, 200, 0.5, 0.8, AbsoluteLayoutFlags.WidthProportional)]
        [InlineData(0.1, 0.2, 50, 60, AbsoluteLayoutFlags.HeightProportional)]
        [InlineData(0.5, 0.5, 0.3, 0.4, AbsoluteLayoutFlags.PositionProportional)]
        [InlineData(10, 20, 0.6, 0.7, AbsoluteLayoutFlags.SizeProportional)]
        [InlineData(0.1, 0.2, 0.3, 0.4, AbsoluteLayoutFlags.All)]
        public void Add_WithBoundsAndFlags_SetsCorrectBoundsAndFlags(double x, double y, double width, double height, AbsoluteLayoutFlags flags)
        {
            // Arrange
            var layout = new Compatibility.AbsoluteLayout { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();
            var bounds = new Rect(x, y, width, height);

            // Act
            layout.Children.Add(view, bounds, flags);

            // Assert
            Assert.Equal(bounds, Compatibility.AbsoluteLayout.GetLayoutBounds(view));
            Assert.Equal(flags, Compatibility.AbsoluteLayout.GetLayoutFlags(view));
            Assert.Contains(view, layout.Children);
        }

        /// <summary>
        /// Tests that Add method handles extreme double values in bounds correctly.
        /// Verifies behavior with NaN, positive infinity, and negative infinity values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 0, 10, 10)]
        [InlineData(0, double.NaN, 10, 10)]
        [InlineData(0, 0, double.NaN, 10)]
        [InlineData(0, 0, 10, double.NaN)]
        [InlineData(double.PositiveInfinity, 0, 10, 10)]
        [InlineData(0, double.PositiveInfinity, 10, 10)]
        [InlineData(0, 0, double.PositiveInfinity, 10)]
        [InlineData(0, 0, 10, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 0, 10, 10)]
        [InlineData(0, double.NegativeInfinity, 10, 10)]
        [InlineData(0, 0, double.NegativeInfinity, 10)]
        [InlineData(0, 0, 10, double.NegativeInfinity)]
        public void Add_WithExtremeBoundsValues_HandlesExtremeValues(double x, double y, double width, double height)
        {
            // Arrange
            var layout = new Compatibility.AbsoluteLayout { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();
            var bounds = new Rect(x, y, width, height);

            // Act
            layout.Children.Add(view, bounds, AbsoluteLayoutFlags.None);

            // Assert
            Assert.Equal(bounds, Compatibility.AbsoluteLayout.GetLayoutBounds(view));
            Assert.Equal(AbsoluteLayoutFlags.None, Compatibility.AbsoluteLayout.GetLayoutFlags(view));
            Assert.Contains(view, layout.Children);
        }

        /// <summary>
        /// Tests that Add method handles boundary values for bounds correctly.
        /// Verifies behavior with zero dimensions, maximum values, and minimum values.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(double.MinValue, double.MinValue, double.MaxValue, double.MaxValue)]
        [InlineData(-1000000, -1000000, 0, 0)]
        [InlineData(1000000, 1000000, 1000000, 1000000)]
        public void Add_WithBoundaryBoundsValues_HandlesBoundaryValues(double x, double y, double width, double height)
        {
            // Arrange  
            var layout = new Compatibility.AbsoluteLayout { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();
            var bounds = new Rect(x, y, width, height);

            // Act
            layout.Children.Add(view, bounds, AbsoluteLayoutFlags.None);

            // Assert
            Assert.Equal(bounds, Compatibility.AbsoluteLayout.GetLayoutBounds(view));
            Assert.Equal(AbsoluteLayoutFlags.None, Compatibility.AbsoluteLayout.GetLayoutFlags(view));
            Assert.Contains(view, layout.Children);
        }

        /// <summary>
        /// Tests that Add method handles combined layout flags correctly.
        /// Verifies behavior with bitwise flag combinations.
        /// </summary>
        [Theory]
        [InlineData(AbsoluteLayoutFlags.XProportional | AbsoluteLayoutFlags.YProportional)]
        [InlineData(AbsoluteLayoutFlags.WidthProportional | AbsoluteLayoutFlags.HeightProportional)]
        [InlineData(AbsoluteLayoutFlags.XProportional | AbsoluteLayoutFlags.WidthProportional)]
        [InlineData(AbsoluteLayoutFlags.YProportional | AbsoluteLayoutFlags.HeightProportional)]
        [InlineData(AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.SizeProportional)]
        public void Add_WithCombinedFlags_SetsCombinedFlags(AbsoluteLayoutFlags flags)
        {
            // Arrange
            var layout = new Compatibility.AbsoluteLayout { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();
            var bounds = new Rect(10, 20, 30, 40);

            // Act
            layout.Children.Add(view, bounds, flags);

            // Assert
            Assert.Equal(bounds, Compatibility.AbsoluteLayout.GetLayoutBounds(view));
            Assert.Equal(flags, Compatibility.AbsoluteLayout.GetLayoutFlags(view));
            Assert.Contains(view, layout.Children);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentNullException when view is null.
        /// Verifies proper null parameter validation.
        /// </summary>
        [Fact]
        public void Add_WithNullView_ThrowsArgumentNullException()
        {
            // Arrange
            var layout = new Compatibility.AbsoluteLayout { IsPlatformEnabled = true };
            var bounds = new Rect(10, 20, 30, 40);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => layout.Children.Add(null, bounds, AbsoluteLayoutFlags.None));
        }

        /// <summary>
        /// Tests that Add method handles invalid enum flag values correctly.
        /// Verifies behavior when flags are cast from invalid integer values.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void Add_WithInvalidFlagsValue_HandlesInvalidFlags(int invalidFlags)
        {
            // Arrange
            var layout = new Compatibility.AbsoluteLayout { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();
            var bounds = new Rect(10, 20, 30, 40);
            var flags = (AbsoluteLayoutFlags)invalidFlags;

            // Act
            layout.Children.Add(view, bounds, flags);

            // Assert
            Assert.Equal(bounds, Compatibility.AbsoluteLayout.GetLayoutBounds(view));
            Assert.Equal(flags, Compatibility.AbsoluteLayout.GetLayoutFlags(view));
            Assert.Contains(view, layout.Children);
        }

        /// <summary>
        /// Tests that Add method can add multiple views to the same layout.
        /// Verifies that the collection maintains all added views correctly.
        /// </summary>
        [Fact]
        public void Add_MultipleViews_AddsAllViewsToCollection()
        {
            // Arrange
            var layout = new Compatibility.AbsoluteLayout { IsPlatformEnabled = true };
            var view1 = MockPlatformSizeService.Sub<View>();
            var view2 = MockPlatformSizeService.Sub<View>();
            var view3 = MockPlatformSizeService.Sub<View>();
            var bounds1 = new Rect(0, 0, 10, 10);
            var bounds2 = new Rect(20, 20, 30, 30);
            var bounds3 = new Rect(50, 50, 40, 40);

            // Act
            layout.Children.Add(view1, bounds1, AbsoluteLayoutFlags.None);
            layout.Children.Add(view2, bounds2, AbsoluteLayoutFlags.XProportional);
            layout.Children.Add(view3, bounds3, AbsoluteLayoutFlags.All);

            // Assert
            Assert.Equal(3, layout.Children.Count);
            Assert.Contains(view1, layout.Children);
            Assert.Contains(view2, layout.Children);
            Assert.Contains(view3, layout.Children);
            Assert.Equal(bounds1, Compatibility.AbsoluteLayout.GetLayoutBounds(view1));
            Assert.Equal(bounds2, Compatibility.AbsoluteLayout.GetLayoutBounds(view2));
            Assert.Equal(bounds3, Compatibility.AbsoluteLayout.GetLayoutBounds(view3));
            Assert.Equal(AbsoluteLayoutFlags.None, Compatibility.AbsoluteLayout.GetLayoutFlags(view1));
            Assert.Equal(AbsoluteLayoutFlags.XProportional, Compatibility.AbsoluteLayout.GetLayoutFlags(view2));
            Assert.Equal(AbsoluteLayoutFlags.All, Compatibility.AbsoluteLayout.GetLayoutFlags(view3));
        }

        /// <summary>
        /// Tests that Add method can add the same view multiple times with different bounds.
        /// Verifies that the last bounds and flags are applied to the view.
        /// </summary>
        [Fact]
        public void Add_SameViewMultipleTimes_UpdatesBoundsAndFlags()
        {
            // Arrange
            var layout = new Compatibility.AbsoluteLayout { IsPlatformEnabled = true };
            var view = MockPlatformSizeService.Sub<View>();
            var initialBounds = new Rect(0, 0, 10, 10);
            var finalBounds = new Rect(20, 30, 40, 50);

            // Act
            layout.Children.Add(view, initialBounds, AbsoluteLayoutFlags.None);
            layout.Children.Add(view, finalBounds, AbsoluteLayoutFlags.All);

            // Assert
            Assert.Equal(finalBounds, Compatibility.AbsoluteLayout.GetLayoutBounds(view));
            Assert.Equal(AbsoluteLayoutFlags.All, Compatibility.AbsoluteLayout.GetLayoutFlags(view));
            Assert.Contains(view, layout.Children);
        }
    }


    public partial class AbsoluteLayoutIAbsoluteListAddTests
    {
        /// <summary>
        /// Tests that Add method with valid view and point parameters correctly adds the view to the collection
        /// and sets appropriate layout bounds with AutoSize for width and height.
        /// </summary>
        [Fact]
        public void Add_ValidViewAndPoint_AddsViewWithCorrectBounds()
        {
            // Arrange
            var absoluteLayout = new AbsoluteLayout();
            var view = Substitute.For<View>();
            var position = new Point(10, 20);

            // Act
            absoluteLayout.Children.Add(view, position);

            // Assert
            Assert.Contains(view, absoluteLayout.Children);
            var bounds = AbsoluteLayout.GetLayoutBounds(view);
            Assert.Equal(10, bounds.X);
            Assert.Equal(20, bounds.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Height);
        }

        /// <summary>
        /// Tests that Add method throws ArgumentNullException when view parameter is null.
        /// </summary>
        [Fact]
        public void Add_NullView_ThrowsArgumentNullException()
        {
            // Arrange
            var absoluteLayout = new AbsoluteLayout();
            var position = new Point(0, 0);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => absoluteLayout.Children.Add(null, position));
        }

        /// <summary>
        /// Tests Add method with various point coordinate values including negative values,
        /// zero values, positive values, and extreme values.
        /// </summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(-10, -20)]
        [InlineData(100.5, 200.75)]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        public void Add_VariousPointCoordinates_SetsCorrectLayoutBounds(double x, double y)
        {
            // Arrange
            var absoluteLayout = new AbsoluteLayout();
            var view = Substitute.For<View>();
            var position = new Point(x, y);

            // Act
            absoluteLayout.Children.Add(view, position);

            // Assert
            Assert.Contains(view, absoluteLayout.Children);
            var bounds = AbsoluteLayout.GetLayoutBounds(view);
            Assert.Equal(x, bounds.X);
            Assert.Equal(y, bounds.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Height);
        }

        /// <summary>
        /// Tests that Add method correctly adds multiple views with different positions
        /// and maintains them in the collection.
        /// </summary>
        [Fact]
        public void Add_MultipleViewsWithDifferentPositions_AddsAllViews()
        {
            // Arrange
            var absoluteLayout = new AbsoluteLayout();
            var view1 = Substitute.For<View>();
            var view2 = Substitute.For<View>();
            var view3 = Substitute.For<View>();
            var position1 = new Point(10, 20);
            var position2 = new Point(30, 40);
            var position3 = new Point(50, 60);

            // Act
            absoluteLayout.Children.Add(view1, position1);
            absoluteLayout.Children.Add(view2, position2);
            absoluteLayout.Children.Add(view3, position3);

            // Assert
            Assert.Equal(3, absoluteLayout.Children.Count);
            Assert.Contains(view1, absoluteLayout.Children);
            Assert.Contains(view2, absoluteLayout.Children);
            Assert.Contains(view3, absoluteLayout.Children);

            var bounds1 = AbsoluteLayout.GetLayoutBounds(view1);
            var bounds2 = AbsoluteLayout.GetLayoutBounds(view2);
            var bounds3 = AbsoluteLayout.GetLayoutBounds(view3);

            Assert.Equal(new Rect(10, 20, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), bounds1);
            Assert.Equal(new Rect(30, 40, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), bounds2);
            Assert.Equal(new Rect(50, 60, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), bounds3);
        }

        /// <summary>
        /// Tests that Add method can add the same view multiple times, with the last position taking precedence.
        /// </summary>
        [Fact]
        public void Add_SameViewMultipleTimes_UpdatesPosition()
        {
            // Arrange
            var absoluteLayout = new AbsoluteLayout();
            var view = Substitute.For<View>();
            var initialPosition = new Point(10, 20);
            var updatedPosition = new Point(30, 40);

            // Act
            absoluteLayout.Children.Add(view, initialPosition);
            absoluteLayout.Children.Add(view, updatedPosition);

            // Assert
            var bounds = AbsoluteLayout.GetLayoutBounds(view);
            Assert.Equal(30, bounds.X);
            Assert.Equal(40, bounds.Y);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Width);
            Assert.Equal(AbsoluteLayout.AutoSize, bounds.Height);
        }

        /// <summary>
        /// Tests Add method with Point at origin (0,0) sets correct layout bounds.
        /// </summary>
        [Fact]
        public void Add_PointAtOrigin_SetsCorrectBounds()
        {
            // Arrange
            var absoluteLayout = new AbsoluteLayout();
            var view = Substitute.For<View>();
            var origin = new Point(0, 0);

            // Act
            absoluteLayout.Children.Add(view, origin);

            // Assert
            var bounds = AbsoluteLayout.GetLayoutBounds(view);
            Assert.Equal(new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), bounds);
        }
    }


    public partial class AbsoluteLayoutCompatibilityTests
    {
        /// <summary>
        /// Tests that Add(View, Point) method correctly sets layout bounds using the provided position coordinates
        /// and adds the view to the collection with AutoSize dimensions.
        /// </summary>
        [Fact]
        public void Add_WithValidViewAndPosition_SetsCorrectLayoutBoundsAndAddsView()
        {
            // Arrange
            var layout = new Compatibility.AbsoluteLayout();
            var view = new Label();
            var position = new Point(10, 20);
            var expectedBounds = new Rect(10, 20, Compatibility.AbsoluteLayout.AutoSize, Compatibility.AbsoluteLayout.AutoSize);

            // Act
            layout.Children.Add(view, position);

            // Assert
            Assert.Contains(view, layout.Children);
            var actualBounds = Compatibility.AbsoluteLayout.GetLayoutBounds(view);
            Assert.Equal(expectedBounds.X, actualBounds.X);
            Assert.Equal(expectedBounds.Y, actualBounds.Y);
            Assert.Equal(expectedBounds.Width, actualBounds.Width);
            Assert.Equal(expectedBounds.Height, actualBounds.Height);
        }

        /// <summary>
        /// Tests that Add(View, Point) method throws ArgumentNullException when view parameter is null.
        /// </summary>
        [Fact]
        public void Add_WithNullView_ThrowsArgumentNullException()
        {
            // Arrange
            var layout = new Compatibility.AbsoluteLayout();
            var position = new Point(0, 0);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => layout.Children.Add(null, position));
        }

        /// <summary>
        /// Tests that Add(View, Point) method correctly handles zero coordinates by setting layout bounds to (0,0) position.
        /// </summary>
        [Fact]
        public void Add_WithZeroCoordinates_SetsCorrectLayoutBounds()
        {
            // Arrange
            var layout = new Compatibility.AbsoluteLayout();
            var view = new Label();
            var position = new Point(0, 0);
            var expectedBounds = new Rect(0, 0, Compatibility.AbsoluteLayout.AutoSize, Compatibility.AbsoluteLayout.AutoSize);

            // Act
            layout.Children.Add(view, position);

            // Assert
            var actualBounds = Compatibility.AbsoluteLayout.GetLayoutBounds(view);
            Assert.Equal(expectedBounds.X, actualBounds.X);
            Assert.Equal(expectedBounds.Y, actualBounds.Y);
            Assert.Equal(expectedBounds.Width, actualBounds.Width);
            Assert.Equal(expectedBounds.Height, actualBounds.Height);
        }

        /// <summary>
        /// Tests that Add(View, Point) method correctly handles negative coordinates by setting layout bounds with negative position values.
        /// </summary>
        [Theory]
        [InlineData(-10, -20)]
        [InlineData(-100, 50)]
        [InlineData(75, -150)]
        public void Add_WithNegativeCoordinates_SetsCorrectLayoutBounds(double x, double y)
        {
            // Arrange
            var layout = new Compatibility.AbsoluteLayout();
            var view = new Label();
            var position = new Point(x, y);
            var expectedBounds = new Rect(x, y, Compatibility.AbsoluteLayout.AutoSize, Compatibility.AbsoluteLayout.AutoSize);

            // Act
            layout.Children.Add(view, position);

            // Assert
            var actualBounds = Compatibility.AbsoluteLayout.GetLayoutBounds(view);
            Assert.Equal(expectedBounds.X, actualBounds.X);
            Assert.Equal(expectedBounds.Y, actualBounds.Y);
            Assert.Equal(expectedBounds.Width, actualBounds.Width);
            Assert.Equal(expectedBounds.Height, actualBounds.Height);
        }

        /// <summary>
        /// Tests that Add(View, Point) method correctly handles extreme coordinate values including boundary conditions.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.MaxValue, double.MinValue)]
        [InlineData(double.MinValue, double.MaxValue)]
        public void Add_WithExtremeCoordinates_SetsCorrectLayoutBounds(double x, double y)
        {
            // Arrange
            var layout = new Compatibility.AbsoluteLayout();
            var view = new Label();
            var position = new Point(x, y);
            var expectedBounds = new Rect(x, y, Compatibility.AbsoluteLayout.AutoSize, Compatibility.AbsoluteLayout.AutoSize);

            // Act
            layout.Children.Add(view, position);

            // Assert
            var actualBounds = Compatibility.AbsoluteLayout.GetLayoutBounds(view);
            Assert.Equal(expectedBounds.X, actualBounds.X);
            Assert.Equal(expectedBounds.Y, actualBounds.Y);
            Assert.Equal(expectedBounds.Width, actualBounds.Width);
            Assert.Equal(expectedBounds.Height, actualBounds.Height);
        }

        /// <summary>
        /// Tests that Add(View, Point) method correctly handles special double values like NaN and infinity.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, 10)]
        [InlineData(10, double.NaN)]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, 10)]
        [InlineData(10, double.PositiveInfinity)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, 10)]
        [InlineData(10, double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        public void Add_WithSpecialDoubleValues_SetsCorrectLayoutBounds(double x, double y)
        {
            // Arrange
            var layout = new Compatibility.AbsoluteLayout();
            var view = new Label();
            var position = new Point(x, y);
            var expectedBounds = new Rect(x, y, Compatibility.AbsoluteLayout.AutoSize, Compatibility.AbsoluteLayout.AutoSize);

            // Act
            layout.Children.Add(view, position);

            // Assert
            var actualBounds = Compatibility.AbsoluteLayout.GetLayoutBounds(view);
            Assert.Equal(expectedBounds.X, actualBounds.X);
            Assert.Equal(expectedBounds.Y, actualBounds.Y);
            Assert.Equal(expectedBounds.Width, actualBounds.Width);
            Assert.Equal(expectedBounds.Height, actualBounds.Height);
        }

        /// <summary>
        /// Tests that Add(View, Point) method handles adding the same view multiple times by not duplicating it in the collection.
        /// </summary>
        [Fact]
        public void Add_WithSameViewTwice_DoesNotDuplicateInCollection()
        {
            // Arrange
            var layout = new Compatibility.AbsoluteLayout();
            var view = new Label();
            var position1 = new Point(10, 20);
            var position2 = new Point(30, 40);

            // Act
            layout.Children.Add(view, position1);
            var countAfterFirst = layout.Children.Count;
            layout.Children.Add(view, position2);
            var countAfterSecond = layout.Children.Count;

            // Assert
            Assert.Equal(1, countAfterFirst);
            Assert.Equal(1, countAfterSecond);
            Assert.Contains(view, layout.Children);
        }

        /// <summary>
        /// Tests that Add(View, Point) method correctly creates Rect with AutoSize dimensions which equal -1.
        /// </summary>
        [Fact]
        public void Add_WithAnyPosition_CreatesRectWithAutoSizeDimensions()
        {
            // Arrange
            var layout = new Compatibility.AbsoluteLayout();
            var view = new Label();
            var position = new Point(100, 200);

            // Act
            layout.Children.Add(view, position);

            // Assert
            var actualBounds = Compatibility.AbsoluteLayout.GetLayoutBounds(view);
            Assert.Equal(-1, actualBounds.Width);
            Assert.Equal(-1, actualBounds.Height);
            Assert.Equal(Compatibility.AbsoluteLayout.AutoSize, actualBounds.Width);
            Assert.Equal(Compatibility.AbsoluteLayout.AutoSize, actualBounds.Height);
        }
    }


    public class AbsoluteLayoutComputeConstraintForViewTests
    {
        /// <summary>
        /// Tests ComputeConstraintForView when SizeProportional flag is set with Fill alignment for both vertical and horizontal options.
        /// Should return the Constraint property value.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_SizeProportionalWithFillAlignment_ReturnsConstraint()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var view = Substitute.For<View>();
            var verticalOptions = new LayoutOptions(LayoutAlignment.Fill, false);
            var horizontalOptions = new LayoutOptions(LayoutAlignment.Fill, false);

            view.VerticalOptions.Returns(verticalOptions);
            view.HorizontalOptions.Returns(horizontalOptions);

            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.SizeProportional);

            // Act
            var result = layout.TestComputeConstraintForView(view);

            // Assert
            Assert.Equal(layout.Constraint, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView when SizeProportional flag is set but vertical alignment is not Fill.
        /// Should return LayoutConstraint.None.
        /// </summary>
        [Theory]
        [InlineData(LayoutAlignment.Start)]
        [InlineData(LayoutAlignment.Center)]
        [InlineData(LayoutAlignment.End)]
        public void ComputeConstraintForView_SizeProportionalWithNonFillVerticalAlignment_ReturnsNone(LayoutAlignment verticalAlignment)
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var view = Substitute.For<View>();
            var verticalOptions = new LayoutOptions(verticalAlignment, false);
            var horizontalOptions = new LayoutOptions(LayoutAlignment.Fill, false);

            view.VerticalOptions.Returns(verticalOptions);
            view.HorizontalOptions.Returns(horizontalOptions);

            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.SizeProportional);

            // Act
            var result = layout.TestComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.None, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView when SizeProportional flag is set but horizontal alignment is not Fill.
        /// Should return LayoutConstraint.None.
        /// </summary>
        [Theory]
        [InlineData(LayoutAlignment.Start)]
        [InlineData(LayoutAlignment.Center)]
        [InlineData(LayoutAlignment.End)]
        public void ComputeConstraintForView_SizeProportionalWithNonFillHorizontalAlignment_ReturnsNone(LayoutAlignment horizontalAlignment)
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var view = Substitute.For<View>();
            var verticalOptions = new LayoutOptions(LayoutAlignment.Fill, false);
            var horizontalOptions = new LayoutOptions(horizontalAlignment, false);

            view.VerticalOptions.Returns(verticalOptions);
            view.HorizontalOptions.Returns(horizontalOptions);

            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.SizeProportional);

            // Act
            var result = layout.TestComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.None, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView when HeightProportional flag is set with fixed width (not AutoSize).
        /// Should return constraint with both vertically and horizontally fixed flags.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_HeightProportionalWithFixedWidth_ReturnsBothFixed()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var view = Substitute.For<View>();
            var bounds = new Rect(0, 0, 100, AbsoluteLayout.AutoSize); // Fixed width, auto height

            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.HeightProportional);
            AbsoluteLayout.SetLayoutBounds(view, bounds);

            // Act
            var result = layout.TestComputeConstraintForView(view);

            // Assert
            var expected = (layout.Constraint & LayoutConstraint.VerticallyFixed) | LayoutConstraint.HorizontallyFixed;
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView when HeightProportional flag is set with auto width (AutoSize).
        /// Should return constraint with only vertically fixed flag.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_HeightProportionalWithAutoWidth_ReturnsVerticallyFixed()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var view = Substitute.For<View>();
            var bounds = new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize); // Auto width, auto height

            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.HeightProportional);
            AbsoluteLayout.SetLayoutBounds(view, bounds);

            // Act
            var result = layout.TestComputeConstraintForView(view);

            // Assert
            var expected = layout.Constraint & LayoutConstraint.VerticallyFixed;
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView when WidthProportional flag is set with fixed height (not AutoSize).
        /// Should return constraint with both horizontally and vertically fixed flags.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_WidthProportionalWithFixedHeight_ReturnsBothFixed()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var view = Substitute.For<View>();
            var bounds = new Rect(0, 0, AbsoluteLayout.AutoSize, 100); // Auto width, fixed height

            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.WidthProportional);
            AbsoluteLayout.SetLayoutBounds(view, bounds);

            // Act
            var result = layout.TestComputeConstraintForView(view);

            // Assert
            var expected = (layout.Constraint & LayoutConstraint.HorizontallyFixed) | LayoutConstraint.VerticallyFixed;
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView when WidthProportional flag is set with auto height (AutoSize).
        /// Should return constraint with only horizontally fixed flag.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_WidthProportionalWithAutoHeight_ReturnsHorizontallyFixed()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var view = Substitute.For<View>();
            var bounds = new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize); // Auto width, auto height

            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.WidthProportional);
            AbsoluteLayout.SetLayoutBounds(view, bounds);

            // Act
            var result = layout.TestComputeConstraintForView(view);

            // Assert
            var expected = layout.Constraint & LayoutConstraint.HorizontallyFixed;
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView when no proportional flags are set with both width and height as AutoSize.
        /// Should return LayoutConstraint.None.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_NoProportionalFlagsWithAutoSize_ReturnsNone()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var view = Substitute.For<View>();
            var bounds = new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);

            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(view, bounds);

            // Act
            var result = layout.TestComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.None, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView when no proportional flags are set with fixed width only.
        /// Should return LayoutConstraint.HorizontallyFixed.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_NoProportionalFlagsWithFixedWidthOnly_ReturnsHorizontallyFixed()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var view = Substitute.For<View>();
            var bounds = new Rect(0, 0, 100, AbsoluteLayout.AutoSize); // Fixed width, auto height

            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(view, bounds);

            // Act
            var result = layout.TestComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.HorizontallyFixed, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView when no proportional flags are set with fixed height only.
        /// Should return LayoutConstraint.VerticallyFixed.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_NoProportionalFlagsWithFixedHeightOnly_ReturnsVerticallyFixed()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var view = Substitute.For<View>();
            var bounds = new Rect(0, 0, AbsoluteLayout.AutoSize, 100); // Auto width, fixed height

            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(view, bounds);

            // Act
            var result = layout.TestComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.VerticallyFixed, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView when no proportional flags are set with both width and height fixed.
        /// Should return LayoutConstraint.Fixed (both horizontally and vertically fixed).
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_NoProportionalFlagsWithBothFixed_ReturnsFixed()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var view = Substitute.For<View>();
            var bounds = new Rect(0, 0, 100, 200); // Fixed width and height

            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.None);
            AbsoluteLayout.SetLayoutBounds(view, bounds);

            // Act
            var result = layout.TestComputeConstraintForView(view);

            // Assert
            Assert.Equal(LayoutConstraint.Fixed, result);
        }

        /// <summary>
        /// Tests ComputeConstraintForView with multiple flags combined (not SizeProportional).
        /// Should handle the first matching proportional flag encountered.
        /// </summary>
        [Fact]
        public void ComputeConstraintForView_MultipleFlags_HandlesFirstMatch()
        {
            // Arrange
            var layout = new TestableAbsoluteLayout();
            var view = Substitute.For<View>();
            var bounds = new Rect(0, 0, 100, 200); // Fixed width and height

            // Set both HeightProportional and WidthProportional - HeightProportional should be processed first
            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.HeightProportional | AbsoluteLayoutFlags.WidthProportional);
            AbsoluteLayout.SetLayoutBounds(view, bounds);

            // Act
            var result = layout.TestComputeConstraintForView(view);

            // Assert
            // Should process HeightProportional branch since it comes first in the if-else chain
            var expected = (layout.Constraint & LayoutConstraint.VerticallyFixed) | LayoutConstraint.HorizontallyFixed;
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Helper class to expose the protected ComputeConstraintForView method for testing.
        /// </summary>
        private class TestableAbsoluteLayout : AbsoluteLayout
        {
            public LayoutConstraint TestComputeConstraintForView(View view)
            {
                return ComputeConstraintForView(view);
            }
        }
    }
}