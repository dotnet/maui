#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the StructuredItemsView class.
    /// </summary>
    public class StructuredItemsViewTests
    {
        /// <summary>
        /// Tests that the FooterTemplate property returns null by default.
        /// Verifies the getter retrieves the default value correctly.
        /// Expected result: null is returned.
        /// </summary>
        [Fact]
        public void FooterTemplate_DefaultValue_ReturnsNull()
        {
            // Arrange
            var view = new StructuredItemsView();

            // Act
            var result = view.FooterTemplate;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the FooterTemplate property can be set to a valid DataTemplate and retrieved.
        /// Verifies the setter stores the value and the getter retrieves it correctly.
        /// Expected result: the same DataTemplate instance is returned.
        /// </summary>
        [Fact]
        public void FooterTemplate_SetValidDataTemplate_ReturnsSetValue()
        {
            // Arrange
            var view = new StructuredItemsView();
            var dataTemplate = new DataTemplate();

            // Act
            view.FooterTemplate = dataTemplate;
            var result = view.FooterTemplate;

            // Assert
            Assert.Same(dataTemplate, result);
        }

        /// <summary>
        /// Tests that the FooterTemplate property can be set to null and retrieved.
        /// Verifies the setter accepts null values and the getter returns null correctly.
        /// Expected result: null is returned.
        /// </summary>
        [Fact]
        public void FooterTemplate_SetNull_ReturnsNull()
        {
            // Arrange
            var view = new StructuredItemsView();
            var initialTemplate = new DataTemplate();
            view.FooterTemplate = initialTemplate;

            // Act
            view.FooterTemplate = null;
            var result = view.FooterTemplate;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the FooterTemplate property works correctly with different DataTemplate types.
        /// Verifies the getter casts and returns various DataTemplate instances properly.
        /// Expected result: each DataTemplate type is stored and retrieved correctly.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetDataTemplateTestCases))]
        public void FooterTemplate_SetDifferentDataTemplateTypes_ReturnsCorrectValue(DataTemplate template, string description)
        {
            // Arrange
            var view = new StructuredItemsView();

            // Act
            view.FooterTemplate = template;
            var result = view.FooterTemplate;

            // Assert
            if (template == null)
            {
                Assert.Null(result);
            }
            else
            {
                Assert.Same(template, result);
            }
        }

        /// <summary>
        /// Tests that multiple set/get operations on FooterTemplate work consistently.
        /// Verifies the property maintains state correctly across multiple operations.
        /// Expected result: each value is stored and retrieved accurately.
        /// </summary>
        [Fact]
        public void FooterTemplate_MultipleSetGetOperations_MaintainsStateCorrectly()
        {
            // Arrange
            var view = new StructuredItemsView();
            var template1 = new DataTemplate();
            var template2 = new DataTemplate(typeof(Label));

            // Act & Assert - Initial state
            Assert.Null(view.FooterTemplate);

            // Set first template
            view.FooterTemplate = template1;
            Assert.Same(template1, view.FooterTemplate);

            // Set second template
            view.FooterTemplate = template2;
            Assert.Same(template2, view.FooterTemplate);

            // Set to null
            view.FooterTemplate = null;
            Assert.Null(view.FooterTemplate);

            // Set first template again
            view.FooterTemplate = template1;
            Assert.Same(template1, view.FooterTemplate);
        }

        /// <summary>
        /// Provides test data for different DataTemplate types.
        /// </summary>
        public static TheoryData<DataTemplate, string> GetDataTemplateTestCases()
        {
            return new TheoryData<DataTemplate, string>
            {
                { null, "null value" },
                { new DataTemplate(), "parameterless constructor" },
                { new DataTemplate(typeof(Label)), "type-based constructor" },
                { new DataTemplate(() => new Label()), "func-based constructor" }
            };
        }

        /// <summary>
        /// Tests that the Footer property returns null when no value has been set,
        /// verifying the default value behavior of the underlying BindableProperty.
        /// </summary>
        [Fact]
        public void Footer_WhenNoValueSet_ReturnsNull()
        {
            // Arrange
            var structuredItemsView = new StructuredItemsView();

            // Act
            var result = structuredItemsView.Footer;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Footer property correctly stores and retrieves various object types,
        /// including strings, ensuring the getter returns the exact value that was set.
        /// </summary>
        /// <param name="footerValue">The footer value to test with.</param>
        [Theory]
        [InlineData("Test Footer")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(42)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(true)]
        [InlineData(false)]
        public void Footer_WhenValueSet_ReturnsCorrectValue(object footerValue)
        {
            // Arrange
            var structuredItemsView = new StructuredItemsView();

            // Act
            structuredItemsView.Footer = footerValue;
            var result = structuredItemsView.Footer;

            // Assert
            Assert.Equal(footerValue, result);
        }

        /// <summary>
        /// Tests that the Footer property correctly handles null values,
        /// ensuring it can be set to null and retrieved as null.
        /// </summary>
        [Fact]
        public void Footer_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var structuredItemsView = new StructuredItemsView();
            structuredItemsView.Footer = "Initial Value";

            // Act
            structuredItemsView.Footer = null;
            var result = structuredItemsView.Footer;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Footer property can store and retrieve complex object types,
        /// verifying that reference equality is maintained for reference types.
        /// </summary>
        [Fact]
        public void Footer_WhenSetToComplexObject_ReturnsCorrectReference()
        {
            // Arrange
            var structuredItemsView = new StructuredItemsView();
            var complexObject = new TestFooterObject { Name = "Test", Value = 123 };

            // Act
            structuredItemsView.Footer = complexObject;
            var result = structuredItemsView.Footer;

            // Assert
            Assert.Same(complexObject, result);
        }

        /// <summary>
        /// Tests that multiple set operations on the Footer property work correctly,
        /// ensuring the last set value is always returned by the getter.
        /// </summary>
        [Fact]
        public void Footer_WhenSetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var structuredItemsView = new StructuredItemsView();

            // Act & Assert - Set and verify first value
            structuredItemsView.Footer = "First Value";
            Assert.Equal("First Value", structuredItemsView.Footer);

            // Act & Assert - Set and verify second value
            structuredItemsView.Footer = "Second Value";
            Assert.Equal("Second Value", structuredItemsView.Footer);

            // Act & Assert - Set and verify null value
            structuredItemsView.Footer = null;
            Assert.Null(structuredItemsView.Footer);
        }

        /// <summary>
        /// Helper class for testing complex object storage in Footer property.
        /// </summary>
        private class TestFooterObject
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        /// <summary>
        /// Tests that the Header property returns null as the default value when no value has been set.
        /// </summary>
        [Fact]
        public void Header_WhenNotSet_ReturnsNull()
        {
            // Arrange
            var structuredItemsView = new StructuredItemsView();

            // Act
            var result = structuredItemsView.Header;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Header property correctly stores and retrieves various object types and values.
        /// </summary>
        /// <param name="headerValue">The header value to set and retrieve.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Simple string")]
        [InlineData("   ")]
        [InlineData("String with special chars: !@#$%^&*()")]
        [InlineData(42)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(3.14)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(true)]
        [InlineData(false)]
        public void Header_SetAndGet_ReturnsExpectedValue(object headerValue)
        {
            // Arrange
            var structuredItemsView = new StructuredItemsView();

            // Act
            structuredItemsView.Header = headerValue;
            var result = structuredItemsView.Header;

            // Assert
            Assert.Equal(headerValue, result);
        }

        /// <summary>
        /// Tests that the Header property handles very long strings correctly.
        /// </summary>
        [Fact]
        public void Header_WithVeryLongString_ReturnsExpectedValue()
        {
            // Arrange
            var structuredItemsView = new StructuredItemsView();
            var longString = new string('A', 10000);

            // Act
            structuredItemsView.Header = longString;
            var result = structuredItemsView.Header;

            // Assert
            Assert.Equal(longString, result);
        }

        /// <summary>
        /// Tests that the Header property handles complex custom objects correctly.
        /// </summary>
        [Fact]
        public void Header_WithCustomObject_ReturnsExpectedValue()
        {
            // Arrange
            var structuredItemsView = new StructuredItemsView();
            var customObject = new CustomHeaderObject { Name = "Test", Value = 123 };

            // Act
            structuredItemsView.Header = customObject;
            var result = structuredItemsView.Header;

            // Assert
            Assert.Same(customObject, result);
        }

        /// <summary>
        /// Tests that setting the same Header value multiple times works correctly.
        /// </summary>
        [Fact]
        public void Header_SetSameValueMultipleTimes_ReturnsExpectedValue()
        {
            // Arrange
            var structuredItemsView = new StructuredItemsView();
            var headerValue = "Test Header";

            // Act
            structuredItemsView.Header = headerValue;
            structuredItemsView.Header = headerValue;
            structuredItemsView.Header = headerValue;
            var result = structuredItemsView.Header;

            // Assert
            Assert.Equal(headerValue, result);
        }

        /// <summary>
        /// Tests that the Header property handles string with control characters correctly.
        /// </summary>
        [Fact]
        public void Header_WithControlCharacters_ReturnsExpectedValue()
        {
            // Arrange
            var structuredItemsView = new StructuredItemsView();
            var stringWithControlChars = "Line1\nLine2\tTabbed\r\nWindows Line Ending";

            // Act
            structuredItemsView.Header = stringWithControlChars;
            var result = structuredItemsView.Header;

            // Assert
            Assert.Equal(stringWithControlChars, result);
        }

        /// <summary>
        /// Helper class for testing custom objects as header values.
        /// </summary>
        private class CustomHeaderObject
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }
    }
}