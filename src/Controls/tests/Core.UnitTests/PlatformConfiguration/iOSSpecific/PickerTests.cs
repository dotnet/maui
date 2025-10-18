#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.iOSSpecific
{
    /// <summary>
    /// Unit tests for the Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Picker class.
    /// </summary>
    public class PickerTests
    {
        /// <summary>
        /// Tests that SetUpdateMode throws ArgumentNullException when element parameter is null.
        /// Input: null element, any UpdateMode value.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void SetUpdateMode_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;
            var value = UpdateMode.Immediately;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Picker.SetUpdateMode(element, value));
        }

        /// <summary>
        /// Tests that SetUpdateMode handles invalid enum values by still calling SetValue.
        /// Input: valid BindableObject, invalid UpdateMode enum value.
        /// Expected: element.SetValue is called with the invalid enum value (behavior depends on underlying implementation).
        /// </summary>
        [Fact]
        public void SetUpdateMode_InvalidEnumValue_CallsSetValueWithInvalidValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var invalidUpdateMode = (UpdateMode)999;

            // Act
            Picker.SetUpdateMode(element, invalidUpdateMode);

            // Assert
            element.Received(1).SetValue(Picker.UpdateModeProperty, invalidUpdateMode);
        }

        /// <summary>
        /// Tests that SetUpdateMode handles the minimum enum value correctly.
        /// Input: valid BindableObject, minimum UpdateMode enum value.
        /// Expected: element.SetValue is called with the minimum enum value.
        /// </summary>
        [Fact]
        public void SetUpdateMode_MinimumEnumValue_CallsSetValueCorrectly()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var minUpdateMode = (UpdateMode)0; // Immediately

            // Act
            Picker.SetUpdateMode(element, minUpdateMode);

            // Assert
            element.Received(1).SetValue(Picker.UpdateModeProperty, minUpdateMode);
        }

        /// <summary>
        /// Tests that SetUpdateMode handles negative enum values.
        /// Input: valid BindableObject, negative UpdateMode enum value.
        /// Expected: element.SetValue is called with the negative enum value.
        /// </summary>
        [Fact]
        public void SetUpdateMode_NegativeEnumValue_CallsSetValueWithNegativeValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var negativeUpdateMode = (UpdateMode)(-1);

            // Act
            Picker.SetUpdateMode(element, negativeUpdateMode);

            // Assert
            element.Received(1).SetValue(Picker.UpdateModeProperty, negativeUpdateMode);
        }

        /// <summary>
        /// Tests that UpdateMode extension method returns the correct UpdateMode value when given a valid configuration with Immediately mode.
        /// Verifies the method correctly retrieves and returns the UpdateMode.Immediately value from the underlying element.
        /// </summary>
        [Fact]
        public void UpdateMode_ValidConfigurationWithImmediatelyMode_ReturnsImmediately()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Picker>>();
            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(iOSSpecific.Picker.UpdateModeProperty).Returns(UpdateMode.Immediately);

            // Act
            var result = mockConfig.UpdateMode();

            // Assert
            Assert.Equal(UpdateMode.Immediately, result);
        }

        /// <summary>
        /// Tests that UpdateMode extension method returns the correct UpdateMode value when given a valid configuration with WhenFinished mode.
        /// Verifies the method correctly retrieves and returns the UpdateMode.WhenFinished value from the underlying element.
        /// </summary>
        [Fact]
        public void UpdateMode_ValidConfigurationWithWhenFinishedMode_ReturnsWhenFinished()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Picker>>();
            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(iOSSpecific.Picker.UpdateModeProperty).Returns(UpdateMode.WhenFinished);

            // Act
            var result = mockConfig.UpdateMode();

            // Assert
            Assert.Equal(UpdateMode.WhenFinished, result);
        }

        /// <summary>
        /// Tests that UpdateMode extension method throws NullReferenceException when given a null configuration parameter.
        /// Verifies proper error handling for invalid null input.
        /// </summary>
        [Fact]
        public void UpdateMode_NullConfiguration_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, Picker> nullConfig = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => nullConfig.UpdateMode());
        }

        /// <summary>
        /// Tests that UpdateMode extension method throws NullReferenceException when configuration has null Element property.
        /// Verifies proper error handling when the underlying element is null.
        /// </summary>
        [Fact]
        public void UpdateMode_ConfigurationWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Picker>>();
            mockConfig.Element.Returns((BindableObject)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => mockConfig.UpdateMode());
        }

        /// <summary>
        /// Tests that UpdateMode extension method handles default enum value correctly.
        /// Verifies the method works with the default UpdateMode value (which should be Immediately as it's the first enum value).
        /// </summary>
        [Fact]
        public void UpdateMode_ValidConfigurationWithDefaultValue_ReturnsDefaultValue()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Picker>>();
            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(iOSSpecific.Picker.UpdateModeProperty).Returns(default(UpdateMode));

            // Act
            var result = mockConfig.UpdateMode();

            // Assert
            Assert.Equal(default(UpdateMode), result);
        }

        /// <summary>
        /// Tests that SetUpdateMode extension method calls the static SetUpdateMode method with Immediately value
        /// and returns the same configuration object.
        /// </summary>
        [Fact]
        public void SetUpdateMode_WithImmediatelyValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Picker>>();
            var mockPicker = Substitute.For<Picker>();
            mockConfig.Element.Returns(mockPicker);
            var updateMode = UpdateMode.Immediately;

            // Act
            var result = mockConfig.SetUpdateMode(updateMode);

            // Assert
            mockPicker.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Picker.UpdateModeProperty, updateMode);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetUpdateMode extension method calls the static SetUpdateMode method with WhenFinished value
        /// and returns the same configuration object.
        /// </summary>
        [Fact]
        public void SetUpdateMode_WithWhenFinishedValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Picker>>();
            var mockPicker = Substitute.For<Picker>();
            mockConfig.Element.Returns(mockPicker);
            var updateMode = UpdateMode.WhenFinished;

            // Act
            var result = mockConfig.SetUpdateMode(updateMode);

            // Assert
            mockPicker.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Picker.UpdateModeProperty, updateMode);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetUpdateMode extension method throws NullReferenceException when config parameter is null.
        /// </summary>
        [Fact]
        public void SetUpdateMode_WithNullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, Picker> nullConfig = null;
            var updateMode = UpdateMode.Immediately;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => nullConfig.SetUpdateMode(updateMode));
        }

        /// <summary>
        /// Tests that SetUpdateMode extension method works with invalid enum values (cast from integer)
        /// by passing them through to the static method.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SetUpdateMode_WithInvalidEnumValue_CallsStaticMethodAndReturnsConfig(int invalidValue)
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Picker>>();
            var mockPicker = Substitute.For<Picker>();
            mockConfig.Element.Returns(mockPicker);
            var invalidUpdateMode = (UpdateMode)invalidValue;

            // Act
            var result = mockConfig.SetUpdateMode(invalidUpdateMode);

            // Assert
            mockPicker.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Picker.UpdateModeProperty, invalidUpdateMode);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetUpdateMode extension method throws NullReferenceException when config.Element is null.
        /// </summary>
        [Fact]
        public void SetUpdateMode_WithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, Picker>>();
            mockConfig.Element.Returns((Picker)null);
            var updateMode = UpdateMode.Immediately;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => mockConfig.SetUpdateMode(updateMode));
        }

        /// <summary>
        /// Tests that GetUpdateMode throws NullReferenceException when element parameter is null.
        /// Input: null element
        /// Expected: NullReferenceException thrown
        /// </summary>
        [Fact]
        public void GetUpdateMode_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Picker.GetUpdateMode(element));
        }

        /// <summary>
        /// Tests that GetUpdateMode returns Immediately when the element's GetValue returns UpdateMode.Immediately.
        /// Input: BindableObject that returns UpdateMode.Immediately
        /// Expected: UpdateMode.Immediately returned
        /// </summary>
        [Fact]
        public void GetUpdateMode_ElementReturnsImmediately_ReturnsImmediately()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Picker.UpdateModeProperty).Returns(UpdateMode.Immediately);

            // Act
            var result = Picker.GetUpdateMode(element);

            // Assert
            Assert.Equal(UpdateMode.Immediately, result);
            element.Received(1).GetValue(Picker.UpdateModeProperty);
        }

        /// <summary>
        /// Tests that GetUpdateMode returns WhenFinished when the element's GetValue returns UpdateMode.WhenFinished.
        /// Input: BindableObject that returns UpdateMode.WhenFinished  
        /// Expected: UpdateMode.WhenFinished returned
        /// </summary>
        [Fact]
        public void GetUpdateMode_ElementReturnsWhenFinished_ReturnsWhenFinished()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Picker.UpdateModeProperty).Returns(UpdateMode.WhenFinished);

            // Act
            var result = Picker.GetUpdateMode(element);

            // Assert
            Assert.Equal(UpdateMode.WhenFinished, result);
            element.Received(1).GetValue(Picker.UpdateModeProperty);
        }

        /// <summary>
        /// Tests that GetUpdateMode correctly handles the default enum value (0) returned from GetValue.
        /// Input: BindableObject that returns default UpdateMode value (Immediately, which is 0)
        /// Expected: UpdateMode.Immediately returned
        /// </summary>
        [Fact]
        public void GetUpdateMode_ElementReturnsDefaultValue_ReturnsImmediately()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Picker.UpdateModeProperty).Returns(default(UpdateMode));

            // Act
            var result = Picker.GetUpdateMode(element);

            // Assert
            Assert.Equal(UpdateMode.Immediately, result);
            element.Received(1).GetValue(Picker.UpdateModeProperty);
        }

    }
}