#nullable disable

using System;
using System.Collections;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the SearchBar class in the WindowsSpecific platform configuration.
    /// </summary>
    public partial class SearchBarTests
    {
        /// <summary>
        /// Tests that GetIsSpellCheckEnabled throws NullReferenceException when element is null.
        /// Input: null BindableObject element.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void GetIsSpellCheckEnabled_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => SearchBar.GetIsSpellCheckEnabled(element));
        }

        /// <summary>
        /// Tests that GetIsSpellCheckEnabled returns the correct boolean value from the element's property.
        /// Input: Valid BindableObject element with specified boolean property values.
        /// Expected: Returns the boolean value returned by element.GetValue().
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetIsSpellCheckEnabled_ValidElement_ReturnsPropertyValue(bool expectedValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(SearchBar.IsSpellCheckEnabledProperty).Returns(expectedValue);

            // Act
            bool result = SearchBar.GetIsSpellCheckEnabled(element);

            // Assert
            Assert.Equal(expectedValue, result);
            element.Received(1).GetValue(SearchBar.IsSpellCheckEnabledProperty);
        }

        /// <summary>
        /// Tests that GetIsSpellCheckEnabled properly casts the object returned by GetValue to bool.
        /// Input: Valid BindableObject element with boxed boolean values.
        /// Expected: Returns the correctly cast boolean value.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetIsSpellCheckEnabled_ValidElementWithBoxedValue_ReturnsCorrectBooleanValue(bool expectedValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            object boxedValue = expectedValue;
            element.GetValue(SearchBar.IsSpellCheckEnabledProperty).Returns(boxedValue);

            // Act
            bool result = SearchBar.GetIsSpellCheckEnabled(element);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetIsSpellCheckEnabled throws ArgumentNullException when config parameter is null.
        /// Verifies null parameter validation for the extension method.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void GetIsSpellCheckEnabled_ConfigIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Windows, SearchBar> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SearchBar.GetIsSpellCheckEnabled(config));
        }

        /// <summary>
        /// Tests that GetIsSpellCheckEnabled returns true when the underlying SearchBar has spell check enabled.
        /// Verifies the method correctly delegates to the underlying GetIsSpellCheckEnabled method.
        /// Expected result: Should return true.
        /// </summary>
        [Fact]
        public void GetIsSpellCheckEnabled_SpellCheckEnabled_ReturnsTrue()
        {
            // Arrange
            var searchBar = new SearchBar();
            SearchBar.SetIsSpellCheckEnabled(searchBar, true);

            var config = Substitute.For<IPlatformElementConfiguration<Windows, SearchBar>>();
            config.Element.Returns(searchBar);

            // Act
            var result = SearchBar.GetIsSpellCheckEnabled(config);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetIsSpellCheckEnabled returns false when the underlying SearchBar has spell check disabled.
        /// Verifies the method correctly delegates to the underlying GetIsSpellCheckEnabled method.
        /// Expected result: Should return false.
        /// </summary>
        [Fact]
        public void GetIsSpellCheckEnabled_SpellCheckDisabled_ReturnsFalse()
        {
            // Arrange
            var searchBar = new SearchBar();
            SearchBar.SetIsSpellCheckEnabled(searchBar, false);

            var config = Substitute.For<IPlatformElementConfiguration<Windows, SearchBar>>();
            config.Element.Returns(searchBar);

            // Act
            var result = SearchBar.GetIsSpellCheckEnabled(config);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetIsSpellCheckEnabled returns false for default SearchBar with no spell check configuration.
        /// Verifies the method returns the default value when spell check property is not explicitly set.
        /// Expected result: Should return false (the default value).
        /// </summary>
        [Fact]
        public void GetIsSpellCheckEnabled_DefaultSearchBar_ReturnsFalse()
        {
            // Arrange
            var searchBar = new SearchBar();

            var config = Substitute.For<IPlatformElementConfiguration<Windows, SearchBar>>();
            config.Element.Returns(searchBar);

            // Act
            var result = SearchBar.GetIsSpellCheckEnabled(config);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that SetIsSpellCheckEnabled throws NullReferenceException when config parameter is null.
        /// This test verifies that the method properly handles null input by attempting to access config.Element.
        /// Expected result: NullReferenceException should be thrown.
        /// </summary>
        [Fact]
        public void SetIsSpellCheckEnabled_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Windows, SearchBar> config = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => SearchBarExtensions.SetIsSpellCheckEnabled(config, value));
        }

        /// <summary>
        /// Tests that SetIsSpellCheckEnabled returns the same config object and accesses the Element property.
        /// This test verifies that the method follows the fluent API pattern by returning the input config
        /// and that it accesses the Element property to call the underlying SetIsSpellCheckEnabled method.
        /// Expected result: The same config object should be returned and Element property should be accessed.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetIsSpellCheckEnabled_ValidConfig_ReturnsConfigAndAccessesElement(bool value)
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, SearchBar>>();
            var mockSearchBar = Substitute.For<SearchBar>();
            mockConfig.Element.Returns(mockSearchBar);

            // Act
            var result = SearchBarExtensions.SetIsSpellCheckEnabled(mockConfig, value);

            // Assert
            Assert.Same(mockConfig, result);
            var elementAccessed = mockConfig.Received(1).Element;
        }

        /// <summary>
        /// Tests that IsSpellCheckEnabled throws ArgumentNullException when config parameter is null.
        /// Input: null config parameter.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void IsSpellCheckEnabled_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.SearchBar> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => WindowsSpecific.SearchBar.IsSpellCheckEnabled(config));
        }

        /// <summary>
        /// Tests that IsSpellCheckEnabled throws NullReferenceException when config.Element is null.
        /// Input: config with null Element property.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void IsSpellCheckEnabled_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.SearchBar>>();
            config.Element.Returns((Microsoft.Maui.Controls.SearchBar)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => WindowsSpecific.SearchBar.IsSpellCheckEnabled(config));
        }

        /// <summary>
        /// Tests that IsSpellCheckEnabled returns true when spell check is enabled on the search bar.
        /// Input: valid config with Element that has spell check enabled.
        /// Expected: Returns true.
        /// </summary>
        [Fact]
        public void IsSpellCheckEnabled_ValidConfigWithSpellCheckEnabled_ReturnsTrue()
        {
            // Arrange
            var searchBar = Substitute.For<Microsoft.Maui.Controls.SearchBar>();
            var config = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.SearchBar>>();

            config.Element.Returns(searchBar);
            searchBar.GetValue(WindowsSpecific.SearchBar.IsSpellCheckEnabledProperty).Returns(true);

            // Act
            var result = WindowsSpecific.SearchBar.IsSpellCheckEnabled(config);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsSpellCheckEnabled returns false when spell check is disabled on the search bar.
        /// Input: valid config with Element that has spell check disabled.
        /// Expected: Returns false.
        /// </summary>
        [Fact]
        public void IsSpellCheckEnabled_ValidConfigWithSpellCheckDisabled_ReturnsFalse()
        {
            // Arrange
            var searchBar = Substitute.For<Microsoft.Maui.Controls.SearchBar>();
            var config = Substitute.For<IPlatformElementConfiguration<Windows, Microsoft.Maui.Controls.SearchBar>>();

            config.Element.Returns(searchBar);
            searchBar.GetValue(WindowsSpecific.SearchBar.IsSpellCheckEnabledProperty).Returns(false);

            // Act
            var result = WindowsSpecific.SearchBar.IsSpellCheckEnabled(config);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that EnableSpellCheck calls SetIsSpellCheckEnabled with true when provided with a valid config.
        /// Expected result: SetValue is called on the element with IsSpellCheckEnabledProperty and true.
        /// </summary>
        [Fact]
        public void EnableSpellCheck_ValidConfig_CallsSetIsSpellCheckEnabledWithTrue()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, SearchBar>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            WindowsSpecific.SearchBar.EnableSpellCheck(mockConfig);

            // Assert
            mockElement.Received(1).SetValue(WindowsSpecific.SearchBar.IsSpellCheckEnabledProperty, true);
        }

        /// <summary>
        /// Tests that EnableSpellCheck throws ArgumentNullException when config parameter is null.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void EnableSpellCheck_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Windows, SearchBar> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => WindowsSpecific.SearchBar.EnableSpellCheck(config));
        }

        /// <summary>
        /// Tests that EnableSpellCheck throws NullReferenceException when config.Element is null.
        /// Expected result: NullReferenceException is thrown when trying to access Element property.
        /// </summary>
        [Fact]
        public void EnableSpellCheck_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, SearchBar>>();
            mockConfig.Element.Returns((SearchBar)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => WindowsSpecific.SearchBar.EnableSpellCheck(mockConfig));
        }

        /// <summary>
        /// Tests that DisableSpellCheck correctly calls SetIsSpellCheckEnabled with false value.
        /// Verifies that the method delegates to SetIsSpellCheckEnabled with the config element and false parameter.
        /// Expected result: SetValue should be called on the element with IsSpellCheckEnabledProperty and false.
        /// </summary>
        [Fact]
        public void DisableSpellCheck_ValidConfig_CallsSetIsSpellCheckEnabledWithFalse()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Windows, Controls.SearchBar>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            WindowsSpecific.SearchBar.DisableSpellCheck(mockConfig);

            // Assert
            mockElement.Received(1).SetValue(WindowsSpecific.SearchBar.IsSpellCheckEnabledProperty, false);
        }

        /// <summary>
        /// Tests that SetIsSpellCheckEnabled throws ArgumentNullException when element parameter is null.
        /// </summary>
        [Fact]
        public void SetIsSpellCheckEnabled_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;
            bool value = true;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SearchBar.SetIsSpellCheckEnabled(element, value));
        }

        /// <summary>
        /// Tests that SetIsSpellCheckEnabled calls SetValue with correct parameters when value is true.
        /// </summary>
        [Fact]
        public void SetIsSpellCheckEnabled_ValidElementAndTrueValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            bool value = true;

            // Act
            SearchBar.SetIsSpellCheckEnabled(element, value);

            // Assert
            element.Received(1).SetValue(SearchBar.IsSpellCheckEnabledProperty, value);
        }

        /// <summary>
        /// Tests that SetIsSpellCheckEnabled calls SetValue with correct parameters when value is false.
        /// </summary>
        [Fact]
        public void SetIsSpellCheckEnabled_ValidElementAndFalseValue_CallsSetValueWithCorrectParameters()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            bool value = false;

            // Act
            SearchBar.SetIsSpellCheckEnabled(element, value);

            // Assert
            element.Received(1).SetValue(SearchBar.IsSpellCheckEnabledProperty, value);
        }

        /// <summary>
        /// Tests that SetIsSpellCheckEnabled works correctly with both true and false values using parameterized test.
        /// </summary>
        /// <param name="value">The boolean value to test with.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetIsSpellCheckEnabled_ValidElement_CallsSetValueWithCorrectParameters(bool value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            SearchBar.SetIsSpellCheckEnabled(element, value);

            // Assert
            element.Received(1).SetValue(SearchBar.IsSpellCheckEnabledProperty, value);
        }
    }


    /// <summary>
    /// Helper class to access the extension methods as static methods for testing purposes.
    /// This allows us to test the extension methods more directly.
    /// </summary>
    internal static class SearchBarExtensions
    {
    }
}