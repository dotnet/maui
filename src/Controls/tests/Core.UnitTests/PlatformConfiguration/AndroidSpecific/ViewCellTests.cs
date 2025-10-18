#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the ViewCell platform-specific configuration methods.
    /// </summary>
    public class ViewCellTests
    {
        /// <summary>
        /// Tests that GetIsContextActionsLegacyModeEnabled throws ArgumentNullException when element parameter is null.
        /// </summary>
        [Fact]
        public void GetIsContextActionsLegacyModeEnabled_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ViewCell.GetIsContextActionsLegacyModeEnabled(element));
        }

        /// <summary>
        /// Tests that GetIsContextActionsLegacyModeEnabled returns true when the property value is set to true.
        /// </summary>
        [Fact]
        public void GetIsContextActionsLegacyModeEnabled_ElementWithTrueValue_ReturnsTrue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ViewCell.IsContextActionsLegacyModeEnabledProperty).Returns(true);

            // Act
            var result = ViewCell.GetIsContextActionsLegacyModeEnabled(element);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetIsContextActionsLegacyModeEnabled returns false when the property value is set to false.
        /// </summary>
        [Fact]
        public void GetIsContextActionsLegacyModeEnabled_ElementWithFalseValue_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ViewCell.IsContextActionsLegacyModeEnabledProperty).Returns(false);

            // Act
            var result = ViewCell.GetIsContextActionsLegacyModeEnabled(element);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetIsContextActionsLegacyModeEnabled returns the default value (false) when no explicit value is set.
        /// </summary>
        [Fact]
        public void GetIsContextActionsLegacyModeEnabled_ElementWithDefaultValue_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ViewCell.IsContextActionsLegacyModeEnabledProperty).Returns(false);

            // Act
            var result = ViewCell.GetIsContextActionsLegacyModeEnabled(element);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetIsContextActionsLegacyModeEnabled calls GetValue with the correct BindableProperty.
        /// </summary>
        [Fact]
        public void GetIsContextActionsLegacyModeEnabled_ValidElement_CallsGetValueWithCorrectProperty()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ViewCell.IsContextActionsLegacyModeEnabledProperty).Returns(true);

            // Act
            ViewCell.GetIsContextActionsLegacyModeEnabled(element);

            // Assert
            element.Received(1).GetValue(ViewCell.IsContextActionsLegacyModeEnabledProperty);
        }

        /// <summary>
        /// Tests that GetIsContextActionsLegacyModeEnabled extension method correctly delegates to the BindableObject overload
        /// when provided with a valid configuration and returns the expected boolean value.
        /// </summary>
        /// <param name="expectedValue">The boolean value that should be returned by the delegate method</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetIsContextActionsLegacyModeEnabled_ValidConfig_ReturnsDelegateResult(bool expectedValue)
        {
            // Arrange
            var element = Substitute.For<Cell>();
            element.GetValue(ViewCell.IsContextActionsLegacyModeEnabledProperty).Returns(expectedValue);

            var config = Substitute.For<IPlatformElementConfiguration<Android, Cell>>();
            config.Element.Returns(element);

            // Act
            var result = ViewCell.GetIsContextActionsLegacyModeEnabled(config);

            // Assert
            Assert.Equal(expectedValue, result);
            element.Received(1).GetValue(ViewCell.IsContextActionsLegacyModeEnabledProperty);
        }

        /// <summary>
        /// Tests that GetIsContextActionsLegacyModeEnabled extension method throws ArgumentNullException
        /// when provided with a null configuration parameter.
        /// </summary>
        [Fact]
        public void GetIsContextActionsLegacyModeEnabled_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Android, Cell> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ViewCell.GetIsContextActionsLegacyModeEnabled(config));
        }

        /// <summary>
        /// Tests that GetIsContextActionsLegacyModeEnabled extension method throws ArgumentNullException
        /// when the configuration has a null Element property.
        /// </summary>
        [Fact]
        public void GetIsContextActionsLegacyModeEnabled_ConfigWithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Android, Cell>>();
            config.Element.Returns((Cell)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ViewCell.GetIsContextActionsLegacyModeEnabled(config));
        }

        /// <summary>
        /// Tests that GetIsContextActionsLegacyModeEnabled extension method correctly accesses the Element property
        /// of the configuration and passes it to the delegate method.
        /// </summary>
        [Fact]
        public void GetIsContextActionsLegacyModeEnabled_ValidConfig_AccessesElementProperty()
        {
            // Arrange
            var element = Substitute.For<Cell>();
            element.GetValue(ViewCell.IsContextActionsLegacyModeEnabledProperty).Returns(true);

            var config = Substitute.For<IPlatformElementConfiguration<Android, Cell>>();
            config.Element.Returns(element);

            // Act
            ViewCell.GetIsContextActionsLegacyModeEnabled(config);

            // Assert
            var _ = config.Received(1).Element;
        }

        /// <summary>
        /// Tests that SetIsContextActionsLegacyModeEnabled extension method returns the same config object 
        /// and calls the underlying SetValue method when value is true.
        /// </summary>
        [Fact]
        public void SetIsContextActionsLegacyModeEnabled_ValidConfigWithTrueValue_ReturnsConfigAndSetsProperty()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Cell>>();
            var mockCell = Substitute.For<Cell>();
            mockConfig.Element.Returns(mockCell);
            bool value = true;

            // Act
            var result = ViewCell.SetIsContextActionsLegacyModeEnabled(mockConfig, value);

            // Assert
            Assert.Same(mockConfig, result);
            mockCell.Received(1).SetValue(ViewCell.IsContextActionsLegacyModeEnabledProperty, value);
        }

        /// <summary>
        /// Tests that SetIsContextActionsLegacyModeEnabled extension method returns the same config object 
        /// and calls the underlying SetValue method when value is false.
        /// </summary>
        [Fact]
        public void SetIsContextActionsLegacyModeEnabled_ValidConfigWithFalseValue_ReturnsConfigAndSetsProperty()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Cell>>();
            var mockCell = Substitute.For<Cell>();
            mockConfig.Element.Returns(mockCell);
            bool value = false;

            // Act
            var result = ViewCell.SetIsContextActionsLegacyModeEnabled(mockConfig, value);

            // Assert
            Assert.Same(mockConfig, result);
            mockCell.Received(1).SetValue(ViewCell.IsContextActionsLegacyModeEnabledProperty, value);
        }

        /// <summary>
        /// Tests that SetIsContextActionsLegacyModeEnabled extension method throws ArgumentNullException 
        /// when config parameter is null.
        /// </summary>
        [Fact]
        public void SetIsContextActionsLegacyModeEnabled_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Android, Cell> config = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ViewCell.SetIsContextActionsLegacyModeEnabled(config, value));
        }

        /// <summary>
        /// Tests that SetIsContextActionsLegacyModeEnabled extension method throws NullReferenceException 
        /// when config.Element is null.
        /// </summary>
        [Fact]
        public void SetIsContextActionsLegacyModeEnabled_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Android, Cell>>();
            mockConfig.Element.Returns((Cell)null);
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ViewCell.SetIsContextActionsLegacyModeEnabled(mockConfig, value));
        }

        /// <summary>
        /// Tests that SetIsContextActionsLegacyModeEnabled throws NullReferenceException when element parameter is null.
        /// This test verifies the method's behavior when called with a null BindableObject reference.
        /// Expected result: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void SetIsContextActionsLegacyModeEnabled_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ViewCell.SetIsContextActionsLegacyModeEnabled(element, value));
        }

        /// <summary>
        /// Tests that SetIsContextActionsLegacyModeEnabled correctly calls SetValue on the element with the proper parameters.
        /// This test verifies the method calls SetValue with the IsContextActionsLegacyModeEnabledProperty and the provided boolean value.
        /// Expected result: SetValue is called once with the correct BindableProperty and value.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetIsContextActionsLegacyModeEnabled_ValidElement_CallsSetValueWithCorrectParameters(bool value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            ViewCell.SetIsContextActionsLegacyModeEnabled(element, value);

            // Assert
            element.Received(1).SetValue(ViewCell.IsContextActionsLegacyModeEnabledProperty, value);
        }
    }
}