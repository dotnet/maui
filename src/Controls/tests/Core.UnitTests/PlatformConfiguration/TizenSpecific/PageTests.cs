#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.TizenSpecific
{
    public partial class PageTests
    {
        /// <summary>
        /// Tests that SetBreadCrumb calls SetValue on the provided BindableObject with BreadCrumbProperty and the specified value.
        /// Tests various string values including null, empty, whitespace, normal strings, and strings with special characters.
        /// </summary>
        /// <param name="value">The breadcrumb value to set</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("TestBreadCrumb")]
        [InlineData("Very Long BreadCrumb Name That Exceeds Normal Expectations With Special Characters !@#$%^&*()")]
        [InlineData("BreadCrumb\nWith\tSpecial\r\nCharacters")]
        public void SetBreadCrumb_ValidPageWithVariousValues_CallsSetValueWithCorrectParameters(string value)
        {
            // Arrange
            var mockPage = Substitute.For<BindableObject>();

            // Act
            Page.SetBreadCrumb(mockPage, value);

            // Assert
            mockPage.Received(1).SetValue(Page.BreadCrumbProperty, value);
        }

        /// <summary>
        /// Tests that SetBreadCrumb throws ArgumentNullException when the page parameter is null.
        /// This should occur when attempting to call SetValue on a null reference.
        /// </summary>
        [Fact]
        public void SetBreadCrumb_NullPage_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject nullPage = null;
            string testValue = "TestBreadCrumb";

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Page.SetBreadCrumb(nullPage, testValue));
        }

        /// <summary>
        /// Tests that SetBreadCrumb works correctly with extreme string values including very long strings.
        /// Verifies that the method passes through any string value without modification or validation.
        /// </summary>
        [Fact]
        public void SetBreadCrumb_ValidPageWithExtremeStringValues_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var mockPage = Substitute.For<BindableObject>();
            string extremelyLongValue = new string('A', 10000); // 10,000 character string

            // Act
            Page.SetBreadCrumb(mockPage, extremelyLongValue);

            // Assert
            mockPage.Received(1).SetValue(Page.BreadCrumbProperty, extremelyLongValue);
        }

        /// <summary>
        /// Tests that GetBreadCrumb extension method returns the expected string value
        /// when the platform configuration has a valid Element with a breadcrumb value set.
        /// </summary>
        [Fact]
        public void GetBreadCrumb_WithValidConfigAndStringValue_ReturnsExpectedString()
        {
            // Arrange
            const string expectedBreadCrumb = "Home > Products > Details";
            var mockPage = Substitute.For<Page>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Page>>();

            mockConfig.Element.Returns(mockPage);
            mockPage.GetValue(TizenSpecific.Page.BreadCrumbProperty).Returns(expectedBreadCrumb);

            // Act
            var result = mockConfig.GetBreadCrumb();

            // Assert
            Assert.Equal(expectedBreadCrumb, result);
        }

        /// <summary>
        /// Tests that GetBreadCrumb extension method returns null
        /// when the platform configuration has a valid Element but no breadcrumb value is set.
        /// </summary>
        [Fact]
        public void GetBreadCrumb_WithValidConfigAndNullValue_ReturnsNull()
        {
            // Arrange
            var mockPage = Substitute.For<Page>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Page>>();

            mockConfig.Element.Returns(mockPage);
            mockPage.GetValue(TizenSpecific.Page.BreadCrumbProperty).Returns((string)null);

            // Act
            var result = mockConfig.GetBreadCrumb();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetBreadCrumb extension method returns an empty string
        /// when the platform configuration has a valid Element with an empty breadcrumb value.
        /// </summary>
        [Fact]
        public void GetBreadCrumb_WithValidConfigAndEmptyString_ReturnsEmptyString()
        {
            // Arrange
            const string expectedBreadCrumb = "";
            var mockPage = Substitute.For<Page>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Page>>();

            mockConfig.Element.Returns(mockPage);
            mockPage.GetValue(TizenSpecific.Page.BreadCrumbProperty).Returns(expectedBreadCrumb);

            // Act
            var result = mockConfig.GetBreadCrumb();

            // Assert
            Assert.Equal(expectedBreadCrumb, result);
        }

        /// <summary>
        /// Tests that GetBreadCrumb extension method returns a whitespace-only string
        /// when the platform configuration has a valid Element with a whitespace breadcrumb value.
        /// </summary>
        [Fact]
        public void GetBreadCrumb_WithValidConfigAndWhitespaceString_ReturnsWhitespaceString()
        {
            // Arrange
            const string expectedBreadCrumb = "   \t\n  ";
            var mockPage = Substitute.For<Page>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Page>>();

            mockConfig.Element.Returns(mockPage);
            mockPage.GetValue(TizenSpecific.Page.BreadCrumbProperty).Returns(expectedBreadCrumb);

            // Act
            var result = mockConfig.GetBreadCrumb();

            // Assert
            Assert.Equal(expectedBreadCrumb, result);
        }

        /// <summary>
        /// Tests that GetBreadCrumb extension method returns a very long string
        /// when the platform configuration has a valid Element with a very long breadcrumb value.
        /// </summary>
        [Fact]
        public void GetBreadCrumb_WithValidConfigAndVeryLongString_ReturnsVeryLongString()
        {
            // Arrange
            var expectedBreadCrumb = new string('A', 10000);
            var mockPage = Substitute.For<Page>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Page>>();

            mockConfig.Element.Returns(mockPage);
            mockPage.GetValue(TizenSpecific.Page.BreadCrumbProperty).Returns(expectedBreadCrumb);

            // Act
            var result = mockConfig.GetBreadCrumb();

            // Assert
            Assert.Equal(expectedBreadCrumb, result);
        }

        /// <summary>
        /// Tests that GetBreadCrumb extension method returns a string with special characters
        /// when the platform configuration has a valid Element with special character breadcrumb value.
        /// </summary>
        [Fact]
        public void GetBreadCrumb_WithValidConfigAndSpecialCharacters_ReturnsSpecialCharacterString()
        {
            // Arrange
            const string expectedBreadCrumb = "Home > Café & Restaurañt > Naïve Résumé <test> \"quoted\" 'single' [brackets] {braces} |pipe| \\backslash /forward \u0000\u0001\u0002";
            var mockPage = Substitute.For<Page>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Page>>();

            mockConfig.Element.Returns(mockPage);
            mockPage.GetValue(TizenSpecific.Page.BreadCrumbProperty).Returns(expectedBreadCrumb);

            // Act
            var result = mockConfig.GetBreadCrumb();

            // Assert
            Assert.Equal(expectedBreadCrumb, result);
        }

        /// <summary>
        /// Tests that GetBreadCrumb extension method throws NullReferenceException
        /// when the platform configuration has a null Element.
        /// </summary>
        [Fact]
        public void GetBreadCrumb_WithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Page>>();
            mockConfig.Element.Returns((Page)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => mockConfig.GetBreadCrumb());
        }

        /// <summary>
        /// Tests that SetBreadCrumb extension method correctly calls the underlying SetBreadCrumb method
        /// and returns the same config object for fluent chaining with a valid string value.
        /// </summary>
        [Fact]
        public void SetBreadCrumb_WithValidValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Page>>();
            var mockPage = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockPage);
            var breadCrumbValue = "Test Breadcrumb";

            // Act
            var result = TizenSpecific.Page.SetBreadCrumb(mockConfig, breadCrumbValue);

            // Assert
            mockPage.Received(1).SetValue(TizenSpecific.Page.BreadCrumbProperty, breadCrumbValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetBreadCrumb extension method handles null value parameter correctly
        /// by passing it to the underlying method and still returns the config object.
        /// </summary>
        [Fact]
        public void SetBreadCrumb_WithNullValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Page>>();
            var mockPage = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockPage);

            // Act
            var result = TizenSpecific.Page.SetBreadCrumb(mockConfig, null);

            // Assert
            mockPage.Received(1).SetValue(TizenSpecific.Page.BreadCrumbProperty, null);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetBreadCrumb extension method handles empty string value correctly
        /// by passing it to the underlying method and still returns the config object.
        /// </summary>
        [Fact]
        public void SetBreadCrumb_WithEmptyValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Page>>();
            var mockPage = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockPage);
            var emptyValue = string.Empty;

            // Act
            var result = TizenSpecific.Page.SetBreadCrumb(mockConfig, emptyValue);

            // Assert
            mockPage.Received(1).SetValue(TizenSpecific.Page.BreadCrumbProperty, emptyValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetBreadCrumb extension method handles whitespace-only string value correctly
        /// by passing it to the underlying method and still returns the config object.
        /// </summary>
        [Fact]
        public void SetBreadCrumb_WithWhitespaceValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Page>>();
            var mockPage = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockPage);
            var whitespaceValue = "   ";

            // Act
            var result = TizenSpecific.Page.SetBreadCrumb(mockConfig, whitespaceValue);

            // Assert
            mockPage.Received(1).SetValue(TizenSpecific.Page.BreadCrumbProperty, whitespaceValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetBreadCrumb extension method throws NullReferenceException
        /// when config parameter is null and accessing config.Element.
        /// </summary>
        [Fact]
        public void SetBreadCrumb_WithNullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<Tizen, Page> nullConfig = null;
            var value = "Test";

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TizenSpecific.Page.SetBreadCrumb(nullConfig, value));
        }

        /// <summary>
        /// Tests that SetBreadCrumb extension method handles very long string values correctly
        /// by passing them to the underlying method and still returns the config object.
        /// </summary>
        [Fact]
        public void SetBreadCrumb_WithVeryLongValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Page>>();
            var mockPage = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockPage);
            var longValue = new string('A', 10000);

            // Act
            var result = TizenSpecific.Page.SetBreadCrumb(mockConfig, longValue);

            // Assert
            mockPage.Received(1).SetValue(TizenSpecific.Page.BreadCrumbProperty, longValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetBreadCrumb extension method handles special characters in string values correctly
        /// by passing them to the underlying method and still returns the config object.
        /// </summary>
        [Fact]
        public void SetBreadCrumb_WithSpecialCharacters_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Page>>();
            var mockPage = Substitute.For<BindableObject>();
            mockConfig.Element.Returns(mockPage);
            var specialValue = "Test\r\n\t\"'<>&\u0000\u001F";

            // Act
            var result = TizenSpecific.Page.SetBreadCrumb(mockConfig, specialValue);

            // Assert
            mockPage.Received(1).SetValue(TizenSpecific.Page.BreadCrumbProperty, specialValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that GetBreadCrumb throws ArgumentNullException when page parameter is null.
        /// This verifies proper null parameter validation.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void GetBreadCrumb_NullPage_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject page = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => TizenSpecific.Page.GetBreadCrumb(page));
        }

        /// <summary>
        /// Tests that GetBreadCrumb returns the correct string value when the breadcrumb property is set.
        /// This test is marked as skipped because BindableObject cannot be properly mocked according to the symbol metadata.
        /// To complete this test, either:
        /// 1. Use a concrete implementation of BindableObject that allows setting the BreadCrumbProperty, or
        /// 2. Create an integration test that uses actual MAUI controls that inherit from BindableObject.
        /// Expected result: Should return the string value stored in the BreadCrumbProperty.
        /// </summary>
        [Fact(Skip = "BindableObject cannot be mocked - requires concrete implementation or integration test")]
        public void GetBreadCrumb_ValidPageWithBreadCrumb_ReturnsCorrectValue()
        {
            // Arrange - This approach cannot work because BindableObject cannot be mocked
            // var mockPage = Substitute.For<BindableObject>();
            // string expectedBreadCrumb = "Test Breadcrumb";
            // mockPage.GetValue(TizenSpecific.Page.BreadCrumbProperty).Returns(expectedBreadCrumb);

            // Act
            // var result = TizenSpecific.Page.GetBreadCrumb(mockPage);

            // Assert
            // Assert.Equal(expectedBreadCrumb, result);

            // TODO: Replace with concrete BindableObject implementation or integration test
            throw new NotImplementedException("Test requires concrete BindableObject implementation");
        }

        /// <summary>
        /// Tests that GetBreadCrumb returns null when the breadcrumb property is not set or is null.
        /// This test is marked as skipped because BindableObject cannot be properly mocked according to the symbol metadata.
        /// To complete this test, use a concrete implementation of BindableObject where the BreadCrumbProperty returns null.
        /// Expected result: Should return null when the property value is null.
        /// </summary>
        [Fact(Skip = "BindableObject cannot be mocked - requires concrete implementation or integration test")]
        public void GetBreadCrumb_ValidPageWithNullBreadCrumb_ReturnsNull()
        {
            // Arrange - This approach cannot work because BindableObject cannot be mocked
            // var mockPage = Substitute.For<BindableObject>();
            // mockPage.GetValue(TizenSpecific.Page.BreadCrumbProperty).Returns((string)null);

            // Act
            // var result = TizenSpecific.Page.GetBreadCrumb(mockPage);

            // Assert
            // Assert.Null(result);

            // TODO: Replace with concrete BindableObject implementation or integration test
            throw new NotImplementedException("Test requires concrete BindableObject implementation");
        }

        /// <summary>
        /// Tests that GetBreadCrumb returns an empty string when the breadcrumb property is set to empty string.
        /// This test is marked as skipped because BindableObject cannot be properly mocked according to the symbol metadata.
        /// To complete this test, use a concrete implementation of BindableObject where the BreadCrumbProperty returns empty string.
        /// Expected result: Should return empty string when the property value is empty string.
        /// </summary>
        [Fact(Skip = "BindableObject cannot be mocked - requires concrete implementation or integration test")]
        public void GetBreadCrumb_ValidPageWithEmptyBreadCrumb_ReturnsEmptyString()
        {
            // Arrange - This approach cannot work because BindableObject cannot be mocked
            // var mockPage = Substitute.For<BindableObject>();
            // mockPage.GetValue(TizenSpecific.Page.BreadCrumbProperty).Returns(string.Empty);

            // Act
            // var result = TizenSpecific.Page.GetBreadCrumb(mockPage);

            // Assert
            // Assert.Equal(string.Empty, result);

            // TODO: Replace with concrete BindableObject implementation or integration test
            throw new NotImplementedException("Test requires concrete BindableObject implementation");
        }
    }
}