#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Tests for ImageButton Android-specific platform configuration shadow color functionality.
    /// </summary>
    public partial class ImageButtonTests
    {
        /// <summary>
        /// Tests that GetShadowColor returns the expected color value when a valid element has a shadow color set.
        /// Verifies that the method correctly retrieves the shadow color from the bindable property.
        /// </summary>
        [Fact]
        public void GetShadowColor_ValidElementWithColorValue_ReturnsExpectedColor()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var expectedColor = Color.FromRgb(255, 0, 0); // Red color
            mockElement.GetValue(ImageButton.ShadowColorProperty).Returns(expectedColor);

            // Act
            var result = ImageButton.GetShadowColor(mockElement);

            // Assert
            Assert.Equal(expectedColor, result);
            mockElement.Received(1).GetValue(ImageButton.ShadowColorProperty);
        }

        /// <summary>
        /// Tests that GetShadowColor returns the default value when a valid element has no shadow color set.
        /// Verifies that the method correctly handles the default null value from the bindable property.
        /// </summary>
        [Fact]
        public void GetShadowColor_ValidElementWithDefaultValue_ReturnsNull()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(ImageButton.ShadowColorProperty).Returns(null);

            // Act
            var result = ImageButton.GetShadowColor(mockElement);

            // Assert
            Assert.Null(result);
            mockElement.Received(1).GetValue(ImageButton.ShadowColorProperty);
        }

        /// <summary>
        /// Tests that GetShadowColor returns various predefined colors correctly.
        /// Verifies that the method works with different color values including transparent and special colors.
        /// </summary>
        [Theory]
        [InlineData("transparent")]
        [InlineData("red")]
        [InlineData("blue")]
        [InlineData("green")]
        [InlineData("yellow")]
        [InlineData("white")]
        [InlineData("black")]
        public void GetShadowColor_ValidElementWithVariousColors_ReturnsExpectedColors(string colorName)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var expectedColor = Color.Parse(colorName);
            mockElement.GetValue(ImageButton.ShadowColorProperty).Returns(expectedColor);

            // Act
            var result = ImageButton.GetShadowColor(mockElement);

            // Assert
            Assert.Equal(expectedColor, result);
            mockElement.Received(1).GetValue(ImageButton.ShadowColorProperty);
        }

        /// <summary>
        /// Tests that GetShadowColor returns colors with various alpha values correctly.
        /// Verifies that the method handles colors with different transparency levels.
        /// </summary>
        [Theory]
        [InlineData(0.0f)] // Fully transparent
        [InlineData(0.5f)] // Semi-transparent
        [InlineData(1.0f)] // Fully opaque
        public void GetShadowColor_ValidElementWithAlphaColors_ReturnsExpectedColors(float alpha)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var expectedColor = Color.FromRgba(255, 128, 64, alpha);
            mockElement.GetValue(ImageButton.ShadowColorProperty).Returns(expectedColor);

            // Act
            var result = ImageButton.GetShadowColor(mockElement);

            // Assert
            Assert.Equal(expectedColor, result);
            mockElement.Received(1).GetValue(ImageButton.ShadowColorProperty);
        }

        /// <summary>
        /// Tests that GetShadowColor throws an exception when passed a null element.
        /// Verifies that the method properly validates its input parameter.
        /// </summary>
        [Fact]
        public void GetShadowColor_NullElement_ThrowsException()
        {
            // Arrange
            BindableObject nullElement = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ImageButton.GetShadowColor(nullElement));
        }

        /// <summary>
        /// Tests that GetShadowColor handles extreme color values correctly.
        /// Verifies that the method works with colors at RGB component boundaries.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0)] // Black - minimum values
        [InlineData(255, 255, 255)] // White - maximum values
        [InlineData(127, 127, 127)] // Gray - middle values
        [InlineData(255, 0, 0)] // Pure red
        [InlineData(0, 255, 0)] // Pure green
        [InlineData(0, 0, 255)] // Pure blue
        public void GetShadowColor_ValidElementWithExtremeBoundaryColors_ReturnsExpectedColors(int red, int green, int blue)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var expectedColor = Color.FromRgb(red, green, blue);
            mockElement.GetValue(ImageButton.ShadowColorProperty).Returns(expectedColor);

            // Act
            var result = ImageButton.GetShadowColor(mockElement);

            // Assert
            Assert.Equal(expectedColor, result);
            mockElement.Received(1).GetValue(ImageButton.ShadowColorProperty);
        }

        /// <summary>
        /// Tests that SetShadowColor throws NullReferenceException when element parameter is null.
        /// Verifies that the method properly handles null element input by throwing the expected exception.
        /// </summary>
        [Fact]
        public void SetShadowColor_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            var color = Colors.Red;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ImageButton.SetShadowColor(element, color));
        }

        /// <summary>
        /// Tests that SetShadowColor correctly calls SetValue with the ShadowColorProperty and provided color value.
        /// Verifies that the method properly delegates to the BindableObject.SetValue method with correct parameters.
        /// </summary>
        [Fact]
        public void SetShadowColor_ValidElement_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var color = Colors.Blue;

            // Act
            ImageButton.SetShadowColor(element, color);

            // Assert
            element.Received(1).SetValue(ImageButton.ShadowColorProperty, color);
        }

        /// <summary>
        /// Tests that SetShadowColor handles various color values correctly including transparent, primary colors, and white.
        /// Verifies that different color inputs are properly passed through to the SetValue method.
        /// </summary>
        [Theory]
        [InlineData(0f, 0f, 0f, 0f)]      // Transparent
        [InlineData(1f, 0f, 0f, 1f)]      // Red
        [InlineData(0f, 1f, 0f, 1f)]      // Green  
        [InlineData(0f, 0f, 1f, 1f)]      // Blue
        [InlineData(1f, 1f, 1f, 1f)]      // White
        public void SetShadowColor_ValidElementWithVariousColors_CallsSetValueWithCorrectParameters(float r, float g, float b, float a)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var color = new Color(r, g, b, a);

            // Act
            ImageButton.SetShadowColor(element, color);

            // Assert
            element.Received(1).SetValue(ImageButton.ShadowColorProperty, color);
        }

        /// <summary>
        /// Tests that SetShadowColor handles default Color value correctly.
        /// Verifies that the method works with Color's default struct value.
        /// </summary>
        [Fact]
        public void SetShadowColor_ValidElementWithDefaultColor_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var color = default(Color);

            // Act
            ImageButton.SetShadowColor(element, color);

            // Assert
            element.Received(1).SetValue(ImageButton.ShadowColorProperty, color);
        }

        /// <summary>
        /// Tests that GetShadowColor throws NullReferenceException when config parameter is null.
        /// This verifies the method correctly handles null input by accessing config.Element.
        /// </summary>
        [Fact]
        public void GetShadowColor_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton> config = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                AndroidSpecific.ImageButton.GetShadowColor(config));
        }

        /// <summary>
        /// Tests that GetShadowColor throws NullReferenceException when config.Element is null.
        /// This verifies the method correctly handles the case where the configuration has a null element.
        /// </summary>
        [Fact]
        public void GetShadowColor_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton>>();
            config.Element.Returns((Controls.ImageButton)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                AndroidSpecific.ImageButton.GetShadowColor(config));
        }

        /// <summary>
        /// Tests that GetShadowColor returns null when the shadow color property is not set.
        /// This verifies the method correctly returns the default value from the bindable property.
        /// </summary>
        [Fact]
        public void GetShadowColor_ValidConfigWithDefaultColor_ReturnsNull()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(AndroidSpecific.ImageButton.ShadowColorProperty).Returns((Color)null);

            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton>>();
            config.Element.Returns(element);

            // Act
            var result = AndroidSpecific.ImageButton.GetShadowColor(config);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetShadowColor returns the correct color when a shadow color is set.
        /// This verifies the method correctly delegates to the underlying GetShadowColor method
        /// and returns the expected color value.
        /// </summary>
        [Fact]
        public void GetShadowColor_ValidConfigWithSetColor_ReturnsExpectedColor()
        {
            // Arrange
            var expectedColor = Colors.Red;
            var element = Substitute.For<BindableObject>();
            element.GetValue(AndroidSpecific.ImageButton.ShadowColorProperty).Returns(expectedColor);

            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton>>();
            config.Element.Returns(element);

            // Act
            var result = AndroidSpecific.ImageButton.GetShadowColor(config);

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that GetShadowColor correctly calls GetValue on the element with the proper property.
        /// This verifies the method correctly accesses the shadow color property through the element.
        /// </summary>
        [Fact]
        public void GetShadowColor_ValidConfig_CallsGetValueWithCorrectProperty()
        {
            // Arrange
            var expectedColor = Colors.Blue;
            var element = Substitute.For<BindableObject>();
            element.GetValue(AndroidSpecific.ImageButton.ShadowColorProperty).Returns(expectedColor);

            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton>>();
            config.Element.Returns(element);

            // Act
            var result = AndroidSpecific.ImageButton.GetShadowColor(config);

            // Assert
            element.Received(1).GetValue(AndroidSpecific.ImageButton.ShadowColorProperty);
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that SetShadowColor extension method calls the static SetShadowColor method with config.Element and value parameters,
        /// and returns the same config object for method chaining when provided with valid parameters.
        /// </summary>
        [Fact]
        public void SetShadowColor_ValidConfigAndColor_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton>>();
            var mockImageButton = Substitute.For<Controls.ImageButton>();
            mockConfig.Element.Returns(mockImageButton);
            var color = Colors.Red;

            // Act
            var result = ImageButton.SetShadowColor(mockConfig, color);

            // Assert
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetShadowColor extension method throws ArgumentNullException when config parameter is null.
        /// </summary>
        [Fact]
        public void SetShadowColor_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton> config = null;
            var color = Colors.Blue;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ImageButton.SetShadowColor(config, color));
        }

        /// <summary>
        /// Tests that SetShadowColor extension method works correctly with default Color value.
        /// </summary>
        [Fact]
        public void SetShadowColor_DefaultColor_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton>>();
            var mockImageButton = Substitute.For<Controls.ImageButton>();
            mockConfig.Element.Returns(mockImageButton);
            var color = default(Color);

            // Act
            var result = ImageButton.SetShadowColor(mockConfig, color);

            // Assert
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetShadowColor extension method works correctly with various Color values including named colors,
        /// transparent colors, and custom RGBA values.
        /// </summary>
        [Theory]
        [InlineData(1.0f, 0.0f, 0.0f, 1.0f)] // Red
        [InlineData(0.0f, 1.0f, 0.0f, 1.0f)] // Green
        [InlineData(0.0f, 0.0f, 1.0f, 1.0f)] // Blue
        [InlineData(0.0f, 0.0f, 0.0f, 0.0f)] // Transparent
        [InlineData(1.0f, 1.0f, 1.0f, 1.0f)] // White
        [InlineData(0.5f, 0.5f, 0.5f, 0.5f)] // Gray with transparency
        public void SetShadowColor_VariousColorValues_CallsStaticMethodAndReturnsConfig(float red, float green, float blue, float alpha)
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton>>();
            var mockImageButton = Substitute.For<Controls.ImageButton>();
            mockConfig.Element.Returns(mockImageButton);
            var color = new Color(red, green, blue, alpha);

            // Act
            var result = ImageButton.SetShadowColor(mockConfig, color);

            // Assert
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that GetShadowRadius throws NullReferenceException when element parameter is null.
        /// This verifies the method handles null input appropriately by throwing an exception.
        /// Expected result: NullReferenceException should be thrown.
        /// </summary>
        [Fact]
        public void GetShadowRadius_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ImageButton.GetShadowRadius(element));
        }

        /// <summary>
        /// Tests that GetShadowRadius returns the correct double value from the BindableObject.
        /// This verifies the method properly retrieves and casts the value from the ShadowRadiusProperty.
        /// Expected result: The method should return the exact double value stored in the property.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(10.0)]
        [InlineData(-5.5)]
        [InlineData(100.25)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        public void GetShadowRadius_ValidElement_ReturnsExpectedValue(double expectedValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ImageButton.ShadowRadiusProperty).Returns(expectedValue);

            // Act
            var result = ImageButton.GetShadowRadius(element);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetShadowRadius handles special double values correctly.
        /// This verifies the method properly handles edge cases like NaN and infinity values.
        /// Expected result: The method should return the special double values without modification.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void GetShadowRadius_SpecialDoubleValues_ReturnsExpectedValue(double expectedValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ImageButton.ShadowRadiusProperty).Returns(expectedValue);

            // Act
            var result = ImageButton.GetShadowRadius(element);

            // Assert
            if (double.IsNaN(expectedValue))
            {
                Assert.True(double.IsNaN(result));
            }
            else
            {
                Assert.Equal(expectedValue, result);
            }
        }

        /// <summary>
        /// Tests that GetShadowRadius calls GetValue with the correct ShadowRadiusProperty.
        /// This verifies the method uses the proper BindableProperty when retrieving the value.
        /// Expected result: GetValue should be called exactly once with ShadowRadiusProperty.
        /// </summary>
        [Fact]
        public void GetShadowRadius_ValidElement_CallsGetValueWithCorrectProperty()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ImageButton.ShadowRadiusProperty).Returns(15.0);

            // Act
            ImageButton.GetShadowRadius(element);

            // Assert
            element.Received(1).GetValue(ImageButton.ShadowRadiusProperty);
        }

        /// <summary>
        /// Tests that GetShadowOffset extension method returns the shadow offset from the underlying element
        /// when provided with a valid configuration object containing an element with default shadow offset.
        /// </summary>
        [Fact]
        public void GetShadowOffset_WithValidConfigAndDefaultValue_ReturnsDefaultShadowOffset()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton>>();

            mockElement.GetValue(ImageButton.ShadowOffsetProperty).Returns(Size.Zero);
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = ImageButton.GetShadowOffset(mockConfig);

            // Assert
            Assert.Equal(Size.Zero, result);
        }

        /// <summary>
        /// Tests that GetShadowOffset extension method returns the custom shadow offset from the underlying element
        /// when provided with a valid configuration object containing an element with a custom shadow offset value.
        /// </summary>
        [Fact]
        public void GetShadowOffset_WithValidConfigAndCustomValue_ReturnsCustomShadowOffset()
        {
            // Arrange
            var customShadowOffset = new Size(10, 20);
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton>>();

            mockElement.GetValue(ImageButton.ShadowOffsetProperty).Returns(customShadowOffset);
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = ImageButton.GetShadowOffset(mockConfig);

            // Assert
            Assert.Equal(customShadowOffset, result);
        }

        /// <summary>
        /// Tests that GetShadowOffset extension method throws NullReferenceException
        /// when provided with a null configuration parameter.
        /// </summary>
        [Fact]
        public void GetShadowOffset_WithNullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton> nullConfig = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ImageButton.GetShadowOffset(nullConfig));
        }

        /// <summary>
        /// Tests that GetShadowOffset extension method throws ArgumentNullException
        /// when provided with a configuration that has a null Element property.
        /// </summary>
        [Fact]
        public void GetShadowOffset_WithConfigHavingNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton>>();
            mockConfig.Element.Returns((BindableObject)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ImageButton.GetShadowOffset(mockConfig));
        }

        /// <summary>
        /// Tests that GetShadowOffset extension method properly delegates to the static GetShadowOffset method
        /// and verifies the Element property is accessed exactly once.
        /// </summary>
        [Fact]
        public void GetShadowOffset_WithValidConfig_AccessesElementPropertyOnce()
        {
            // Arrange
            var expectedShadowOffset = new Size(5, 15);
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton>>();

            mockElement.GetValue(ImageButton.ShadowOffsetProperty).Returns(expectedShadowOffset);
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = ImageButton.GetShadowOffset(mockConfig);

            // Assert
            Assert.Equal(expectedShadowOffset, result);
            var elementAccessed = mockConfig.Received(1).Element;
        }

        /// <summary>
        /// Tests that GetShadowOffset extension method handles extreme Size values correctly
        /// including very large positive and negative values.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(0, 0)]
        [InlineData(-100.5, 200.7)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        public void GetShadowOffset_WithExtremeSizeValues_ReturnsExpectedValues(double width, double height)
        {
            // Arrange
            var extremeShadowOffset = new Size(width, height);
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton>>();

            mockElement.GetValue(ImageButton.ShadowOffsetProperty).Returns(extremeShadowOffset);
            mockConfig.Element.Returns(extremeShadowOffset);

            // Act
            var result = ImageButton.GetShadowOffset(mockConfig);

            // Assert
            Assert.Equal(extremeShadowOffset, result);
        }

        /// <summary>
        /// Tests that GetShadowOffset extension method handles NaN Size values correctly.
        /// </summary>
        [Fact]
        public void GetShadowOffset_WithNaNSizeValues_ReturnsNaNSize()
        {
            // Arrange
            var nanShadowOffset = new Size(double.NaN, double.NaN);
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton>>();

            mockElement.GetValue(ImageButton.ShadowOffsetProperty).Returns(nanShadowOffset);
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = ImageButton.GetShadowOffset(mockConfig);

            // Assert
            Assert.True(double.IsNaN(result.Width));
            Assert.True(double.IsNaN(result.Height));
        }

        /// <summary>
        /// Tests that GetRippleColor throws NullReferenceException when config parameter is null.
        /// Verifies that the method correctly handles null input by throwing an appropriate exception.
        /// </summary>
        [Fact]
        public void GetRippleColor_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Android, ImageButton> config = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => AndroidSpecific.ImageButton.GetRippleColor(config));
        }

        /// <summary>
        /// Tests that GetRippleColor returns the correct color when provided with a valid config and element.
        /// Verifies that the method properly delegates to the static GetRippleColor method and returns the expected color value.
        /// </summary>
        [Fact]
        public void GetRippleColor_ValidConfigWithElement_ReturnsExpectedColor()
        {
            // Arrange
            var expectedColor = Colors.Red;
            var mockImageButton = Substitute.For<ImageButton>();
            mockImageButton.GetValue(AndroidSpecific.ImageButton.RippleColorProperty).Returns(expectedColor);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, ImageButton>>();
            mockConfig.Element.Returns(mockImageButton);

            // Act
            var result = AndroidSpecific.ImageButton.GetRippleColor(mockConfig);

            // Assert
            Assert.Equal(expectedColor, result);
        }

        /// <summary>
        /// Tests that GetRippleColor handles null element in config by passing null to the underlying static method.
        /// Verifies that the method correctly propagates null element which should result in an exception from the static method.
        /// </summary>
        [Fact]
        public void GetRippleColor_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, ImageButton>>();
            mockConfig.Element.Returns((ImageButton)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => AndroidSpecific.ImageButton.GetRippleColor(mockConfig));
        }

        /// <summary>
        /// Tests that GetRippleColor returns default color when element has default color value.
        /// Verifies that the method correctly handles the default color scenario.
        /// </summary>
        [Fact]
        public void GetRippleColor_ElementWithDefaultColor_ReturnsDefaultColor()
        {
            // Arrange
            var defaultColor = default(Color);
            var mockImageButton = Substitute.For<ImageButton>();
            mockImageButton.GetValue(AndroidSpecific.ImageButton.RippleColorProperty).Returns(defaultColor);

            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, ImageButton>>();
            mockConfig.Element.Returns(mockImageButton);

            // Act
            var result = AndroidSpecific.ImageButton.GetRippleColor(mockConfig);

            // Assert
            Assert.Equal(defaultColor, result);
        }

        /// <summary>
        /// Tests that SetShadowRadius throws ArgumentNullException when element is null.
        /// Verifies that the method properly validates the element parameter.
        /// </summary>
        [Fact]
        public void SetShadowRadius_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;
            double value = 5.0;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ImageButton.SetShadowRadius(element, value));
        }

        /// <summary>
        /// Tests that SetShadowRadius calls SetValue on the element with the correct property and value.
        /// Verifies that the method properly delegates to the BindableObject's SetValue method with valid parameters.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(5.0)]
        [InlineData(10.0)]
        [InlineData(100.0)]
        [InlineData(-1.0)]
        [InlineData(-10.0)]
        public void SetShadowRadius_ValidElement_CallsSetValueWithCorrectParameters(double value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            ImageButton.SetShadowRadius(element, value);

            // Assert
            element.Received(1).SetValue(ImageButton.ShadowRadiusProperty, value);
        }

        /// <summary>
        /// Tests that SetShadowRadius handles boundary double values correctly.
        /// Verifies that the method can handle extreme numeric values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.Epsilon)]
        public void SetShadowRadius_BoundaryValues_CallsSetValueWithCorrectParameters(double value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            ImageButton.SetShadowRadius(element, value);

            // Assert
            element.Received(1).SetValue(ImageButton.ShadowRadiusProperty, value);
        }

        /// <summary>
        /// Tests that SetShadowRadius handles special double values correctly.
        /// Verifies that the method can handle NaN, positive infinity, and negative infinity values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void SetShadowRadius_SpecialDoubleValues_CallsSetValueWithCorrectParameters(double value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            ImageButton.SetShadowRadius(element, value);

            // Assert
            element.Received(1).SetValue(ImageButton.ShadowRadiusProperty, value);
        }

        /// <summary>
        /// Tests that GetShadowRadius extension method throws ArgumentNullException when config parameter is null.
        /// </summary>
        [Fact]
        public void GetShadowRadius_ConfigIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ImageButton.GetShadowRadius(config));
        }

        /// <summary>
        /// Tests that GetShadowRadius extension method returns the expected shadow radius value when called with valid config.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(10.0)]
        [InlineData(25.5)]
        [InlineData(100.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        public void GetShadowRadius_ValidConfig_ReturnsExpectedValue(double expectedRadius)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton>>();

            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(ImageButton.ShadowRadiusProperty).Returns(expectedRadius);

            // Act
            var result = ImageButton.GetShadowRadius(mockConfig);

            // Assert
            Assert.Equal(expectedRadius, result);
        }

        /// <summary>
        /// Tests that GetShadowRadius extension method calls the underlying GetShadowRadius method with correct element.
        /// </summary>
        [Fact]
        public void GetShadowRadius_ValidConfig_CallsGetValueOnElement()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton>>();
            var expectedRadius = 15.0;

            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(ImageButton.ShadowRadiusProperty).Returns(expectedRadius);

            // Act
            var result = ImageButton.GetShadowRadius(mockConfig);

            // Assert
            mockElement.Received(1).GetValue(ImageButton.ShadowRadiusProperty);
            Assert.Equal(expectedRadius, result);
        }

        /// <summary>
        /// Tests that SetShadowRadius extension method correctly calls the underlying SetShadowRadius method
        /// with a valid positive radius value and returns the same configuration object for method chaining.
        /// </summary>
        [Fact]
        public void SetShadowRadius_ValidPositiveValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton>>();
            mockConfig.Element.Returns(mockElement);
            double shadowRadius = 15.5;

            // Act
            var result = AndroidSpecific.ImageButton.SetShadowRadius(mockConfig, shadowRadius);

            // Assert
            mockElement.Received(1).SetValue(AndroidSpecific.ImageButton.ShadowRadiusProperty, shadowRadius);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetShadowRadius extension method correctly handles zero radius value
        /// and returns the same configuration object for method chaining.
        /// </summary>
        [Fact]
        public void SetShadowRadius_ZeroValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton>>();
            mockConfig.Element.Returns(mockElement);
            double shadowRadius = 0.0;

            // Act
            var result = AndroidSpecific.ImageButton.SetShadowRadius(mockConfig, shadowRadius);

            // Assert
            mockElement.Received(1).SetValue(AndroidSpecific.ImageButton.ShadowRadiusProperty, shadowRadius);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetShadowRadius extension method correctly handles negative radius value
        /// and returns the same configuration object for method chaining.
        /// </summary>
        [Fact]
        public void SetShadowRadius_NegativeValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton>>();
            mockConfig.Element.Returns(mockElement);
            double shadowRadius = -5.0;

            // Act
            var result = AndroidSpecific.ImageButton.SetShadowRadius(mockConfig, shadowRadius);

            // Assert
            mockElement.Received(1).SetValue(AndroidSpecific.ImageButton.ShadowRadiusProperty, shadowRadius);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetShadowRadius extension method correctly handles maximum double value
        /// and returns the same configuration object for method chaining.
        /// </summary>
        [Fact]
        public void SetShadowRadius_MaxValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton>>();
            mockConfig.Element.Returns(mockElement);
            double shadowRadius = double.MaxValue;

            // Act
            var result = AndroidSpecific.ImageButton.SetShadowRadius(mockConfig, shadowRadius);

            // Assert
            mockElement.Received(1).SetValue(AndroidSpecific.ImageButton.ShadowRadiusProperty, shadowRadius);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetShadowRadius extension method correctly handles minimum double value
        /// and returns the same configuration object for method chaining.
        /// </summary>
        [Fact]
        public void SetShadowRadius_MinValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton>>();
            mockConfig.Element.Returns(mockElement);
            double shadowRadius = double.MinValue;

            // Act
            var result = AndroidSpecific.ImageButton.SetShadowRadius(mockConfig, shadowRadius);

            // Assert
            mockElement.Received(1).SetValue(AndroidSpecific.ImageButton.ShadowRadiusProperty, shadowRadius);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetShadowRadius extension method correctly handles NaN value
        /// and returns the same configuration object for method chaining.
        /// </summary>
        [Fact]
        public void SetShadowRadius_NaNValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton>>();
            mockConfig.Element.Returns(mockElement);
            double shadowRadius = double.NaN;

            // Act
            var result = AndroidSpecific.ImageButton.SetShadowRadius(mockConfig, shadowRadius);

            // Assert
            mockElement.Received(1).SetValue(AndroidSpecific.ImageButton.ShadowRadiusProperty, shadowRadius);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetShadowRadius extension method correctly handles positive infinity value
        /// and returns the same configuration object for method chaining.
        /// </summary>
        [Fact]
        public void SetShadowRadius_PositiveInfinityValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton>>();
            mockConfig.Element.Returns(mockElement);
            double shadowRadius = double.PositiveInfinity;

            // Act
            var result = AndroidSpecific.ImageButton.SetShadowRadius(mockConfig, shadowRadius);

            // Assert
            mockElement.Received(1).SetValue(AndroidSpecific.ImageButton.ShadowRadiusProperty, shadowRadius);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetShadowRadius extension method correctly handles negative infinity value
        /// and returns the same configuration object for method chaining.
        /// </summary>
        [Fact]
        public void SetShadowRadius_NegativeInfinityValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton>>();
            mockConfig.Element.Returns(mockElement);
            double shadowRadius = double.NegativeInfinity;

            // Act
            var result = AndroidSpecific.ImageButton.SetShadowRadius(mockConfig, shadowRadius);

            // Assert
            mockElement.Received(1).SetValue(AndroidSpecific.ImageButton.ShadowRadiusProperty, shadowRadius);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetShadowRadius throws ArgumentNullException when config parameter is null
        /// when called as a static method rather than extension method.
        /// </summary>
        [Fact]
        public void SetShadowRadius_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton> config = null;
            double shadowRadius = 10.0;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                AndroidSpecific.ImageButton.SetShadowRadius(config, shadowRadius));
        }

        /// <summary>
        /// Tests that SetShadowRadius throws NullReferenceException when config.Element is null.
        /// </summary>
        [Fact]
        public void SetShadowRadius_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Microsoft.Maui.Controls.ImageButton>>();
            mockConfig.Element.Returns((BindableObject)null);
            double shadowRadius = 10.0;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                AndroidSpecific.ImageButton.SetShadowRadius(mockConfig, shadowRadius));
        }

        /// <summary>
        /// Tests that GetIsShadowEnabled throws NullReferenceException when element parameter is null.
        /// Verifies proper null handling for the element parameter.
        /// Expected result: NullReferenceException should be thrown.
        /// </summary>
        [Fact]
        public void GetIsShadowEnabled_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                PlatformConfiguration.AndroidSpecific.ImageButton.GetIsShadowEnabled(element));
        }

        /// <summary>
        /// Tests that GetIsShadowEnabled returns the correct boolean value from the BindableObject.
        /// Verifies that the method properly calls GetValue and casts the result to bool.
        /// Expected result: Should return the value from GetValue cast to bool.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetIsShadowEnabled_ValidElement_ReturnsExpectedValue(bool expectedValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(PlatformConfiguration.AndroidSpecific.ImageButton.IsShadowEnabledProperty)
                .Returns(expectedValue);

            // Act
            var result = PlatformConfiguration.AndroidSpecific.ImageButton.GetIsShadowEnabled(element);

            // Assert
            Assert.Equal(expectedValue, result);
            element.Received(1).GetValue(PlatformConfiguration.AndroidSpecific.ImageButton.IsShadowEnabledProperty);
        }

        /// <summary>
        /// Tests that GetShadowOffset returns the correct Size value when element has a custom shadow offset set.
        /// Input: BindableObject with custom Size value set.
        /// Expected: Method returns the custom Size value.
        /// </summary>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(10.5, 20.3)]
        [InlineData(-5.0, -10.0)]
        [InlineData(double.MaxValue, double.MaxValue)]
        [InlineData(double.MinValue, double.MinValue)]
        [InlineData(double.PositiveInfinity, double.NegativeInfinity)]
        public void GetShadowOffset_ElementWithCustomValue_ReturnsCustomValue(double width, double height)
        {
            // Arrange
            var expectedSize = new Size(width, height);
            var element = Substitute.For<BindableObject>();
            element.GetValue(ImageButton.ShadowOffsetProperty).Returns(expectedSize);

            // Act
            var result = ImageButton.GetShadowOffset(element);

            // Assert
            Assert.Equal(expectedSize, result);
        }

        /// <summary>
        /// Tests that GetShadowOffset returns Size.Zero when element has the default shadow offset value.
        /// Input: BindableObject returning default Size.Zero value.
        /// Expected: Method returns Size.Zero.
        /// </summary>
        [Fact]
        public void GetShadowOffset_ElementWithDefaultValue_ReturnsSizeZero()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(ImageButton.ShadowOffsetProperty).Returns(Size.Zero);

            // Act
            var result = ImageButton.GetShadowOffset(element);

            // Assert
            Assert.Equal(Size.Zero, result);
        }

        /// <summary>
        /// Tests that GetShadowOffset throws NullReferenceException when element parameter is null.
        /// Input: null BindableObject element.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void GetShadowOffset_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ImageButton.GetShadowOffset(element));
        }

        /// <summary>
        /// Tests that GetShadowOffset handles special floating-point Size values correctly.
        /// Input: BindableObject with Size containing NaN values.
        /// Expected: Method returns Size with NaN values.
        /// </summary>
        [Fact]
        public void GetShadowOffset_ElementWithNaNSize_ReturnsNaNSize()
        {
            // Arrange
            var nanSize = new Size(double.NaN, double.NaN);
            var element = Substitute.For<BindableObject>();
            element.GetValue(ImageButton.ShadowOffsetProperty).Returns(nanSize);

            // Act
            var result = ImageButton.GetShadowOffset(element);

            // Assert
            Assert.Equal(nanSize, result);
        }

        /// <summary>
        /// Tests that GetShadowOffset correctly casts the return value from GetValue to Size.
        /// Input: BindableObject returning boxed Size value.
        /// Expected: Method returns correctly cast Size value.
        /// </summary>
        [Fact]
        public void GetShadowOffset_ElementReturnsBoxedSize_CorrectlyCastsToSize()
        {
            // Arrange
            var expectedSize = new Size(42.5, 67.8);
            var element = Substitute.For<BindableObject>();
            element.GetValue(ImageButton.ShadowOffsetProperty).Returns((object)expectedSize);

            // Act
            var result = ImageButton.GetShadowOffset(element);

            // Assert
            Assert.Equal(expectedSize, result);
            Assert.Equal(42.5, result.Width);
            Assert.Equal(67.8, result.Height);
        }

        /// <summary>
        /// Tests that SetRippleColor throws NullReferenceException when element parameter is null.
        /// </summary>
        [Fact]
        public void SetRippleColor_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            var color = Colors.Red;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                ImageButton.SetRippleColor(element, color));
        }

        /// <summary>
        /// Tests that SetRippleColor successfully sets the ripple color value on a valid BindableObject.
        /// Verifies that SetValue is called with the correct BindableProperty and color value.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0)] // Transparent
        [InlineData(1, 0, 0, 1)] // Red
        [InlineData(0, 1, 0, 1)] // Green  
        [InlineData(0, 0, 1, 1)] // Blue
        [InlineData(1, 1, 1, 1)] // White
        [InlineData(0.5, 0.5, 0.5, 0.5)] // Semi-transparent gray
        public void SetRippleColor_ValidElement_SetsValueCorrectly(float red, float green, float blue, float alpha)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var color = new Color(red, green, blue, alpha);

            // Act
            ImageButton.SetRippleColor(mockElement, color);

            // Assert
            mockElement.Received(1).SetValue(ImageButton.RippleColorProperty, color);
        }

        /// <summary>
        /// Tests that SetRippleColor works with default color value.
        /// Verifies the method handles the default Color struct value correctly.
        /// </summary>
        [Fact]
        public void SetRippleColor_DefaultColor_SetsValueCorrectly()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var defaultColor = default(Color);

            // Act
            ImageButton.SetRippleColor(mockElement, defaultColor);

            // Assert
            mockElement.Received(1).SetValue(ImageButton.RippleColorProperty, defaultColor);
        }

        /// <summary>
        /// Tests SetRippleColor extension method with valid configuration and color value.
        /// Verifies that the ripple color is set on the underlying element and the same config is returned.
        /// </summary>
        [Fact]
        public void SetRippleColor_WithValidConfigAndColor_SetsColorAndReturnsConfig()
        {
            // Arrange
            var element = new Controls.ImageButton();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton>>();
            config.Element.Returns(element);
            var color = Colors.Red;

            // Act
            var result = config.SetRippleColor(color);

            // Assert
            Assert.Same(config, result);
            var actualColor = AndroidSpecific.ImageButton.GetRippleColor(element);
            Assert.Equal(color, actualColor);
        }

        /// <summary>
        /// Tests SetRippleColor extension method with null color value.
        /// Verifies that null color can be set and the same config is returned.
        /// </summary>
        [Fact]
        public void SetRippleColor_WithNullColor_SetsNullColorAndReturnsConfig()
        {
            // Arrange
            var element = new Controls.ImageButton();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton>>();
            config.Element.Returns(element);
            Color color = null;

            // Act
            var result = config.SetRippleColor(color);

            // Assert
            Assert.Same(config, result);
            var actualColor = AndroidSpecific.ImageButton.GetRippleColor(element);
            Assert.Equal(color, actualColor);
        }

        /// <summary>
        /// Tests SetRippleColor extension method with default color value.
        /// Verifies that default color can be set and the same config is returned.
        /// </summary>
        [Fact]
        public void SetRippleColor_WithDefaultColor_SetsDefaultColorAndReturnsConfig()
        {
            // Arrange
            var element = new Controls.ImageButton();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton>>();
            config.Element.Returns(element);
            var color = default(Color);

            // Act
            var result = config.SetRippleColor(color);

            // Assert
            Assert.Same(config, result);
            var actualColor = AndroidSpecific.ImageButton.GetRippleColor(element);
            Assert.Equal(color, actualColor);
        }

        /// <summary>
        /// Tests SetRippleColor extension method with transparent color.
        /// Verifies that transparent color can be set and the same config is returned.
        /// </summary>
        [Fact]
        public void SetRippleColor_WithTransparentColor_SetsTransparentColorAndReturnsConfig()
        {
            // Arrange
            var element = new Controls.ImageButton();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton>>();
            config.Element.Returns(element);
            var color = Colors.Transparent;

            // Act
            var result = config.SetRippleColor(color);

            // Assert
            Assert.Same(config, result);
            var actualColor = AndroidSpecific.ImageButton.GetRippleColor(element);
            Assert.Equal(color, actualColor);
        }

        /// <summary>
        /// Tests SetRippleColor extension method with null configuration.
        /// Verifies that ArgumentNullException is thrown when config is null.
        /// </summary>
        [Fact]
        public void SetRippleColor_WithNullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton> config = null;
            var color = Colors.Blue;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.SetRippleColor(color));
        }

        /// <summary>
        /// Tests SetRippleColor extension method when config.Element is null.
        /// Verifies that ArgumentNullException is thrown when the underlying element is null.
        /// </summary>
        [Fact]
        public void SetRippleColor_WithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Android, Controls.ImageButton>>();
            config.Element.Returns((Controls.ImageButton)null);
            var color = Colors.Green;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.SetRippleColor(color));
        }
    }
}