#nullable disable

namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UnitTests
{
    public class DatePickerTests
    {
        /// <summary>
        /// Tests that SetUpdateMode throws NullReferenceException when element parameter is null.
        /// Input: null element, valid UpdateMode value.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void SetUpdateMode_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            var value = UpdateMode.Immediately;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => DatePicker.SetUpdateMode(element, value));
        }

        /// <summary>
        /// Tests that SetUpdateMode calls SetValue on the element with correct parameters when UpdateMode is Immediately.
        /// Input: valid BindableObject, UpdateMode.Immediately.
        /// Expected: element.SetValue is called with UpdateModeProperty and UpdateMode.Immediately.
        /// </summary>
        [Fact]
        public void SetUpdateMode_ValidElementAndImmediately_CallsSetValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = UpdateMode.Immediately;

            // Act
            DatePicker.SetUpdateMode(element, value);

            // Assert
            element.Received(1).SetValue(DatePicker.UpdateModeProperty, value);
        }

        /// <summary>
        /// Tests that SetUpdateMode calls SetValue on the element with correct parameters when UpdateMode is WhenFinished.
        /// Input: valid BindableObject, UpdateMode.WhenFinished.
        /// Expected: element.SetValue is called with UpdateModeProperty and UpdateMode.WhenFinished.
        /// </summary>
        [Fact]
        public void SetUpdateMode_ValidElementAndWhenFinished_CallsSetValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = UpdateMode.WhenFinished;

            // Act
            DatePicker.SetUpdateMode(element, value);

            // Assert
            element.Received(1).SetValue(DatePicker.UpdateModeProperty, value);
        }

        /// <summary>
        /// Tests that SetUpdateMode calls SetValue on the element even with invalid enum values.
        /// Input: valid BindableObject, invalid UpdateMode value (cast from int).
        /// Expected: element.SetValue is called with UpdateModeProperty and the invalid enum value.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        [InlineData(999)]
        public void SetUpdateMode_ValidElementAndInvalidEnumValue_CallsSetValue(int invalidEnumValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = (UpdateMode)invalidEnumValue;

            // Act
            DatePicker.SetUpdateMode(element, value);

            // Assert
            element.Received(1).SetValue(DatePicker.UpdateModeProperty, value);
        }

        /// <summary>
        /// Tests that SetUpdateMode works with all valid UpdateMode enum values using parameterized test.
        /// Input: valid BindableObject, all valid UpdateMode values.
        /// Expected: element.SetValue is called with UpdateModeProperty and the provided enum value.
        /// </summary>
        [Theory]
        [InlineData(UpdateMode.Immediately)]
        [InlineData(UpdateMode.WhenFinished)]
        public void SetUpdateMode_ValidElementAndAllValidEnumValues_CallsSetValue(UpdateMode value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            DatePicker.SetUpdateMode(element, value);

            // Assert
            element.Received(1).SetValue(DatePicker.UpdateModeProperty, value);
        }

        /// <summary>
        /// Tests that UpdateMode extension method returns the correct UpdateMode value when called with a valid configuration.
        /// This test verifies the method correctly retrieves the UpdateMode from the Element's UpdateModeProperty.
        /// </summary>
        /// <param name="expectedUpdateMode">The UpdateMode value that should be returned</param>
        [Theory]
        [InlineData(UpdateMode.Immediately)]
        [InlineData(UpdateMode.WhenFinished)]
        public void UpdateMode_ValidConfig_ReturnsExpectedUpdateMode(UpdateMode expectedUpdateMode)
        {
            // Arrange
            var mockDatePicker = Substitute.For<DatePicker>();
            mockDatePicker.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.DatePicker.UpdateModeProperty)
                          .Returns(expectedUpdateMode);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, DatePicker>>();
            mockConfig.Element.Returns(mockDatePicker);

            // Act
            var result = mockConfig.UpdateMode();

            // Assert
            Assert.Equal(expectedUpdateMode, result);
        }

        /// <summary>
        /// Tests that UpdateMode extension method throws NullReferenceException when called with null configuration.
        /// This test verifies proper error handling for invalid input parameters.
        /// </summary>
        [Fact]
        public void UpdateMode_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, DatePicker> nullConfig = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => nullConfig.UpdateMode());
        }

        /// <summary>
        /// Tests that UpdateMode extension method works correctly when the Element property returns a valid DatePicker.
        /// This test verifies the method correctly accesses the Element property and calls GetUpdateMode.
        /// </summary>
        [Fact]
        public void UpdateMode_ValidConfigWithElement_CallsGetUpdateModeOnElement()
        {
            // Arrange
            var mockDatePicker = Substitute.For<DatePicker>();
            mockDatePicker.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.DatePicker.UpdateModeProperty)
                          .Returns(UpdateMode.WhenFinished);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<iOS, DatePicker>>();
            mockConfig.Element.Returns(mockDatePicker);

            // Act
            var result = mockConfig.UpdateMode();

            // Assert
            mockDatePicker.Received(1).GetValue(Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.DatePicker.UpdateModeProperty);
            Assert.Equal(UpdateMode.WhenFinished, result);
        }

        /// <summary>
        /// Tests that SetUpdateMode calls the static SetUpdateMode method with the correct parameters 
        /// and returns the same config object when given Immediately update mode.
        /// </summary>
        [Fact]
        public void SetUpdateMode_WithImmediatelyValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<iOS, DatePicker>>();
            var element = Substitute.For<DatePicker>();
            config.Element.Returns(element);
            var updateMode = UpdateMode.Immediately;

            // Act
            var result = DatePicker.SetUpdateMode(config, updateMode);

            // Assert
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetUpdateMode calls the static SetUpdateMode method with the correct parameters 
        /// and returns the same config object when given WhenFinished update mode.
        /// </summary>
        [Fact]
        public void SetUpdateMode_WithWhenFinishedValue_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<iOS, DatePicker>>();
            var element = Substitute.For<DatePicker>();
            config.Element.Returns(element);
            var updateMode = UpdateMode.WhenFinished;

            // Act
            var result = DatePicker.SetUpdateMode(config, updateMode);

            // Assert
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetUpdateMode throws ArgumentNullException when config parameter is null.
        /// </summary>
        [Fact]
        public void SetUpdateMode_WithNullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<iOS, DatePicker> config = null;
            var updateMode = UpdateMode.Immediately;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => DatePicker.SetUpdateMode(config, updateMode));
        }

        /// <summary>
        /// Tests that SetUpdateMode handles invalid enum values by casting and still returns the config object.
        /// This tests the boundary case of enum values outside the defined range.
        /// </summary>
        [Fact]
        public void SetUpdateMode_WithInvalidEnumValue_ReturnsConfig()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<iOS, DatePicker>>();
            var element = Substitute.For<DatePicker>();
            config.Element.Returns(element);
            var invalidUpdateMode = (UpdateMode)999;

            // Act
            var result = DatePicker.SetUpdateMode(config, invalidUpdateMode);

            // Assert
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetUpdateMode accesses the Element property of the config parameter
        /// to pass it to the static SetUpdateMode method.
        /// </summary>
        [Fact]
        public void SetUpdateMode_AccessesElementProperty_CallsStaticMethod()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<iOS, DatePicker>>();
            var element = Substitute.For<DatePicker>();
            config.Element.Returns(element);
            var updateMode = UpdateMode.WhenFinished;

            // Act
            DatePicker.SetUpdateMode(config, updateMode);

            // Assert
            var _ = config.Received(1).Element;
        }

        /// <summary>
        /// Tests that GetUpdateMode returns the correct UpdateMode value when called with a valid BindableObject
        /// that has the UpdateMode property set to Immediately.
        /// </summary>
        [Fact]
        public void GetUpdateMode_ValidElementWithImmediatelyValue_ReturnsImmediately()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(DatePicker.UpdateModeProperty).Returns(UpdateMode.Immediately);

            // Act
            var result = DatePicker.GetUpdateMode(element);

            // Assert
            Assert.Equal(UpdateMode.Immediately, result);
        }

        /// <summary>
        /// Tests that GetUpdateMode returns the correct UpdateMode value when called with a valid BindableObject
        /// that has the UpdateMode property set to WhenFinished.
        /// </summary>
        [Fact]
        public void GetUpdateMode_ValidElementWithWhenFinishedValue_ReturnsWhenFinished()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(DatePicker.UpdateModeProperty).Returns(UpdateMode.WhenFinished);

            // Act
            var result = DatePicker.GetUpdateMode(element);

            // Assert
            Assert.Equal(UpdateMode.WhenFinished, result);
        }

        /// <summary>
        /// Tests that GetUpdateMode correctly casts and returns UpdateMode values for different enum values,
        /// including testing the default enum value (0) and explicit enum values.
        /// </summary>
        /// <param name="enumValue">The UpdateMode enum value to test.</param>
        /// <param name="expectedResult">The expected UpdateMode result.</param>
        [Theory]
        [InlineData(0, UpdateMode.Immediately)]
        [InlineData(1, UpdateMode.WhenFinished)]
        public void GetUpdateMode_ValidElementWithEnumValues_ReturnsCorrectValue(int enumValue, UpdateMode expectedResult)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(DatePicker.UpdateModeProperty).Returns((UpdateMode)enumValue);

            // Act
            var result = DatePicker.GetUpdateMode(element);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that GetUpdateMode throws NullReferenceException when called with a null element parameter,
        /// as the method attempts to call GetValue on the null reference.
        /// </summary>
        [Fact]
        public void GetUpdateMode_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => DatePicker.GetUpdateMode(element));
        }
    }
}