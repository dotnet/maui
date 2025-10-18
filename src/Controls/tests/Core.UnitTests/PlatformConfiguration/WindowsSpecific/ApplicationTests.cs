#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.WindowsSpecific
{
    /// <summary>
    /// Unit tests for the Application class in WindowsSpecific platform configuration.
    /// </summary>
    public partial class ApplicationTests
    {
        /// <summary>
        /// Tests that GetImageDirectory throws NullReferenceException when config parameter is null.
        /// This test verifies the method's behavior with null input and ensures proper exception handling.
        /// Expected result: NullReferenceException should be thrown.
        /// </summary>
        [Fact]
        public void GetImageDirectory_ConfigIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.Application> config = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.GetImageDirectory(config));
        }

        /// <summary>
        /// Tests that GetImageDirectory returns null when Element.GetValue returns null.
        /// This test verifies the method's behavior when the underlying property value is null.
        /// Expected result: null should be returned after casting.
        /// </summary>
        [Fact]
        public void GetImageDirectory_ElementGetValueReturnsNull_ReturnsNull()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.Application>>();
            var element = Substitute.For<Microsoft.Maui.Controls.Application>();
            config.Element.Returns(element);
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.ImageDirectoryProperty).Returns((object)null);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.GetImageDirectory(config);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetImageDirectory returns empty string when Element.GetValue returns empty string.
        /// This test verifies the method's behavior with empty string values from the underlying property.
        /// Expected result: Empty string should be returned.
        /// </summary>
        [Fact]
        public void GetImageDirectory_ElementGetValueReturnsEmptyString_ReturnsEmptyString()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.Application>>();
            var element = Substitute.For<Microsoft.Maui.Controls.Application>();
            config.Element.Returns(element);
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.ImageDirectoryProperty).Returns(string.Empty);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.GetImageDirectory(config);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that GetImageDirectory returns whitespace string when Element.GetValue returns whitespace.
        /// This test verifies the method's behavior with whitespace-only string values.
        /// Expected result: Whitespace string should be returned as-is.
        /// </summary>
        [Fact]
        public void GetImageDirectory_ElementGetValueReturnsWhitespace_ReturnsWhitespace()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.Application>>();
            var element = Substitute.For<Microsoft.Maui.Controls.Application>();
            var whitespaceValue = "   \t\n  ";
            config.Element.Returns(element);
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.ImageDirectoryProperty).Returns(whitespaceValue);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.GetImageDirectory(config);

            // Assert
            Assert.Equal(whitespaceValue, result);
        }

        /// <summary>
        /// Tests that GetImageDirectory returns valid directory path when Element.GetValue returns a normal string.
        /// This test verifies the method's behavior with typical valid directory path values.
        /// Expected result: The directory path string should be returned unchanged.
        /// </summary>
        [Fact]
        public void GetImageDirectory_ElementGetValueReturnsValidPath_ReturnsValidPath()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.Application>>();
            var element = Substitute.For<Microsoft.Maui.Controls.Application>();
            var expectedPath = "Assets/Images";
            config.Element.Returns(element);
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.ImageDirectoryProperty).Returns(expectedPath);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.GetImageDirectory(config);

            // Assert
            Assert.Equal(expectedPath, result);
        }

        /// <summary>
        /// Tests that GetImageDirectory returns very long string when Element.GetValue returns a very long string.
        /// This test verifies the method's behavior with extremely long string values that could cause memory issues.
        /// Expected result: The long string should be returned unchanged.
        /// </summary>
        [Fact]
        public void GetImageDirectory_ElementGetValueReturnsVeryLongString_ReturnsVeryLongString()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.Application>>();
            var element = Substitute.For<Microsoft.Maui.Controls.Application>();
            var longString = new string('a', 10000);
            config.Element.Returns(element);
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.ImageDirectoryProperty).Returns(longString);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.GetImageDirectory(config);

            // Assert
            Assert.Equal(longString, result);
        }

        /// <summary>
        /// Tests that GetImageDirectory returns string with special characters when Element.GetValue returns such a string.
        /// This test verifies the method's behavior with strings containing special characters, unicode, and control characters.
        /// Expected result: The string with special characters should be returned unchanged.
        /// </summary>
        [Fact]
        public void GetImageDirectory_ElementGetValueReturnsStringWithSpecialCharacters_ReturnsStringWithSpecialCharacters()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.Application>>();
            var element = Substitute.For<Microsoft.Maui.Controls.Application>();
            var specialString = "Path/With/Special@#$%^&*()_+Characters\u0001\u0002东西";
            config.Element.Returns(element);
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.ImageDirectoryProperty).Returns(specialString);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.GetImageDirectory(config);

            // Assert
            Assert.Equal(specialString, result);
        }

        /// <summary>
        /// Tests that GetImageDirectory throws NullReferenceException when config.Element is null.
        /// This test verifies the method's behavior when the Element property returns null.
        /// Expected result: NullReferenceException should be thrown when trying to call GetValue on null Element.
        /// </summary>
        [Fact]
        public void GetImageDirectory_ConfigElementIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.Application>>();
            config.Element.Returns((Microsoft.Maui.Controls.Application)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.GetImageDirectory(config));
        }

        /// <summary>
        /// Tests that GetImageDirectory returns the correct value when the ImageDirectory property is set to a normal string value.
        /// Input: BindableObject with ImageDirectory set to "test/path"
        /// Expected: Returns "test/path"
        /// </summary>
        [Fact]
        public void GetImageDirectory_WithValidElementAndSetValue_ReturnsExpectedValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var expectedValue = "test/path";
            element.GetValue(Application.ImageDirectoryProperty).Returns(expectedValue);

            // Act
            var result = Application.GetImageDirectory(element);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetImageDirectory returns different string values correctly using parameterized test data.
        /// Input: BindableObject with various ImageDirectory values
        /// Expected: Returns the corresponding string value
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("simple/path")]
        [InlineData("C:\\Windows\\System32")]
        [InlineData("/usr/local/bin")]
        [InlineData("   ")]
        [InlineData("path with spaces")]
        [InlineData("path/with/special/chars!@#$%")]
        public void GetImageDirectory_WithVariousStringValues_ReturnsCorrectValue(string expectedValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Application.ImageDirectoryProperty).Returns(expectedValue);

            // Act
            var result = Application.GetImageDirectory(element);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetImageDirectory returns empty string when the property is not set (default value).
        /// Input: BindableObject with no ImageDirectory set
        /// Expected: Returns string.Empty (default value)
        /// </summary>
        [Fact]
        public void GetImageDirectory_WithDefaultValue_ReturnsEmptyString()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Application.ImageDirectoryProperty).Returns(string.Empty);

            // Act
            var result = Application.GetImageDirectory(element);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that GetImageDirectory handles null return value from GetValue by casting it to string.
        /// Input: BindableObject that returns null from GetValue
        /// Expected: Returns null (cast from object null to string null)
        /// </summary>
        [Fact]
        public void GetImageDirectory_WithNullReturnFromGetValue_ReturnsNull()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Application.ImageDirectoryProperty).Returns((object)null);

            // Act
            var result = Application.GetImageDirectory(element);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetImageDirectory throws NullReferenceException when element parameter is null.
        /// Input: null BindableObject
        /// Expected: Throws NullReferenceException
        /// </summary>
        [Fact]
        public void GetImageDirectory_WithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Application.GetImageDirectory(element));
        }

        /// <summary>
        /// Tests that GetImageDirectory handles very long string values correctly.
        /// Input: BindableObject with very long ImageDirectory value
        /// Expected: Returns the complete long string
        /// </summary>
        [Fact]
        public void GetImageDirectory_WithVeryLongString_ReturnsCompleteString()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var longPath = new string('a', 1000) + "/path/" + new string('b', 1000);
            element.GetValue(Application.ImageDirectoryProperty).Returns(longPath);

            // Act
            var result = Application.GetImageDirectory(element);

            // Assert
            Assert.Equal(longPath, result);
        }

        /// <summary>
        /// Tests that SetImageDirectory sets the value on the element and returns the config object when provided with a valid string value.
        /// Input: Valid config object and non-null string value.
        /// Expected: SetValue called with correct parameters and same config object returned.
        /// </summary>
        [Fact]
        public void SetImageDirectory_ValidString_SetsValueAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.Application>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.Application>();
            mockConfig.Element.Returns(mockElement);
            string testValue = "test/directory/path";

            // Act
            var result = mockConfig.SetImageDirectory(testValue);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.ImageDirectoryProperty, testValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetImageDirectory handles null string values correctly.
        /// Input: Valid config object and null string value.
        /// Expected: SetValue called with null and same config object returned.
        /// </summary>
        [Fact]
        public void SetImageDirectory_NullValue_SetsNullValueAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.Application>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.Application>();
            mockConfig.Element.Returns(mockElement);
            string testValue = null;

            // Act
            var result = mockConfig.SetImageDirectory(testValue);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.ImageDirectoryProperty, testValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetImageDirectory handles empty string values correctly.
        /// Input: Valid config object and empty string value.
        /// Expected: SetValue called with empty string and same config object returned.
        /// </summary>
        [Fact]
        public void SetImageDirectory_EmptyString_SetsEmptyValueAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.Application>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.Application>();
            mockConfig.Element.Returns(mockElement);
            string testValue = string.Empty;

            // Act
            var result = mockConfig.SetImageDirectory(testValue);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.ImageDirectoryProperty, testValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetImageDirectory handles whitespace-only string values correctly.
        /// Input: Valid config object and whitespace-only string value.
        /// Expected: SetValue called with whitespace string and same config object returned.
        /// </summary>
        [Fact]
        public void SetImageDirectory_WhitespaceString_SetsWhitespaceValueAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.Application>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.Application>();
            mockConfig.Element.Returns(mockElement);
            string testValue = "   \t\n  ";

            // Act
            var result = mockConfig.SetImageDirectory(testValue);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.ImageDirectoryProperty, testValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetImageDirectory handles very long string values correctly.
        /// Input: Valid config object and very long string value.
        /// Expected: SetValue called with long string and same config object returned.
        /// </summary>
        [Fact]
        public void SetImageDirectory_VeryLongString_SetsLongValueAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.Application>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.Application>();
            mockConfig.Element.Returns(mockElement);
            string testValue = new string('a', 10000);

            // Act
            var result = mockConfig.SetImageDirectory(testValue);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.ImageDirectoryProperty, testValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetImageDirectory throws ArgumentNullException when config parameter is null.
        /// Input: Null config object and any string value.
        /// Expected: ArgumentNullException thrown.
        /// </summary>
        [Fact]
        public void SetImageDirectory_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.Application> nullConfig = null;
            string testValue = "test/directory/path";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => nullConfig.SetImageDirectory(testValue));
        }

        /// <summary>
        /// Tests that SetImageDirectory handles strings with special characters correctly.
        /// Input: Valid config object and string with special characters.
        /// Expected: SetValue called with special character string and same config object returned.
        /// </summary>
        [Fact]
        public void SetImageDirectory_SpecialCharacters_SetsSpecialValueAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.Application>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.Application>();
            mockConfig.Element.Returns(mockElement);
            string testValue = "test/directory\\path with spaces & special chars: !@#$%^&*()";

            // Act
            var result = mockConfig.SetImageDirectory(testValue);

            // Assert
            mockElement.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.ImageDirectoryProperty, testValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetImageDirectory throws ArgumentNullException when element parameter is null.
        /// Verifies that the method properly validates the required element parameter.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void SetImageDirectory_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;
            string value = "test-directory";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.SetImageDirectory(element, value));
        }

        /// <summary>
        /// Tests that SetImageDirectory successfully calls SetValue on the element with various string values.
        /// Verifies that the method properly forwards the value to the underlying BindableObject.SetValue method.
        /// Expected result: SetValue is called with ImageDirectoryProperty and the provided value.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("test-directory")]
        [InlineData("C:\\Program Files\\App\\Images")]
        [InlineData("/usr/local/share/images")]
        [InlineData("very-long-directory-name-that-exceeds-normal-length-expectations-and-contains-many-characters-to-test-boundary-conditions")]
        [InlineData("directory-with-special-chars-!@#$%^&*()_+-=[]{}|;':\",./<>?")]
        [InlineData("\t\n\r")]
        public void SetImageDirectory_ValidElement_CallsSetValueWithCorrectParameters(string value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.SetImageDirectory(element, value);

            // Assert
            element.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.ImageDirectoryProperty,
                value);
        }

        /// <summary>
        /// Tests that SetImageDirectory with empty string successfully calls SetValue.
        /// Verifies that empty strings are handled correctly without throwing exceptions.
        /// Expected result: SetValue is called with ImageDirectoryProperty and empty string.
        /// </summary>
        [Fact]
        public void SetImageDirectory_ValidElementWithEmptyString_CallsSetValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            string value = string.Empty;

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.SetImageDirectory(element, value);

            // Assert
            element.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.ImageDirectoryProperty,
                value);
        }

        /// <summary>
        /// Tests that SetImageDirectory with whitespace-only string successfully calls SetValue.
        /// Verifies that whitespace-only strings are handled correctly without trimming or validation.
        /// Expected result: SetValue is called with ImageDirectoryProperty and whitespace string.
        /// </summary>
        [Fact]
        public void SetImageDirectory_ValidElementWithWhitespaceString_CallsSetValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            string value = "   \t\n   ";

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.SetImageDirectory(element, value);

            // Assert
            element.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application.ImageDirectoryProperty,
                value);
        }
    }
}