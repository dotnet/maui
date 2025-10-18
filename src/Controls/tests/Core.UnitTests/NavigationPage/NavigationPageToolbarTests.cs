#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Tests for NavigationPageToolbar class
    /// </summary>
    public class NavigationPageToolbarTests
    {
        /// <summary>
        /// Tests that ToolbarTracker property returns a non-null ToolbarTracker instance.
        /// Tests the basic functionality of the ToolbarTracker getter property.
        /// Expected result: Property should return a valid ToolbarTracker instance.
        /// </summary>
        [Fact]
        public void ToolbarTracker_WhenAccessed_ReturnsNonNullToolbarTracker()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var mockRootPage = Substitute.For<Page>();
            var toolbar = new NavigationPageToolbar(mockParent, mockRootPage);

            // Act
            var result = toolbar.ToolbarTracker;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ToolbarTracker>(result);
        }

        /// <summary>
        /// Tests that ToolbarTracker property returns the same instance on multiple calls.
        /// Tests the referential equality and consistency of the ToolbarTracker getter property.
        /// Expected result: Multiple calls should return the exact same object instance.
        /// </summary>
        [Fact]
        public void ToolbarTracker_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var mockRootPage = Substitute.For<Page>();
            var toolbar = new NavigationPageToolbar(mockParent, mockRootPage);

            // Act
            var firstCall = toolbar.ToolbarTracker;
            var secondCall = toolbar.ToolbarTracker;
            var thirdCall = toolbar.ToolbarTracker;

            // Assert
            Assert.Same(firstCall, secondCall);
            Assert.Same(firstCall, thirdCall);
            Assert.Same(secondCall, thirdCall);
        }

        /// <summary>
        /// Creates a NavigationPageToolbar instance for testing with mocked dependencies.
        /// </summary>
        /// <returns>A new NavigationPageToolbar instance with mocked parent and root page.</returns>
        private NavigationPageToolbar CreateNavigationPageToolbar()
        {
            var mockParent = Substitute.For<IElement>();
            var mockRootPage = Substitute.For<Page>();
            return new NavigationPageToolbar(mockParent, mockRootPage);
        }

        /// <summary>
        /// Sets the private _currentNavigationPage field using reflection.
        /// </summary>
        /// <param name="toolbar">The toolbar instance to modify.</param>
        /// <param name="navigationPage">The navigation page to set.</param>
        private void SetCurrentNavigationPage(NavigationPageToolbar toolbar, NavigationPage navigationPage)
        {
            var field = typeof(NavigationPageToolbar).GetField("_currentNavigationPage", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(toolbar, navigationPage);
        }

        /// <summary>
        /// Gets the private _barTextColor field value using reflection.
        /// </summary>
        /// <param name="toolbar">The toolbar instance to read from.</param>
        /// <returns>The current _barTextColor field value.</returns>
        private Color GetBarTextColorField(NavigationPageToolbar toolbar)
        {
            var field = typeof(NavigationPageToolbar).GetField("_barTextColor", BindingFlags.NonPublic | BindingFlags.Instance);
            return (Color)(field?.GetValue(toolbar) ?? new Color());
        }

        [Fact]
        public void BarTextColor_Get_WhenCurrentNavigationPageIsNull_ReturnsDefaultColor()
        {
            // Arrange
            var toolbar = CreateNavigationPageToolbar();
            SetCurrentNavigationPage(toolbar, null);

            // Act
            var result = toolbar.BarTextColor;

            // Assert
            Assert.Equal(new Color(), result);
        }

        [Fact]
        public void BarTextColor_Get_WhenCurrentNavigationPageHasBarTextColor_ReturnsNavigationPageBarTextColor()
        {
            // Arrange
            var toolbar = CreateNavigationPageToolbar();
            var navigationPage = new NavigationPage();
            var expectedColor = new Color(0.5f, 0.3f, 0.8f, 1.0f);
            navigationPage.BarTextColor = expectedColor;
            SetCurrentNavigationPage(toolbar, navigationPage);

            // Act
            var result = toolbar.BarTextColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        [Fact]
        public void BarTextColor_Get_WhenCurrentNavigationPageHasDefaultBarTextColor_ReturnsDefaultColor()
        {
            // Arrange
            var toolbar = CreateNavigationPageToolbar();
            var navigationPage = new NavigationPage();
            // BarTextColor is default when not explicitly set
            SetCurrentNavigationPage(toolbar, navigationPage);

            // Act
            var result = toolbar.BarTextColor;

            // Assert
            Assert.Equal(navigationPage.BarTextColor, result);
        }

        [Fact]
        public void BarTextColor_Set_WithNewColorValue_UpdatesBackingField()
        {
            // Arrange
            var toolbar = CreateNavigationPageToolbar();
            var newColor = new Color(0.2f, 0.4f, 0.6f, 0.8f);

            // Act
            toolbar.BarTextColor = newColor;

            // Assert
            var backingFieldValue = GetBarTextColorField(toolbar);
            Assert.Equal(newColor, backingFieldValue);
        }

        [Fact]
        public void BarTextColor_Set_WithSameColorValue_DoesNotRaisePropertyChanged()
        {
            // Arrange
            var toolbar = CreateNavigationPageToolbar();
            var color = new Color(0.1f, 0.2f, 0.3f, 0.4f);
            toolbar.BarTextColor = color; // Set initial value

            bool propertyChangedRaised = false;
            toolbar.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(NavigationPageToolbar.BarTextColor))
                    propertyChangedRaised = true;
            };

            // Act
            toolbar.BarTextColor = color; // Set same value

            // Assert
            Assert.False(propertyChangedRaised);
        }

        [Fact]
        public void BarTextColor_Set_WithDifferentColorValue_RaisesPropertyChanged()
        {
            // Arrange
            var toolbar = CreateNavigationPageToolbar();
            var initialColor = new Color(0.1f, 0.2f, 0.3f, 0.4f);
            var newColor = new Color(0.5f, 0.6f, 0.7f, 0.8f);
            toolbar.BarTextColor = initialColor;

            bool propertyChangedRaised = false;
            string propertyName = null;
            toolbar.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(NavigationPageToolbar.BarTextColor))
                {
                    propertyChangedRaised = true;
                    propertyName = e.PropertyName;
                }
            };

            // Act
            toolbar.BarTextColor = newColor;

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal(nameof(NavigationPageToolbar.BarTextColor), propertyName);
        }

        [Theory]
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent black
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // Opaque white
        [InlineData(0.5f, 0.0f, 1.0f, 0.5f)] // Semi-transparent purple
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Opaque red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Opaque green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Opaque blue
        public void BarTextColor_Set_WithVariousColorValues_UpdatesCorrectly(float red, float green, float blue, float alpha)
        {
            // Arrange
            var toolbar = CreateNavigationPageToolbar();
            var color = new Color(red, green, blue, alpha);

            // Act
            toolbar.BarTextColor = color;

            // Assert
            var backingFieldValue = GetBarTextColorField(toolbar);
            Assert.Equal(color, backingFieldValue);
            Assert.Equal(red, backingFieldValue.Red);
            Assert.Equal(green, backingFieldValue.Green);
            Assert.Equal(blue, backingFieldValue.Blue);
            Assert.Equal(alpha, backingFieldValue.Alpha);
        }

        [Fact]
        public void BarTextColor_Set_WithDefaultColor_UpdatesBackingField()
        {
            // Arrange
            var toolbar = CreateNavigationPageToolbar();
            var defaultColor = new Color(); // Default is black (0,0,0,1)

            // Act
            toolbar.BarTextColor = defaultColor;

            // Assert
            var backingFieldValue = GetBarTextColorField(toolbar);
            Assert.Equal(defaultColor, backingFieldValue);
            Assert.Equal(0.0f, backingFieldValue.Red);
            Assert.Equal(0.0f, backingFieldValue.Green);
            Assert.Equal(0.0f, backingFieldValue.Blue);
            Assert.Equal(1.0f, backingFieldValue.Alpha);
        }

        [Fact]
        public void BarTextColor_SetMultipleTimes_EachDifferentValueRaisesPropertyChanged()
        {
            // Arrange
            var toolbar = CreateNavigationPageToolbar();
            var color1 = new Color(1.0f, 0.0f, 0.0f, 1.0f); // Red
            var color2 = new Color(0.0f, 1.0f, 0.0f, 1.0f); // Green
            var color3 = new Color(0.0f, 0.0f, 1.0f, 1.0f); // Blue

            int propertyChangedCount = 0;
            toolbar.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(NavigationPageToolbar.BarTextColor))
                    propertyChangedCount++;
            };

            // Act
            toolbar.BarTextColor = color1;
            toolbar.BarTextColor = color2;
            toolbar.BarTextColor = color3;

            // Assert
            Assert.Equal(3, propertyChangedCount);
            var finalValue = GetBarTextColorField(toolbar);
            Assert.Equal(color3, finalValue);
        }

        /// <summary>
        /// Tests that IconColor getter returns the color from current page when available.
        /// Input: Current page has an icon color set, navigation page may or may not have one.
        /// Expected: Returns the current page's icon color.
        /// </summary>
        [Fact]
        public void IconColor_Get_ReturnsCurrentPageIconColor_WhenCurrentPageHasIconColor()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(parent, rootPage);
            var currentPage = new ContentPage();
            var currentNavigationPage = new NavigationPage();
            var expectedColor = Colors.Red;

            NavigationPage.SetIconColor(currentPage, expectedColor);
            NavigationPage.SetIconColor(currentNavigationPage, Colors.Blue);

            // Use reflection to set private fields since they can't be mocked
            var currentPageField = typeof(NavigationPageToolbar).GetField("_currentPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var currentNavigationPageField = typeof(NavigationPageToolbar).GetField("_currentNavigationPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            currentPageField.SetValue(toolbar, currentPage);
            currentNavigationPageField.SetValue(toolbar, currentNavigationPage);

            // Act
            var result = toolbar.IconColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that IconColor getter returns the color from navigation page when current page has no icon color.
        /// Input: Current page has no icon color, navigation page has an icon color set.
        /// Expected: Returns the navigation page's icon color.
        /// </summary>
        [Fact]
        public void IconColor_Get_ReturnsNavigationPageIconColor_WhenCurrentPageHasNoIconColor()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(parent, rootPage);
            var currentPage = new ContentPage();
            var currentNavigationPage = new NavigationPage();
            var expectedColor = Colors.Blue;

            // Current page has no icon color set (null)
            NavigationPage.SetIconColor(currentNavigationPage, expectedColor);

            var currentPageField = typeof(NavigationPageToolbar).GetField("_currentPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var currentNavigationPageField = typeof(NavigationPageToolbar).GetField("_currentNavigationPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            currentPageField.SetValue(toolbar, currentPage);
            currentNavigationPageField.SetValue(toolbar, currentNavigationPage);

            // Act
            var result = toolbar.IconColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that IconColor getter returns null when both current page and navigation page have no icon color.
        /// Input: Both current page and navigation page have no icon color set.
        /// Expected: Returns null.
        /// </summary>
        [Fact]
        public void IconColor_Get_ReturnsNull_WhenBothPageAndNavigationPageHaveNoIconColor()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(parent, rootPage);
            var currentPage = new ContentPage();
            var currentNavigationPage = new NavigationPage();

            // Neither page has icon color set
            var currentPageField = typeof(NavigationPageToolbar).GetField("_currentPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var currentNavigationPageField = typeof(NavigationPageToolbar).GetField("_currentNavigationPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            currentPageField.SetValue(toolbar, currentPage);
            currentNavigationPageField.SetValue(toolbar, currentNavigationPage);

            // Act
            var result = toolbar.IconColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that IconColor getter returns null when current page is null.
        /// Input: Current page is null, navigation page may or may not have icon color.
        /// Expected: Returns navigation page's icon color or null.
        /// </summary>
        [Fact]
        public void IconColor_Get_ReturnsNavigationPageIconColor_WhenCurrentPageIsNull()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(parent, rootPage);
            var currentNavigationPage = new NavigationPage();
            var expectedColor = Colors.Green;

            NavigationPage.SetIconColor(currentNavigationPage, expectedColor);

            // Current page is null
            var currentPageField = typeof(NavigationPageToolbar).GetField("_currentPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var currentNavigationPageField = typeof(NavigationPageToolbar).GetField("_currentNavigationPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            currentPageField.SetValue(toolbar, null);
            currentNavigationPageField.SetValue(toolbar, currentNavigationPage);

            // Act
            var result = toolbar.IconColor;

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that IconColor getter returns null when both current page and navigation page are null.
        /// Input: Both current page and navigation page are null.
        /// Expected: Returns null.
        /// </summary>
        [Fact]
        public void IconColor_Get_ReturnsNull_WhenBothPagesAreNull()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(parent, rootPage);

            var currentPageField = typeof(NavigationPageToolbar).GetField("_currentPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var currentNavigationPageField = typeof(NavigationPageToolbar).GetField("_currentNavigationPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            currentPageField.SetValue(toolbar, null);
            currentNavigationPageField.SetValue(toolbar, null);

            // Act
            var result = toolbar.IconColor;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that IconColor setter updates the backing field and raises property change notifications.
        /// Input: Setting a valid color value.
        /// Expected: Backing field is updated, PropertyChanged event is raised, Handler.UpdateValue is called.
        /// </summary>
        [Fact]
        public void IconColor_Set_UpdatesBackingFieldAndRaisesNotifications_WhenValueChanges()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(parent, rootPage);
            var newColor = Colors.Purple;
            var propertyChangedRaised = false;
            var propertyName = string.Empty;

            var handler = Substitute.For<IToolbarHandler>();
            toolbar.Handler = handler;

            toolbar.PropertyChanged += (s, e) =>
            {
                propertyChangedRaised = true;
                propertyName = e.PropertyName;
            };

            // Act
            toolbar.IconColor = newColor;

            // Assert
            Assert.Equal(newColor, toolbar.IconColor);
            Assert.True(propertyChangedRaised);
            Assert.Equal("IconColor", propertyName);
            handler.Received(1).UpdateValue("IconColor");
        }

        /// <summary>
        /// Tests that IconColor setter does not raise notifications when setting the same value.
        /// Input: Setting the same color value twice.
        /// Expected: PropertyChanged event is not raised on second set, Handler.UpdateValue is not called.
        /// </summary>
        [Fact]
        public void IconColor_Set_DoesNotRaiseNotifications_WhenValueIsSame()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(parent, rootPage);
            var color = Colors.Orange;
            var propertyChangedCount = 0;

            var handler = Substitute.For<IToolbarHandler>();
            toolbar.Handler = handler;

            toolbar.PropertyChanged += (s, e) => propertyChangedCount++;

            // Set initial value
            toolbar.IconColor = color;
            Assert.Equal(1, propertyChangedCount);
            handler.Received(1).UpdateValue("IconColor");

            // Act - set same value again
            toolbar.IconColor = color;

            // Assert
            Assert.Equal(1, propertyChangedCount); // Should still be 1, not 2
            handler.Received(1).UpdateValue("IconColor"); // Should still be called only once
        }

        /// <summary>
        /// Tests that IconColor setter handles null values correctly.
        /// Input: Setting null as icon color value.
        /// Expected: Backing field is set to null, notifications are raised.
        /// </summary>
        [Fact]
        public void IconColor_Set_HandlesNullValue_WhenSettingToNull()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(parent, rootPage);
            var propertyChangedRaised = false;

            var handler = Substitute.For<IToolbarHandler>();
            toolbar.Handler = handler;

            // First set to a non-null value
            toolbar.IconColor = Colors.Red;

            toolbar.PropertyChanged += (s, e) => propertyChangedRaised = true;

            // Act
            toolbar.IconColor = null;

            // Assert
            Assert.Null(toolbar.IconColor);
            Assert.True(propertyChangedRaised);
            handler.Received().UpdateValue("IconColor");
        }

        /// <summary>
        /// Tests IconColor with various edge case color values including transparent and extreme values.
        /// Input: Various color values including transparent, extremes, and edge cases.
        /// Expected: All values are handled correctly.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetEdgeCaseColors))]
        public void IconColor_Set_HandlesEdgeCaseColors_WhenSettingVariousValues(Color testColor, string testName)
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(parent, rootPage);

            // Act & Assert - should not throw
            toolbar.IconColor = testColor;
            Assert.Equal(testColor, toolbar.IconColor);
        }

        /// <summary>
        /// Provides edge case color values for parameterized testing.
        /// </summary>
        public static IEnumerable<object[]> GetEdgeCaseColors()
        {
            yield return new object[] { Colors.Transparent, "Transparent" };
            yield return new object[] { new Color(0, 0, 0, 0), "Full Transparent Black" };
            yield return new object[] { new Color(1, 1, 1, 1), "Full Opaque White" };
            yield return new object[] { new Color(0.5f), "Gray" };
            yield return new object[] { new Color(float.Epsilon, float.Epsilon, float.Epsilon, float.Epsilon), "Near Zero Values" };
            yield return new object[] { new Color(1 - float.Epsilon, 1 - float.Epsilon, 1 - float.Epsilon, 1 - float.Epsilon), "Near Max Values" };
        }

        /// <summary>
        /// Tests that the DrawerToggleVisible property getter returns the current field value.
        /// Input: Default initialized NavigationPageToolbar instance.
        /// Expected result: Property returns false (default bool value).
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_DefaultValue_ReturnsFalse()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = Substitute.For<Page>();
            var toolbar = new NavigationPageToolbar(parent, rootPage);

            // Act
            bool result = toolbar.DrawerToggleVisible;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that setting DrawerToggleVisible to true updates the property value.
        /// Input: Setting property to true.
        /// Expected result: Property getter returns true.
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = Substitute.For<Page>();
            var toolbar = new NavigationPageToolbar(parent, rootPage);

            // Act
            toolbar.DrawerToggleVisible = true;

            // Assert
            Assert.True(toolbar.DrawerToggleVisible);
        }

        /// <summary>
        /// Tests that setting DrawerToggleVisible to false updates the property value.
        /// Input: Setting property to false after setting to true.
        /// Expected result: Property getter returns false.
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = Substitute.For<Page>();
            var toolbar = new NavigationPageToolbar(parent, rootPage);
            toolbar.DrawerToggleVisible = true;

            // Act
            toolbar.DrawerToggleVisible = false;

            // Assert
            Assert.False(toolbar.DrawerToggleVisible);
        }

        /// <summary>
        /// Tests that setting DrawerToggleVisible to different values raises PropertyChanged event.
        /// Input: Setting property to true, then to false.
        /// Expected result: PropertyChanged event is raised twice with correct property name.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DrawerToggleVisible_SetToDifferentValue_RaisesPropertyChanged(bool newValue)
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = Substitute.For<Page>();
            var toolbar = new NavigationPageToolbar(parent, rootPage);
            var propertyChangedRaised = false;
            string propertyName = null;

            toolbar.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                propertyName = args.PropertyName;
            };

            // Act
            toolbar.DrawerToggleVisible = newValue;

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal("DrawerToggleVisible", propertyName);
        }

        /// <summary>
        /// Tests that setting DrawerToggleVisible to the same value does not raise PropertyChanged event.
        /// Input: Setting property to false when it's already false.
        /// Expected result: PropertyChanged event is not raised.
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_SetToSameValue_DoesNotRaisePropertyChanged()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = Substitute.For<Page>();
            var toolbar = new NavigationPageToolbar(parent, rootPage);
            var propertyChangedRaised = false;

            toolbar.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
            };

            // Act
            toolbar.DrawerToggleVisible = false; // Setting to default value (false)

            // Assert
            Assert.False(propertyChangedRaised);
        }

        /// <summary>
        /// Tests that setting DrawerToggleVisible calls Handler.UpdateValue when Handler is set.
        /// Input: Setting property to true with a mocked handler.
        /// Expected result: Handler.UpdateValue is called with correct property name.
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_SetWithHandler_CallsHandlerUpdateValue()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = Substitute.For<Page>();
            var handler = Substitute.For<IElementHandler>();
            var toolbar = new NavigationPageToolbar(parent, rootPage);
            toolbar.Handler = handler;

            // Act
            toolbar.DrawerToggleVisible = true;

            // Assert
            handler.Received(1).UpdateValue("DrawerToggleVisible");
        }

        /// <summary>
        /// Tests that setting DrawerToggleVisible does not call Handler.UpdateValue when Handler is null.
        /// Input: Setting property to true with null handler.
        /// Expected result: No exception is thrown and property value is updated.
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_SetWithNullHandler_DoesNotThrow()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = Substitute.For<Page>();
            var toolbar = new NavigationPageToolbar(parent, rootPage);
            // Handler is null by default

            // Act & Assert
            var exception = Record.Exception(() => toolbar.DrawerToggleVisible = true);
            Assert.Null(exception);
            Assert.True(toolbar.DrawerToggleVisible);
        }

        /// <summary>
        /// Tests that setting DrawerToggleVisible to the same value does not call Handler.UpdateValue.
        /// Input: Setting property to false when it's already false with a mocked handler.
        /// Expected result: Handler.UpdateValue is not called.
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_SetSameValueWithHandler_DoesNotCallHandlerUpdateValue()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = Substitute.For<Page>();
            var handler = Substitute.For<IElementHandler>();
            var toolbar = new NavigationPageToolbar(parent, rootPage);
            toolbar.Handler = handler;

            // Act
            toolbar.DrawerToggleVisible = false; // Setting to default value (false)

            // Assert
            handler.DidNotReceive().UpdateValue(Arg.Any<string>());
        }

        /// <summary>
        /// Tests that the NavigationPageToolbar constructor completes successfully with valid parent and rootPage parameters,
        /// and correctly sets the RootPage property.
        /// </summary>
        [Fact]
        public void NavigationPageToolbar_ValidParameters_ConstructsSuccessfully()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var rootPage = Substitute.For<Page>();

            // Act
            var toolbar = new NavigationPageToolbar(parent, rootPage);

            // Assert
            Assert.NotNull(toolbar);
            Assert.Equal(rootPage, toolbar.RootPage);
        }

        /// <summary>
        /// Tests that the NavigationPageToolbar constructor handles null parent parameter gracefully
        /// and correctly sets the RootPage property.
        /// </summary>
        [Fact]
        public void NavigationPageToolbar_NullParent_ConstructsSuccessfully()
        {
            // Arrange
            IElement parent = null;
            var rootPage = Substitute.For<Page>();

            // Act
            var toolbar = new NavigationPageToolbar(parent, rootPage);

            // Assert
            Assert.NotNull(toolbar);
            Assert.Equal(rootPage, toolbar.RootPage);
        }

        /// <summary>
        /// Tests that the NavigationPageToolbar constructor handles null rootPage parameter
        /// and sets the RootPage property to null.
        /// </summary>
        [Fact]
        public void NavigationPageToolbar_NullRootPage_ConstructsSuccessfully()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            Page rootPage = null;

            // Act
            var toolbar = new NavigationPageToolbar(parent, rootPage);

            // Assert
            Assert.NotNull(toolbar);
            Assert.Null(toolbar.RootPage);
        }

        /// <summary>
        /// Tests that the NavigationPageToolbar constructor handles both null parent and null rootPage parameters
        /// and sets the RootPage property to null.
        /// </summary>
        [Fact]
        public void NavigationPageToolbar_BothParametersNull_ConstructsSuccessfully()
        {
            // Arrange
            IElement parent = null;
            Page rootPage = null;

            // Act
            var toolbar = new NavigationPageToolbar(parent, rootPage);

            // Assert
            Assert.NotNull(toolbar);
            Assert.Null(toolbar.RootPage);
        }

        /// <summary>
        /// Tests the NavigationPageToolbar constructor with various parameter combinations using parameterized test data.
        /// Verifies that construction completes successfully and RootPage is set correctly for each combination.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetConstructorTestData))]
        public void NavigationPageToolbar_ParameterCombinations_ConstructsCorrectly(IElement parent, Page rootPage, Page expectedRootPage)
        {
            // Act
            var toolbar = new NavigationPageToolbar(parent, rootPage);

            // Assert
            Assert.NotNull(toolbar);
            Assert.Equal(expectedRootPage, toolbar.RootPage);
        }

        public static TheoryData<IElement, Page, Page> GetConstructorTestData()
        {
            var mockParent = Substitute.For<IElement>();
            var mockRootPage = Substitute.For<Page>();

            return new TheoryData<IElement, Page, Page>
            {
                { mockParent, mockRootPage, mockRootPage },
                { mockParent, null, null },
                { null, mockRootPage, mockRootPage },
                { null, null, null }
            };
        }

        /// <summary>
        /// Tests that BackButtonVisible returns false when the toolbar is in its initial state
        /// with no current page set (i.e., _currentPage is null).
        /// Expected result: false
        /// </summary>
        [Fact]
        public void BackButtonVisible_WhenCurrentPageIsNull_ReturnsFalse()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(mockParent, rootPage);

            // Act
            var result = toolbar.BackButtonVisible;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that BackButtonVisible returns false when current page is set but navigation stack is empty.
        /// Input conditions: ContentPage with no navigation stack
        /// Expected result: false
        /// </summary>
        [Fact]
        public void BackButtonVisible_WhenNavigationStackIsEmpty_ReturnsFalse()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var navigationPage = new NavigationPage(rootPage);
            var toolbar = new NavigationPageToolbar(mockParent, rootPage);

            // Act
            var result = toolbar.BackButtonVisible;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that BackButtonVisible returns false when current page has back button disabled via NavigationPage.SetHasBackButton.
        /// Input conditions: ContentPage in NavigationPage with HasBackButton set to false
        /// Expected result: false
        /// </summary>
        [Fact]
        public void BackButtonVisible_WhenHasBackButtonIsFalse_ReturnsFalse()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            NavigationPage.SetHasBackButton(rootPage, false);
            var navigationPage = new NavigationPage(rootPage);
            var toolbar = new NavigationPageToolbar(mockParent, rootPage);

            // Act
            var result = toolbar.BackButtonVisible;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that BackButtonVisible getter calls GetBackButtonVisible method and returns its result.
        /// Input conditions: Toolbar in various states
        /// Expected result: Consistent behavior between getter calls
        /// </summary>
        [Fact]
        public void BackButtonVisible_Getter_CallsGetBackButtonVisibleMethod()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(mockParent, rootPage);

            // Act - Multiple calls to ensure consistent behavior
            var result1 = toolbar.BackButtonVisible;
            var result2 = toolbar.BackButtonVisible;

            // Assert
            Assert.Equal(result1, result2);
            Assert.False(result1); // Should be false in initial state
        }

        /// <summary>
        /// Tests BackButtonVisible property setter and getter interaction.
        /// Input conditions: Setting _backButtonVisible field via setter
        /// Expected result: Getter still calls GetBackButtonVisible, not affected by setter
        /// </summary>
        [Fact]
        public void BackButtonVisible_SetterDoesNotAffectGetter_GetterCallsGetBackButtonVisible()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(mockParent, rootPage);

            // Act
            toolbar.BackButtonVisible = true; // This sets _backButtonVisible field
            var getterResult = toolbar.BackButtonVisible; // This calls GetBackButtonVisible()

            // Assert
            // The getter should still return false because GetBackButtonVisible() returns false when _currentPage is null
            // The setter only affects _backButtonVisible field, but getter calls GetBackButtonVisible() method
            Assert.False(getterResult);
        }

        /// <summary>
        /// Tests BackButtonVisible with null parent element.
        /// Input conditions: null parent IElement
        /// Expected result: Should not throw exception and return false
        /// </summary>
        [Fact]
        public void BackButtonVisible_WithNullParent_ReturnsFalse()
        {
            // Arrange & Act
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(null, rootPage);
            var result = toolbar.BackButtonVisible;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests BackButtonVisible with null root page.
        /// Input conditions: null root page
        /// Expected result: Should not throw exception and return false
        /// </summary>
        [Fact]
        public void BackButtonVisible_WithNullRootPage_ReturnsFalse()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();

            // Act
            var toolbar = new NavigationPageToolbar(mockParent, null);
            var result = toolbar.BackButtonVisible;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CurrentNavigationPage returns null when the toolbar is initially created
        /// and no navigation page has been set through the page appearing lifecycle.
        /// </summary>
        [Fact]
        public void CurrentNavigationPage_InitialState_ReturnsNull()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var rootPage = new ContentPage();

            // Act
            var toolbar = new NavigationPageToolbar(mockParent, rootPage);
            var result = toolbar.CurrentNavigationPage;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that CurrentNavigationPage returns the correct NavigationPage instance
        /// when a page with a NavigationPage parent triggers the page appearing event.
        /// </summary>
        [Fact]
        public void CurrentNavigationPage_AfterPageAppearingWithNavigationPageParent_ReturnsNavigationPage()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(mockParent, rootPage);

            // Create a navigation page and content page hierarchy
            var navigationPage = new NavigationPage();
            var contentPage = new ContentPage();
            navigationPage.PushAsync(contentPage);

            // Act - Trigger the PageAppearing event that sets _currentNavigationPage
            toolbar.ToolbarTracker.Target = contentPage;

            // Get the result
            var result = toolbar.CurrentNavigationPage;

            // Assert
            Assert.Equal(navigationPage, result);
        }

        /// <summary>
        /// Tests that CurrentNavigationPage returns null when a page without 
        /// a NavigationPage parent triggers the page appearing event.
        /// </summary>
        [Fact]
        public void CurrentNavigationPage_AfterPageAppearingWithoutNavigationPageParent_ReturnsNull()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(mockParent, rootPage);

            // Create a standalone content page without NavigationPage parent
            var contentPage = new ContentPage();

            // Act - Trigger the PageAppearing event
            toolbar.ToolbarTracker.Target = contentPage;

            // Get the result
            var result = toolbar.CurrentNavigationPage;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that CurrentNavigationPage consistently returns the same instance
        /// when accessed multiple times without state changes.
        /// </summary>
        [Fact]
        public void CurrentNavigationPage_MultipleAccesses_ReturnsSameInstance()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(mockParent, rootPage);

            var navigationPage = new NavigationPage();
            var contentPage = new ContentPage();
            navigationPage.PushAsync(contentPage);

            // Set up the navigation page
            toolbar.ToolbarTracker.Target = contentPage;

            // Act
            var result1 = toolbar.CurrentNavigationPage;
            var result2 = toolbar.CurrentNavigationPage;
            var result3 = toolbar.CurrentNavigationPage;

            // Assert
            Assert.Same(result1, result2);
            Assert.Same(result2, result3);
            Assert.Equal(navigationPage, result1);
        }

        /// <summary>
        /// Tests that CurrentNavigationPage property reflects changes when the internal 
        /// navigation page changes through different page appearing events.
        /// </summary>
        [Fact]
        public void CurrentNavigationPage_NavigationPageChanges_ReflectsNewNavigationPage()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var rootPage = new ContentPage();
            var toolbar = new NavigationPageToolbar(mockParent, rootPage);

            // First navigation page setup
            var navigationPage1 = new NavigationPage();
            var contentPage1 = new ContentPage();
            navigationPage1.PushAsync(contentPage1);

            // Second navigation page setup
            var navigationPage2 = new NavigationPage();
            var contentPage2 = new ContentPage();
            navigationPage2.PushAsync(contentPage2);

            // Act & Assert - First navigation page
            toolbar.ToolbarTracker.Target = contentPage1;
            var result1 = toolbar.CurrentNavigationPage;
            Assert.Equal(navigationPage1, result1);

            // Act & Assert - Second navigation page
            toolbar.ToolbarTracker.Target = contentPage2;
            var result2 = toolbar.CurrentNavigationPage;
            Assert.Equal(navigationPage2, result2);

            // Verify they are different instances
            Assert.NotSame(result1, result2);
        }
    }
}