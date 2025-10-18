#nullable disable

using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class PreserveAttributeTests
    {
        /// <summary>
        /// Tests the PreserveAttribute constructor with boolean parameters.
        /// Verifies that the constructor properly assigns the allMembers and conditional parameters
        /// to their corresponding AllMembers and Conditional fields respectively.
        /// </summary>
        /// <param name="allMembers">The value for the AllMembers field</param>
        /// <param name="conditional">The value for the Conditional field</param>
        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void Constructor_WithBooleanParameters_SetsFieldsCorrectly(bool allMembers, bool conditional)
        {
            // Arrange & Act
            var attribute = new PreserveAttribute(allMembers, conditional);

            // Assert
            Assert.Equal(allMembers, attribute.AllMembers);
            Assert.Equal(conditional, attribute.Conditional);
        }

        /// <summary>
        /// Tests that the PreserveAttribute constructor creates a valid attribute instance.
        /// Verifies that the constructed object is not null and is of the expected type.
        /// </summary>
        [Fact]
        public void Constructor_WithBooleanParameters_CreatesValidInstance()
        {
            // Arrange & Act
            var attribute = new PreserveAttribute(true, false);

            // Assert
            Assert.NotNull(attribute);
            Assert.IsType<PreserveAttribute>(attribute);
            Assert.IsAssignableFrom<Attribute>(attribute);
        }
    }
}
