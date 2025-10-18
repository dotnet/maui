#nullable disable

using System;
using System.Xml;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class XmlLineInfoTests
    {
        /// <summary>
        /// Tests that HasLineInfo returns false when XmlLineInfo is created using the default constructor.
        /// The default constructor does not set the _hasLineInfo field, so it remains false.
        /// </summary>
        [Fact]
        public void HasLineInfo_DefaultConstructor_ReturnsFalse()
        {
            // Arrange
            var xmlLineInfo = new XmlLineInfo();

            // Act
            var result = xmlLineInfo.HasLineInfo();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasLineInfo returns true when XmlLineInfo is created using the parameterized constructor.
        /// The parameterized constructor sets _hasLineInfo to true regardless of the line number and position values.
        /// </summary>
        [Theory]
        [InlineData(1, 1)]
        [InlineData(0, 0)]
        [InlineData(-1, -1)]
        [InlineData(int.MaxValue, int.MaxValue)]
        [InlineData(int.MinValue, int.MinValue)]
        [InlineData(100, 50)]
        [InlineData(-100, 200)]
        public void HasLineInfo_ParameterizedConstructor_ReturnsTrue(int lineNumber, int linePosition)
        {
            // Arrange
            var xmlLineInfo = new XmlLineInfo(lineNumber, linePosition);

            // Act
            var result = xmlLineInfo.HasLineInfo();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the default constructor creates an XmlLineInfo instance with no line information.
        /// Verifies that HasLineInfo returns false and properties have default values.
        /// </summary>
        [Fact]
        public void Constructor_Default_CreatesInstanceWithNoLineInfo()
        {
            // Arrange & Act
            var xmlLineInfo = new XmlLineInfo();

            // Assert
            Assert.NotNull(xmlLineInfo);
            Assert.False(xmlLineInfo.HasLineInfo());
            Assert.Equal(0, xmlLineInfo.LineNumber);
            Assert.Equal(0, xmlLineInfo.LinePosition);
        }

        /// <summary>
        /// Tests that the default constructor creates an object that properly implements IXmlLineInfo interface.
        /// Verifies interface compliance and expected behavior.
        /// </summary>
        [Fact]
        public void Constructor_Default_ImplementsIXmlLineInfoInterface()
        {
            // Arrange & Act
            var xmlLineInfo = new XmlLineInfo();
            IXmlLineInfo interfaceReference = xmlLineInfo;

            // Assert
            Assert.NotNull(interfaceReference);
            Assert.False(interfaceReference.HasLineInfo());
            Assert.Equal(0, interfaceReference.LineNumber);
            Assert.Equal(0, interfaceReference.LinePosition);
        }
    }
}