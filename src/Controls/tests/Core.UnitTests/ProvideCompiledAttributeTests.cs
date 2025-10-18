#nullable disable

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ProvideCompiledAttributeTests
    {
        /// <summary>
        /// Tests that the constructor properly assigns the compiledVersion parameter to the CompiledVersion property
        /// for various valid string inputs including null, empty, whitespace, normal strings, and edge cases.
        /// </summary>
        /// <param name="compiledVersion">The compiled version string to test</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Microsoft.Maui.Controls.Button")]
        [InlineData("My.Namespace.MyClass")]
        [InlineData("a")]
        [InlineData("VeryLongClassNameThatExceedsTypicalLengthLimitsButShouldStillBeValidForTestingPurposesAndEdgeCases")]
        [InlineData("Class.With.Special!@#$%^&*()Characters")]
        [InlineData("ClassWith\nNewLine")]
        [InlineData("ClassWith\tTab")]
        public void Constructor_WithVariousStrings_AssignsToCompiledVersionProperty(string compiledVersion)
        {
            // Arrange & Act
            var attribute = new ProvideCompiledAttribute(compiledVersion);

            // Assert
            Assert.Equal(compiledVersion, attribute.CompiledVersion);
        }
    }
}
