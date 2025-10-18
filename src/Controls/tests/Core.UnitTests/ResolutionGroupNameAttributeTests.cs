#nullable disable

using System;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ResolutionGroupNameAttributeTests
    {
        /// <summary>
        /// Tests that the ResolutionGroupNameAttribute constructor correctly initializes the ShortName property
        /// with various string inputs including null, empty, whitespace, and valid strings.
        /// </summary>
        /// <param name="name">The name parameter to pass to the constructor</param>
        /// <param name="expectedShortName">The expected value of the ShortName property</param>
        [Theory]
        [InlineData("ValidName", "ValidName")]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("   ", "   ")]
        [InlineData("Name with spaces", "Name with spaces")]
        [InlineData("Special!@#$%^&*()Characters", "Special!@#$%^&*()Characters")]
        [InlineData("VeryLongStringThatExceedsTypicalLengthLimitsToTestBoundaryConditionsAndEnsureTheAttributeCanHandleLargeInputsWithoutIssuesOrExceptions", "VeryLongStringThatExceedsTypicalLengthLimitsToTestBoundaryConditionsAndEnsureTheAttributeCanHandleLargeInputsWithoutIssuesOrExceptions")]
        [InlineData("\t\n\r", "\t\n\r")]
        [InlineData("Unicode测试", "Unicode测试")]
        public void Constructor_WithVariousStringInputs_SetsShortNameCorrectly(string name, string expectedShortName)
        {
            // Arrange & Act
            var attribute = new ResolutionGroupNameAttribute(name);

            // Assert
            Assert.Equal(expectedShortName, attribute.ShortName);
        }

        /// <summary>
        /// Tests that the ResolutionGroupNameAttribute constructor creates a valid attribute instance
        /// that inherits from System.Attribute as expected.
        /// </summary>
        [Fact]
        public void Constructor_WithValidName_CreatesValidAttributeInstance()
        {
            // Arrange
            const string testName = "TestGroupName";

            // Act
            var attribute = new ResolutionGroupNameAttribute(testName);

            // Assert
            Assert.NotNull(attribute);
            Assert.IsAssignableFrom<Attribute>(attribute);
            Assert.Equal(testName, attribute.ShortName);
        }
    }
}
