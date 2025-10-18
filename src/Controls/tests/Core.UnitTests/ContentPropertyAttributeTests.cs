#nullable disable
//
// ContentPropertyAttributeTests.cs
//
// Author:
//       Stephane Delcroix <stephane@delcroix.org>
//
// Copyright (c) 2013 S. Delcroix
//

using Microsoft.Maui.Controls;
using System;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ContentPropertyAttributeTests
    {
        /// <summary>
        /// Tests that the ContentPropertyAttribute constructor properly sets the Name property with various string inputs.
        /// Verifies that the Name property returns exactly what was passed to the constructor.
        /// </summary>
        /// <param name="name">The name parameter to pass to the constructor</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        [InlineData("Content")]
        [InlineData("Children")]
        [InlineData("MyProperty")]
        [InlineData("Property123")]
        [InlineData("Property_With_Underscores")]
        [InlineData("Property-With-Dashes")]
        [InlineData("Property.With.Dots")]
        [InlineData("Property With Spaces")]
        [InlineData("PropertyWithSpecialChars!@#$%^&*()")]
        [InlineData("PropertyWithUnicode中文字符")]
        [InlineData("\0PropertyWithNullChar")]
        [InlineData("VeryLongPropertyNameThatExceedsTypicalLengthLimitsToTestBoundaryConditionsAndEnsureTheAttributeCanHandleExtremelyLongStringInputsWithoutAnyIssuesOrUnexpectedBehavior")]
        public void Constructor_WithVariousStringInputs_SetsNamePropertyCorrectly(string name)
        {
            // Arrange & Act
            var attribute = new ContentPropertyAttribute(name);

            // Assert
            Assert.Equal(name, attribute.Name);
        }

        /// <summary>
        /// Tests that the ContentPropertyAttribute constructor with a typical property name creates a valid attribute.
        /// Verifies basic functionality with a common use case.
        /// </summary>
        [Fact]
        public void Constructor_WithTypicalPropertyName_CreatesValidAttribute()
        {
            // Arrange
            const string expectedName = "Content";

            // Act
            var attribute = new ContentPropertyAttribute(expectedName);

            // Assert
            Assert.Equal(expectedName, attribute.Name);
            Assert.IsAssignableFrom<Attribute>(attribute);
        }

        /// <summary>
        /// Tests that the Name property is read-only and maintains its value after construction.
        /// Verifies that the property getter consistently returns the same value.
        /// </summary>
        [Fact]
        public void Name_Property_IsReadOnlyAndConsistent()
        {
            // Arrange
            const string expectedName = "TestProperty";
            var attribute = new ContentPropertyAttribute(expectedName);

            // Act
            var firstRead = attribute.Name;
            var secondRead = attribute.Name;

            // Assert
            Assert.Equal(expectedName, firstRead);
            Assert.Equal(expectedName, secondRead);
            Assert.Same(firstRead, secondRead); // Should be the same reference
        }
    }
}
