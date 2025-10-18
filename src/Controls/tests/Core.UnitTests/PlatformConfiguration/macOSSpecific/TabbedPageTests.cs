#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.macOSSpecific
{
    public partial class TabbedPageTests
    {
        /// <summary>
        /// Tests that SetTabsStyle throws NullReferenceException when element parameter is null.
        /// Input: null element, any TabsStyle value
        /// Expected: NullReferenceException is thrown
        /// </summary>
        [Fact]
        public void SetTabsStyle_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            var value = TabsStyle.Default;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TabbedPage.SetTabsStyle(element, value));
        }

        /// <summary>
        /// Tests that SetTabsStyle calls SetValue with correct parameters for all valid TabsStyle enum values.
        /// Input: valid BindableObject, each valid TabsStyle enum value
        /// Expected: SetValue is called with TabsStyleProperty and the specified value
        /// </summary>
        [Theory]
        [InlineData(TabsStyle.Default)]
        [InlineData(TabsStyle.Hidden)]
        [InlineData(TabsStyle.Icons)]
        [InlineData(TabsStyle.OnNavigation)]
        [InlineData(TabsStyle.OnBottom)]
        public void SetTabsStyle_ValidEnumValues_CallsSetValueWithCorrectParameters(TabsStyle value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            TabbedPage.SetTabsStyle(element, value);

            // Assert
            element.Received(1).SetValue(TabbedPage.TabsStyleProperty, value);
        }

        /// <summary>
        /// Tests that SetTabsStyle handles invalid TabsStyle enum values by casting integers outside the defined range.
        /// Input: valid BindableObject, invalid TabsStyle values (cast from integers)
        /// Expected: SetValue is called with TabsStyleProperty and the invalid enum value
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(5)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(999)]
        public void SetTabsStyle_InvalidEnumValues_CallsSetValueWithCastValue(int invalidValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = (TabsStyle)invalidValue;

            // Act
            TabbedPage.SetTabsStyle(element, value);

            // Assert
            element.Received(1).SetValue(TabbedPage.TabsStyleProperty, value);
        }

        /// <summary>
        /// Tests that SetTabsStyle works correctly with boundary enum values.
        /// Input: valid BindableObject, first and last defined enum values
        /// Expected: SetValue is called with TabsStyleProperty and the specified value
        /// </summary>
        [Theory]
        [InlineData(TabsStyle.Default)] // First enum value (0)
        [InlineData(TabsStyle.OnBottom)] // Last enum value (4)
        public void SetTabsStyle_BoundaryEnumValues_CallsSetValueWithCorrectParameters(TabsStyle value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            TabbedPage.SetTabsStyle(element, value);

            // Assert
            element.Received(1).SetValue(TabbedPage.TabsStyleProperty, value);
        }

        /// <summary>
        /// Tests that SetShowTabs throws ArgumentNullException when config parameter is null.
        /// </summary>
        [Fact]
        public void SetShowTabs_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<macOS, Controls.TabbedPage> config = null;
            TabsStyle value = TabsStyle.Default;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.SetShowTabs(value));
        }

        /// <summary>
        /// Tests that SetShowTabs correctly sets the TabsStyle value and returns the same config object.
        /// Verifies that SetValue is called on the Element with the correct BindableProperty and value.
        /// </summary>
        /// <param name="tabsStyle">The TabsStyle value to set.</param>
        [Theory]
        [InlineData(TabsStyle.Default)]
        [InlineData(TabsStyle.Hidden)]
        [InlineData(TabsStyle.Icons)]
        [InlineData(TabsStyle.OnNavigation)]
        [InlineData(TabsStyle.OnBottom)]
        public void SetShowTabs_ValidTabsStyle_SetsValueAndReturnsConfig(TabsStyle tabsStyle)
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<macOS, Controls.TabbedPage>>();
            var mockTabbedPage = Substitute.For<Controls.TabbedPage>();
            mockConfig.Element.Returns(mockTabbedPage);

            // Act
            var result = mockConfig.SetShowTabs(tabsStyle);

            // Assert
            mockTabbedPage.Received(1).SetValue(TabbedPage.TabsStyleProperty, tabsStyle);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetShowTabs works with invalid enum values that are cast to TabsStyle.
        /// Verifies that even invalid enum values are passed through correctly.
        /// </summary>
        [Fact]
        public void SetShowTabs_InvalidTabsStyleValue_SetsValueAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<macOS, Controls.TabbedPage>>();
            var mockTabbedPage = Substitute.For<Controls.TabbedPage>();
            mockConfig.Element.Returns(mockTabbedPage);
            var invalidTabsStyle = (TabsStyle)999;

            // Act
            var result = mockConfig.SetShowTabs(invalidTabsStyle);

            // Assert
            mockTabbedPage.Received(1).SetValue(TabbedPage.TabsStyleProperty, invalidTabsStyle);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetShowTabs enables method chaining by returning the same config object.
        /// Verifies the fluent interface pattern is properly implemented.
        /// </summary>
        [Fact]
        public void SetShowTabs_ValidInput_EnablesMethodChaining()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<macOS, Controls.TabbedPage>>();
            var mockTabbedPage = Substitute.For<Controls.TabbedPage>();
            mockConfig.Element.Returns(mockTabbedPage);

            // Act
            var result1 = mockConfig.SetShowTabs(TabsStyle.Hidden);
            var result2 = result1.SetShowTabs(TabsStyle.Icons);

            // Assert
            Assert.Same(mockConfig, result1);
            Assert.Same(mockConfig, result2);
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that HideTabs method returns the same config object when provided with a valid configuration.
        /// This test validates the fluent API pattern and ensures the method executes successfully with valid inputs.
        /// Expected result: The same config object is returned unchanged.
        /// </summary>
        [Fact]
        public void HideTabs_ValidConfig_ReturnsSameConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<macOS, TabbedPage>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = TabbedPage.HideTabs(mockConfig);

            // Assert
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that HideTabs method throws ArgumentNullException when provided with a null config parameter.
        /// This test validates the method's null parameter handling and boundary condition behavior.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void HideTabs_NullConfig_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => TabbedPage.HideTabs(null));
        }

        /// <summary>
        /// Tests that HideTabs method throws NullReferenceException when config.Element is null.
        /// This test validates the method's behavior when the Element property returns null, 
        /// which should cause SetTabsStyle to fail when trying to call SetValue on a null object.
        /// Expected result: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void HideTabs_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<macOS, TabbedPage>>();
            mockConfig.Element.Returns((BindableObject)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TabbedPage.HideTabs(mockConfig));
        }

        /// <summary>
        /// Tests that GetTabsStyle throws ArgumentNullException when element parameter is null.
        /// Validates that null input is properly handled and the appropriate exception is thrown.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void GetTabsStyle_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TabbedPage.GetTabsStyle(element));
        }

        /// <summary>
        /// Tests that GetTabsStyle returns the correct TabsStyle value for valid enum values.
        /// Validates that the method properly retrieves and casts TabsStyle values from BindableObject.
        /// Expected result: The method should return the TabsStyle value that was set on the element.
        /// </summary>
        [Theory]
        [InlineData(TabsStyle.Default)]
        [InlineData(TabsStyle.Hidden)]
        [InlineData(TabsStyle.Icons)]
        [InlineData(TabsStyle.OnNavigation)]
        [InlineData(TabsStyle.OnBottom)]
        public void GetTabsStyle_ValidElement_ReturnsCorrectTabsStyle(TabsStyle expectedTabsStyle)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TabbedPage.TabsStyleProperty).Returns(expectedTabsStyle);

            // Act
            var result = TabbedPage.GetTabsStyle(element);

            // Assert
            Assert.Equal(expectedTabsStyle, result);
        }

        /// <summary>
        /// Tests that GetTabsStyle handles integer values that correspond to TabsStyle enum values.
        /// Validates that the casting mechanism works correctly with boxed integer values.
        /// Expected result: The method should successfully cast integer values to TabsStyle enum.
        /// </summary>
        [Theory]
        [InlineData(0, TabsStyle.Default)]
        [InlineData(1, TabsStyle.Hidden)]
        [InlineData(2, TabsStyle.Icons)]
        [InlineData(3, TabsStyle.OnNavigation)]
        [InlineData(4, TabsStyle.OnBottom)]
        public void GetTabsStyle_ValidElementWithIntegerValue_ReturnsCorrectTabsStyle(int intValue, TabsStyle expectedTabsStyle)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TabbedPage.TabsStyleProperty).Returns(intValue);

            // Act
            var result = TabbedPage.GetTabsStyle(element);

            // Assert
            Assert.Equal(expectedTabsStyle, result);
        }

        /// <summary>
        /// Tests that GetTabsStyle handles edge case with out-of-range enum values.
        /// Validates behavior when an invalid integer value is cast to TabsStyle enum.
        /// Expected result: The method should return the cast value even if it's outside defined enum range.
        /// </summary>
        [Fact]
        public void GetTabsStyle_ValidElementWithOutOfRangeValue_ReturnsTabsStyleValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var outOfRangeValue = 999;
            element.GetValue(TabbedPage.TabsStyleProperty).Returns(outOfRangeValue);

            // Act
            var result = TabbedPage.GetTabsStyle(element);

            // Assert
            Assert.Equal((TabsStyle)outOfRangeValue, result);
        }

        /// <summary>
        /// Tests that GetTabsStyle throws InvalidCastException when GetValue returns incompatible type.
        /// Validates that the casting mechanism properly handles type mismatches.
        /// Expected result: InvalidCastException should be thrown for incompatible types.
        /// </summary>
        [Fact]
        public void GetTabsStyle_ValidElementWithIncompatibleType_ThrowsInvalidCastException()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TabbedPage.TabsStyleProperty).Returns("InvalidType");

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => TabbedPage.GetTabsStyle(element));
        }
    }
}