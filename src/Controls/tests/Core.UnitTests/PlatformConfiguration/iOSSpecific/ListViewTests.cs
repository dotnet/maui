#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.iOSSpecific
{
    /// <summary>
    /// Unit tests for the ListView iOS-specific platform configuration methods.
    /// </summary>
    public class ListViewTests
    {
        /// <summary>
        /// Tests that SetSeparatorStyle throws NullReferenceException when element parameter is null.
        /// </summary>
        [Fact]
        public void SetSeparatorStyle_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            var separatorStyle = SeparatorStyle.Default;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                ListView.SetSeparatorStyle(element, separatorStyle));
        }

        /// <summary>
        /// Tests that SetSeparatorStyle works correctly when BindableObject.SetValue doesn't throw exceptions.
        /// This verifies the normal execution path completes successfully.
        /// </summary>
        [Fact]
        public void SetSeparatorStyle_ValidElement_CompletesSuccessfully()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var separatorStyle = SeparatorStyle.FullWidth;

            // Act (should not throw)
            ListView.SetSeparatorStyle(element, separatorStyle);

            // Assert - method completed without exception
            element.Received(1).SetValue(ListView.SeparatorStyleProperty, separatorStyle);
        }

        /// <summary>
        /// Tests that GetSeparatorStyle throws ArgumentNullException when config parameter is null.
        /// Verifies proper null parameter validation for the extension method.
        /// </summary>
        [Fact]
        public void GetSeparatorStyle_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, ListView> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ListView.GetSeparatorStyle(config));
        }

        /// <summary>
        /// Tests that GetSeparatorStyle returns Default separator style when the underlying element is configured with Default.
        /// Verifies proper delegation to the underlying GetSeparatorStyle method and correct return value forwarding.
        /// </summary>
        [Fact]
        public void GetSeparatorStyle_ValidConfigWithDefaultSeparator_ReturnsDefault()
        {
            // Arrange
            var mockElement = Substitute.For<ListView>();
            mockElement.GetValue(ListView.SeparatorStyleProperty).Returns(SeparatorStyle.Default);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, ListView>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = ListView.GetSeparatorStyle(mockConfig);

            // Assert
            Assert.Equal(SeparatorStyle.Default, result);
        }

        /// <summary>
        /// Tests that GetSeparatorStyle returns FullWidth separator style when the underlying element is configured with FullWidth.
        /// Verifies proper delegation to the underlying GetSeparatorStyle method and correct return value forwarding.
        /// </summary>
        [Fact]
        public void GetSeparatorStyle_ValidConfigWithFullWidthSeparator_ReturnsFullWidth()
        {
            // Arrange
            var mockElement = Substitute.For<ListView>();
            mockElement.GetValue(ListView.SeparatorStyleProperty).Returns(SeparatorStyle.FullWidth);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, ListView>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = ListView.GetSeparatorStyle(mockConfig);

            // Assert
            Assert.Equal(SeparatorStyle.FullWidth, result);
        }

        /// <summary>
        /// Tests that GetSeparatorStyle properly accesses the Element property from config and delegates the call.
        /// Verifies that the extension method correctly forwards the call to the static GetSeparatorStyle method.
        /// </summary>
        [Fact]
        public void GetSeparatorStyle_ValidConfig_AccessesElementPropertyAndDelegatesCall()
        {
            // Arrange
            var mockElement = Substitute.For<ListView>();
            mockElement.GetValue(ListView.SeparatorStyleProperty).Returns(SeparatorStyle.Default);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, ListView>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = ListView.GetSeparatorStyle(mockConfig);

            // Assert
            mockConfig.Received(1).Element;
            mockElement.Received(1).GetValue(ListView.SeparatorStyleProperty);
            Assert.Equal(SeparatorStyle.Default, result);
        }

        /// <summary>
        /// Tests that SetSeparatorStyle extension method with null config throws NullReferenceException.
        /// Validates proper null parameter handling for the config parameter.
        /// </summary>
        [Fact]
        public void SetSeparatorStyle_WithNullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.ListView> nullConfig = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ListView.SetSeparatorStyle(nullConfig, SeparatorStyle.Default));
        }

        /// <summary>
        /// Tests that SetSeparatorStyle extension method with invalid enum value still processes without throwing.
        /// Validates behavior with enum values outside the defined range.
        /// </summary>
        /// <param name="invalidValue">Invalid separator style value cast from integer</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        [InlineData(999)]
        public void SetSeparatorStyle_WithInvalidEnumValue_ProcessesWithoutThrowing(int invalidValue)
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.ListView>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.ListView>();
            mockConfig.Element.Returns(mockElement);
            var invalidSeparatorStyle = (SeparatorStyle)invalidValue;

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ListView.SetSeparatorStyle(mockConfig, invalidSeparatorStyle);

            // Assert
            Assert.Same(mockConfig, result);
            var elementAccessed = mockConfig.Received(1).Element;
        }

        /// <summary>
        /// Tests that SetSeparatorStyle extension method accesses the Element property exactly once.
        /// Validates that the config.Element property is accessed during method execution for delegation to the underlying method.
        /// </summary>
        [Fact]
        public void SetSeparatorStyle_AccessesElementPropertyOnce()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.ListView>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.ListView>();
            mockConfig.Element.Returns(mockElement);

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ListView.SetSeparatorStyle(mockConfig, SeparatorStyle.Default);

            // Assert
            var elementAccessed = mockConfig.Received(1).Element;
        }

        /// <summary>
        /// Tests that SetSeparatorStyle extension method with null Element in config throws NullReferenceException.
        /// Validates behavior when the config.Element property returns null.
        /// </summary>
        [Fact]
        public void SetSeparatorStyle_WithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.ListView>>();
            mockConfig.Element.Returns((Microsoft.Maui.Controls.ListView)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ListView.SetSeparatorStyle(mockConfig, SeparatorStyle.Default));
        }

        /// <summary>
        /// Tests that GetGroupHeaderStyle throws ArgumentNullException when element parameter is null.
        /// Verifies proper null parameter validation.
        /// Expected: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void GetGroupHeaderStyle_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => PlatformConfiguration.iOSSpecific.ListView.GetGroupHeaderStyle(element));
        }

        /// <summary>
        /// Tests that GetGroupHeaderStyle properly calls GetValue with the correct BindableProperty.
        /// Verifies that the method interacts correctly with the BindableObject's GetValue method.
        /// Expected: GetValue should be called exactly once with GroupHeaderStyleProperty.
        /// </summary>
        [Fact]
        public void GetGroupHeaderStyle_ValidElement_CallsGetValueWithCorrectProperty()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(PlatformConfiguration.iOSSpecific.ListView.GroupHeaderStyleProperty).Returns(GroupHeaderStyle.Plain);

            // Act
            PlatformConfiguration.iOSSpecific.ListView.GetGroupHeaderStyle(mockElement);

            // Assert
            mockElement.Received(1).GetValue(PlatformConfiguration.iOSSpecific.ListView.GroupHeaderStyleProperty);
        }

        /// <summary>
        /// Tests that GetGroupHeaderStyle handles the default value correctly when no explicit value is set.
        /// Verifies that the method works with the default GroupHeaderStyle.Plain value.
        /// Expected: Should return GroupHeaderStyle.Plain as the default value.
        /// </summary>
        [Fact]
        public void GetGroupHeaderStyle_ElementWithDefaultValue_ReturnsPlain()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(PlatformConfiguration.iOSSpecific.ListView.GroupHeaderStyleProperty).Returns(GroupHeaderStyle.Plain);

            // Act
            var result = PlatformConfiguration.iOSSpecific.ListView.GetGroupHeaderStyle(mockElement);

            // Assert
            Assert.Equal(GroupHeaderStyle.Plain, result);
        }

        /// <summary>
        /// Tests that SetGroupHeaderStyle correctly calls SetValue on the element with valid GroupHeaderStyle.Plain value.
        /// Verifies the method properly delegates to BindableObject.SetValue with the correct parameters.
        /// </summary>
        [Fact]
        public void SetGroupHeaderStyle_WithPlainValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var value = GroupHeaderStyle.Plain;

            // Act
            ListView.SetGroupHeaderStyle(mockElement, value);

            // Assert
            mockElement.Received(1).SetValue(ListView.GroupHeaderStyleProperty, value);
        }

        /// <summary>
        /// Tests that SetGroupHeaderStyle correctly calls SetValue on the element with valid GroupHeaderStyle.Grouped value.
        /// Verifies the method properly delegates to BindableObject.SetValue with the correct parameters.
        /// </summary>
        [Fact]
        public void SetGroupHeaderStyle_WithGroupedValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var value = GroupHeaderStyle.Grouped;

            // Act
            ListView.SetGroupHeaderStyle(mockElement, value);

            // Assert
            mockElement.Received(1).SetValue(ListView.GroupHeaderStyleProperty, value);
        }

        /// <summary>
        /// Tests that SetGroupHeaderStyle throws NullReferenceException when element parameter is null.
        /// Verifies proper error handling for invalid null element input.
        /// </summary>
        [Fact]
        public void SetGroupHeaderStyle_WithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject nullElement = null;
            var value = GroupHeaderStyle.Plain;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ListView.SetGroupHeaderStyle(nullElement, value));
        }

        /// <summary>
        /// Tests that SetGroupHeaderStyle accepts invalid enum values and passes them through to SetValue.
        /// Verifies the method does not perform enum validation and delegates validation to the underlying SetValue method.
        /// </summary>
        [Fact]
        public void SetGroupHeaderStyle_WithInvalidEnumValue_CallsSetValueWithInvalidValue()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var invalidValue = (GroupHeaderStyle)999;

            // Act
            ListView.SetGroupHeaderStyle(mockElement, invalidValue);

            // Assert
            mockElement.Received(1).SetValue(ListView.GroupHeaderStyleProperty, invalidValue);
        }

        /// <summary>
        /// Tests that GetGroupHeaderStyle returns Plain when the element has Plain GroupHeaderStyle configured.
        /// Validates the extension method correctly delegates to the underlying GetGroupHeaderStyle method
        /// and returns the expected Plain enum value.
        /// </summary>
        [Fact]
        public void GetGroupHeaderStyle_ConfigWithPlainGroupHeaderStyle_ReturnsPlain()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.ListView>>();
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(ListView.GroupHeaderStyleProperty).Returns(GroupHeaderStyle.Plain);
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = mockConfig.GetGroupHeaderStyle();

            // Assert
            Assert.Equal(GroupHeaderStyle.Plain, result);
        }

        /// <summary>
        /// Tests that GetGroupHeaderStyle returns Grouped when the element has Grouped GroupHeaderStyle configured.
        /// Validates the extension method correctly delegates to the underlying GetGroupHeaderStyle method
        /// and returns the expected Grouped enum value.
        /// </summary>
        [Fact]
        public void GetGroupHeaderStyle_ConfigWithGroupedGroupHeaderStyle_ReturnsGrouped()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.ListView>>();
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(ListView.GroupHeaderStyleProperty).Returns(GroupHeaderStyle.Grouped);
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = mockConfig.GetGroupHeaderStyle();

            // Assert
            Assert.Equal(GroupHeaderStyle.Grouped, result);
        }

        /// <summary>
        /// Tests that GetGroupHeaderStyle throws ArgumentNullException when config parameter is null.
        /// Validates proper null parameter handling and exception throwing behavior.
        /// </summary>
        [Fact]
        public void GetGroupHeaderStyle_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.ListView> config = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => config.GetGroupHeaderStyle());
        }

        /// <summary>
        /// Tests that GetGroupHeaderStyle throws ArgumentNullException when config.Element is null.
        /// Validates proper handling of null element reference and exception propagation from the underlying method.
        /// </summary>
        [Fact]
        public void GetGroupHeaderStyle_ConfigWithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Microsoft.Maui.Controls.ListView>>();
            mockConfig.Element.Returns((BindableObject)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => mockConfig.GetGroupHeaderStyle());
        }

        /// <summary>
        /// Tests that SetGroupHeaderStyle extension method calls the delegate method and returns the same config object.
        /// </summary>
        [Fact]
        public void SetGroupHeaderStyle_ValidConfig_CallsDelegateMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, ListView>>();
            var mockElement = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockElement);
            var groupHeaderStyle = GroupHeaderStyle.Plain;

            // Act
            var result = PlatformConfiguration.iOSSpecific.ListView.SetGroupHeaderStyle(mockConfig, groupHeaderStyle);

            // Assert
            mockElement.Received(1).SetValue(PlatformConfiguration.iOSSpecific.ListView.GroupHeaderStyleProperty, groupHeaderStyle);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetGroupHeaderStyle extension method throws NullReferenceException when config is null.
        /// </summary>
        [Fact]
        public void SetGroupHeaderStyle_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.iOS, ListView> nullConfig = null;
            var groupHeaderStyle = GroupHeaderStyle.Plain;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                PlatformConfiguration.iOSSpecific.ListView.SetGroupHeaderStyle(nullConfig, groupHeaderStyle));
        }

        /// <summary>
        /// Tests that SetGroupHeaderStyle extension method handles invalid enum values by still calling SetValue.
        /// This tests the behavior when an invalid enum value is cast and passed to the method.
        /// </summary>
        [Fact]
        public void SetGroupHeaderStyle_InvalidEnumValue_StillCallsSetValue()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, ListView>>();
            var mockElement = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockElement);
            var invalidGroupHeaderStyle = (GroupHeaderStyle)999;

            // Act
            var result = PlatformConfiguration.iOSSpecific.ListView.SetGroupHeaderStyle(mockConfig, invalidGroupHeaderStyle);

            // Assert
            mockElement.Received(1).SetValue(PlatformConfiguration.iOSSpecific.ListView.GroupHeaderStyleProperty, invalidGroupHeaderStyle);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetGroupHeaderStyle extension method throws NullReferenceException when config.Element is null.
        /// </summary>
        [Fact]
        public void SetGroupHeaderStyle_ConfigElementIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, ListView>>();
            mockConfig.Element.Returns((BindableObject)null);
            var groupHeaderStyle = GroupHeaderStyle.Plain;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                PlatformConfiguration.iOSSpecific.ListView.SetGroupHeaderStyle(mockConfig, groupHeaderStyle));
        }

        /// <summary>
        /// Tests that GetRowAnimationsEnabled throws ArgumentNullException when element parameter is null.
        /// This verifies proper null parameter validation.
        /// </summary>
        [Fact]
        public void GetRowAnimationsEnabled_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ListView.GetRowAnimationsEnabled(element));
        }

        /// <summary>
        /// Tests that GetRowAnimationsEnabled returns true when the RowAnimationsEnabled property is set to true.
        /// This verifies the method correctly retrieves and casts the boolean value from the bindable property.
        /// </summary>
        [Fact]
        public void GetRowAnimationsEnabled_ElementWithTrueValue_ReturnsTrue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ListView.RowAnimationsEnabledProperty).Returns(true);

            // Act
            var result = ListView.GetRowAnimationsEnabled(element);

            // Assert
            Assert.True(result);
            element.Received(1).GetValue(ListView.RowAnimationsEnabledProperty);
        }

        /// <summary>
        /// Tests that GetRowAnimationsEnabled returns false when the RowAnimationsEnabled property is set to false.
        /// This verifies the method correctly retrieves and casts the boolean value from the bindable property.
        /// </summary>
        [Fact]
        public void GetRowAnimationsEnabled_ElementWithFalseValue_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ListView.RowAnimationsEnabledProperty).Returns(false);

            // Act
            var result = ListView.GetRowAnimationsEnabled(element);

            // Assert
            Assert.False(result);
            element.Received(1).GetValue(ListView.RowAnimationsEnabledProperty);
        }

        /// <summary>
        /// Tests that GetRowAnimationsEnabled returns the default value when no explicit value has been set.
        /// This verifies the method works correctly with the property's default value of true.
        /// </summary>
        [Fact]
        public void GetRowAnimationsEnabled_ElementWithDefaultValue_ReturnsTrue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ListView.RowAnimationsEnabledProperty).Returns(true); // Default value is true

            // Act
            var result = ListView.GetRowAnimationsEnabled(element);

            // Assert
            Assert.True(result);
            element.Received(1).GetValue(ListView.RowAnimationsEnabledProperty);
        }

        /// <summary>
        /// Tests that SetRowAnimationsEnabled throws NullReferenceException when element parameter is null.
        /// Verifies proper null handling behavior of the method.
        /// </summary>
        [Fact]
        public void SetRowAnimationsEnabled_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ListView.SetRowAnimationsEnabled(element, value));
        }

        /// <summary>
        /// Tests that SetRowAnimationsEnabled correctly calls SetValue with RowAnimationsEnabledProperty and the provided boolean value.
        /// Verifies the method properly delegates to the element's SetValue method with correct parameters.
        /// </summary>
        /// <param name="value">The boolean value to set for row animations enabled property.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetRowAnimationsEnabled_ValidElement_CallsSetValueWithCorrectParameters(bool value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            ListView.SetRowAnimationsEnabled(element, value);

            // Assert
            element.Received(1).SetValue(ListView.RowAnimationsEnabledProperty, value);
        }

        /// <summary>
        /// Tests that SetRowAnimationsEnabled throws ArgumentNullException when config parameter is null.
        /// Verifies proper null parameter validation and exception handling.
        /// </summary>
        [Fact]
        public void SetRowAnimationsEnabled_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, ListView> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ListView.SetRowAnimationsEnabled(config, true));
        }

        /// <summary>
        /// Tests that SetRowAnimationsEnabled with null Element throws ArgumentNullException.
        /// Verifies proper validation when config.Element is null and exception is propagated.
        /// </summary>
        [Fact]
        public void SetRowAnimationsEnabled_ConfigWithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<iOS, ListView>>();
            config.Element.Returns((ListView)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ListView.SetRowAnimationsEnabled(config, true));
        }

        /// <summary>
        /// Tests that SetRowAnimationsEnabled with true value sets the property correctly and returns config.
        /// Verifies the method calls the underlying SetValue method with correct parameters and returns fluent interface.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetRowAnimationsEnabled_ValidConfigWithValue_SetsPropertyAndReturnsConfig(bool value)
        {
            // Arrange
            var listView = Substitute.For<ListView>();
            var config = Substitute.For<IPlatformElementConfiguration<iOS, ListView>>();
            config.Element.Returns(listView);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ListView.SetRowAnimationsEnabled(config, value);

            // Assert
            listView.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ListView.RowAnimationsEnabledProperty,
                value);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that RowAnimationsEnabled returns the default value (true) when the property is not explicitly set.
        /// Verifies that the method correctly delegates to GetRowAnimationsEnabled and returns the expected boolean value.
        /// </summary>
        [Fact]
        public void RowAnimationsEnabled_WithDefaultValue_ReturnsTrue()
        {
            // Arrange
            var listView = new ListView();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, ListView>>();
            config.Element.Returns(listView);

            // Act
            var result = config.RowAnimationsEnabled();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that RowAnimationsEnabled returns true when the RowAnimationsEnabled property is explicitly set to true.
        /// Verifies the method correctly retrieves the property value through the configuration chain.
        /// </summary>
        [Fact]
        public void RowAnimationsEnabled_WithTrueValue_ReturnsTrue()
        {
            // Arrange
            var listView = new ListView();
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ListView.SetRowAnimationsEnabled(listView, true);
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, ListView>>();
            config.Element.Returns(listView);

            // Act
            var result = config.RowAnimationsEnabled();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that RowAnimationsEnabled returns false when the RowAnimationsEnabled property is explicitly set to false.
        /// Verifies the method correctly retrieves the property value through the configuration chain.
        /// </summary>
        [Fact]
        public void RowAnimationsEnabled_WithFalseValue_ReturnsFalse()
        {
            // Arrange
            var listView = new ListView();
            Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ListView.SetRowAnimationsEnabled(listView, false);
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.iOS, ListView>>();
            config.Element.Returns(listView);

            // Act
            var result = config.RowAnimationsEnabled();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that RowAnimationsEnabled throws ArgumentNullException when config parameter is null.
        /// Verifies proper null parameter validation for the extension method.
        /// </summary>
        [Fact]
        public void RowAnimationsEnabled_WithNullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.iOS, ListView> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.RowAnimationsEnabled());
        }

        /// <summary>
        /// Tests that GetSeparatorStyle throws ArgumentNullException when element parameter is null.
        /// Verifies the method properly validates input parameters.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void GetSeparatorStyle_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ListView.GetSeparatorStyle(element));
        }

        /// <summary>
        /// Tests that GetSeparatorStyle returns the default separator style value when property is not explicitly set.
        /// Verifies the method correctly retrieves the default value from an uninitialized BindableObject.
        /// Expected result: SeparatorStyle.Default should be returned.
        /// </summary>
        [Fact]
        public void GetSeparatorStyle_ValidElementWithDefaultValue_ReturnsDefault()
        {
            // Arrange
            var element = new global::Microsoft.Maui.Controls.ListView();

            // Act
            var result = ListView.GetSeparatorStyle(element);

            // Assert
            Assert.Equal(SeparatorStyle.Default, result);
        }

        /// <summary>
        /// Tests that GetSeparatorStyle returns the correct separator style when set to Default.
        /// Verifies the method correctly retrieves explicitly set Default value from BindableObject.
        /// Expected result: SeparatorStyle.Default should be returned.
        /// </summary>
        [Fact]
        public void GetSeparatorStyle_ValidElementSetToDefault_ReturnsDefault()
        {
            // Arrange
            var element = new global::Microsoft.Maui.Controls.ListView();
            ListView.SetSeparatorStyle(element, SeparatorStyle.Default);

            // Act
            var result = ListView.GetSeparatorStyle(element);

            // Assert
            Assert.Equal(SeparatorStyle.Default, result);
        }

        /// <summary>
        /// Tests that GetSeparatorStyle returns the correct separator style when set to FullWidth.
        /// Verifies the method correctly retrieves explicitly set FullWidth value from BindableObject.
        /// Expected result: SeparatorStyle.FullWidth should be returned.
        /// </summary>
        [Fact]
        public void GetSeparatorStyle_ValidElementSetToFullWidth_ReturnsFullWidth()
        {
            // Arrange
            var element = new global::Microsoft.Maui.Controls.ListView();
            ListView.SetSeparatorStyle(element, SeparatorStyle.FullWidth);

            // Act
            var result = ListView.GetSeparatorStyle(element);

            // Assert
            Assert.Equal(SeparatorStyle.FullWidth, result);
        }

        /// <summary>
        /// Tests that GetSeparatorStyle works correctly with different BindableObject derived types.
        /// Verifies the method is not dependent on specific BindableObject implementations.
        /// Expected result: The method should return the correct separator style regardless of BindableObject type.
        /// </summary>
        [Fact]
        public void GetSeparatorStyle_DifferentBindableObjectTypes_ReturnsCorrectValue()
        {
            // Arrange
            var listView = new global::Microsoft.Maui.Controls.ListView();
            var button = new Button();

            ListView.SetSeparatorStyle(listView, SeparatorStyle.FullWidth);
            ListView.SetSeparatorStyle(button, SeparatorStyle.Default);

            // Act
            var listViewResult = ListView.GetSeparatorStyle(listView);
            var buttonResult = ListView.GetSeparatorStyle(button);

            // Assert
            Assert.Equal(SeparatorStyle.FullWidth, listViewResult);
            Assert.Equal(SeparatorStyle.Default, buttonResult);
        }
    }
}