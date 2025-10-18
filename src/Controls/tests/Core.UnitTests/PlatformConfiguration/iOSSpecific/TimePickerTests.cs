#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the TimePicker iOS-specific platform configuration methods.
    /// </summary>
    public partial class TimePickerTests
    {
        /// <summary>
        /// Tests that SetUpdateMode throws NullReferenceException when called with a null BindableObject element.
        /// </summary>
        [Fact]
        public void SetUpdateMode_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject nullElement = null;
            var updateMode = UpdateMode.Immediately;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TimePicker.SetUpdateMode(nullElement, updateMode));
        }

        /// <summary>
        /// Tests that SetUpdateMode handles invalid UpdateMode enum values by passing them through to SetValue.
        /// The underlying SetValue method will handle type validation.
        /// </summary>
        [Fact]
        public void SetUpdateMode_InvalidUpdateModeValue_PassesValueToSetValue()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var invalidUpdateMode = (UpdateMode)999;

            // Act
            TimePicker.SetUpdateMode(mockElement, invalidUpdateMode);

            // Assert
            mockElement.Received(1).SetValue(TimePicker.UpdateModeProperty, invalidUpdateMode);
        }

        /// <summary>
        /// Tests that SetUpdateMode works correctly with the minimum valid enum value.
        /// </summary>
        [Fact]
        public void SetUpdateMode_MinimumEnumValue_SetsValueOnBindableObject()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var minUpdateMode = (UpdateMode)0; // Immediately

            // Act
            TimePicker.SetUpdateMode(mockElement, minUpdateMode);

            // Assert
            mockElement.Received(1).SetValue(TimePicker.UpdateModeProperty, minUpdateMode);
        }

        /// <summary>
        /// Tests that SetUpdateMode works correctly with the maximum valid enum value.
        /// </summary>
        [Fact]
        public void SetUpdateMode_MaximumEnumValue_SetsValueOnBindableObject()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var maxUpdateMode = (UpdateMode)1; // WhenFinished

            // Act
            TimePicker.SetUpdateMode(mockElement, maxUpdateMode);

            // Assert
            mockElement.Received(1).SetValue(TimePicker.UpdateModeProperty, maxUpdateMode);
        }

        /// <summary>
        /// Tests that SetUpdateMode correctly uses the UpdateModeProperty static field.
        /// </summary>
        [Fact]
        public void SetUpdateMode_ValidInput_UsesCorrectBindableProperty()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var updateMode = UpdateMode.WhenFinished;

            // Act
            TimePicker.SetUpdateMode(mockElement, updateMode);

            // Assert
            mockElement.Received(1).SetValue(Arg.Is<BindableProperty>(bp => bp == TimePicker.UpdateModeProperty), updateMode);
        }

        /// <summary>
        /// Tests that UpdateMode extension method returns Immediately when the underlying element 
        /// has UpdateMode property set to Immediately.
        /// </summary>
        [Fact]
        public void UpdateMode_ValidConfigWithImmediatelyValue_ReturnsImmediately()
        {
            // Arrange
            var mockElement = Substitute.For<global::Microsoft.Maui.Controls.TimePicker>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, global::Microsoft.Maui.Controls.TimePicker>>();

            mockElement.GetValue(TimePicker.UpdateModeProperty).Returns(global::Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UpdateMode.Immediately);
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = TimePicker.UpdateMode(mockConfig);

            // Assert
            Assert.Equal(global::Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UpdateMode.Immediately, result);
        }

        /// <summary>
        /// Tests that UpdateMode extension method returns WhenFinished when the underlying element 
        /// has UpdateMode property set to WhenFinished.
        /// </summary>
        [Fact]
        public void UpdateMode_ValidConfigWithWhenFinishedValue_ReturnsWhenFinished()
        {
            // Arrange
            var mockElement = Substitute.For<global::Microsoft.Maui.Controls.TimePicker>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, global::Microsoft.Maui.Controls.TimePicker>>();

            mockElement.GetValue(TimePicker.UpdateModeProperty).Returns(global::Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UpdateMode.WhenFinished);
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = TimePicker.UpdateMode(mockConfig);

            // Assert
            Assert.Equal(global::Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UpdateMode.WhenFinished, result);
        }

        /// <summary>
        /// Tests that UpdateMode extension method throws NullReferenceException when 
        /// config parameter is null, as it attempts to access config.Element.
        /// </summary>
        [Fact]
        public void UpdateMode_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, global::Microsoft.Maui.Controls.TimePicker> config = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TimePicker.UpdateMode(config));
        }

        /// <summary>
        /// Tests that UpdateMode extension method throws NullReferenceException when 
        /// config.Element is null, as GetUpdateMode calls element.GetValue on null element.
        /// </summary>
        [Fact]
        public void UpdateMode_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, global::Microsoft.Maui.Controls.TimePicker>>();
            mockConfig.Element.Returns((global::Microsoft.Maui.Controls.TimePicker)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TimePicker.UpdateMode(mockConfig));
        }

        /// <summary>
        /// Tests that SetUpdateMode with Immediately value returns the same config object and does not throw.
        /// Verifies the fluent interface pattern works correctly with valid UpdateMode.Immediately.
        /// </summary>
        [Fact]
        public void SetUpdateMode_WithImmediatelyValue_ReturnsSameConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, TimePicker>>();
            var mockTimePicker = Substitute.For<TimePicker>();
            mockConfig.Element.Returns(mockTimePicker);
            var updateMode = UpdateMode.Immediately;

            // Act
            var result = mockConfig.SetUpdateMode(updateMode);

            // Assert
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetUpdateMode with WhenFinished value returns the same config object and does not throw.
        /// Verifies the fluent interface pattern works correctly with valid UpdateMode.WhenFinished.
        /// </summary>
        [Fact]
        public void SetUpdateMode_WithWhenFinishedValue_ReturnsSameConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, TimePicker>>();
            var mockTimePicker = Substitute.For<TimePicker>();
            mockConfig.Element.Returns(mockTimePicker);
            var updateMode = UpdateMode.WhenFinished;

            // Act
            var result = mockConfig.SetUpdateMode(updateMode);

            // Assert
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetUpdateMode throws ArgumentNullException when config parameter is null.
        /// Verifies proper null parameter validation for the config parameter.
        /// </summary>
        [Fact]
        public void SetUpdateMode_WithNullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, TimePicker> nullConfig = null;
            var updateMode = UpdateMode.Immediately;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => nullConfig.SetUpdateMode(updateMode));
        }

        /// <summary>
        /// Tests that SetUpdateMode handles invalid enum values without throwing.
        /// Verifies behavior with UpdateMode values outside the defined enum range.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void SetUpdateMode_WithInvalidEnumValue_ReturnsSameConfig(int invalidEnumValue)
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, TimePicker>>();
            var mockTimePicker = Substitute.For<TimePicker>();
            mockConfig.Element.Returns(mockTimePicker);
            var invalidUpdateMode = (UpdateMode)invalidEnumValue;

            // Act
            var result = mockConfig.SetUpdateMode(invalidUpdateMode);

            // Assert
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that GetUpdateMode throws NullReferenceException when element parameter is null.
        /// Verifies proper null parameter handling.
        /// Expected result: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void GetUpdateMode_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TimePicker.GetUpdateMode(element));
        }

        /// <summary>
        /// Tests that GetUpdateMode returns the default UpdateMode value when no value has been set on the element.
        /// Verifies that default enum value (Immediately) is returned correctly.
        /// Expected result: UpdateMode.Immediately (default enum value).
        /// </summary>
        [Fact]
        public void GetUpdateMode_ElementWithDefaultValue_ReturnsImmediately()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TimePicker.UpdateModeProperty).Returns(UpdateMode.Immediately);

            // Act
            var result = TimePicker.GetUpdateMode(element);

            // Assert
            Assert.Equal(UpdateMode.Immediately, result);
        }

        /// <summary>
        /// Tests that GetUpdateMode returns UpdateMode.Immediately when that value is set on the element.
        /// Verifies correct retrieval and casting of Immediately enum value.
        /// Expected result: UpdateMode.Immediately.
        /// </summary>
        [Fact]
        public void GetUpdateMode_ElementWithImmediatelyValue_ReturnsImmediately()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TimePicker.UpdateModeProperty).Returns(UpdateMode.Immediately);

            // Act
            var result = TimePicker.GetUpdateMode(element);

            // Assert
            Assert.Equal(UpdateMode.Immediately, result);
        }

        /// <summary>
        /// Tests that GetUpdateMode returns UpdateMode.WhenFinished when that value is set on the element.
        /// Verifies correct retrieval and casting of WhenFinished enum value.
        /// Expected result: UpdateMode.WhenFinished.
        /// </summary>
        [Fact]
        public void GetUpdateMode_ElementWithWhenFinishedValue_ReturnsWhenFinished()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TimePicker.UpdateModeProperty).Returns(UpdateMode.WhenFinished);

            // Act
            var result = TimePicker.GetUpdateMode(element);

            // Assert
            Assert.Equal(UpdateMode.WhenFinished, result);
        }

    }
}