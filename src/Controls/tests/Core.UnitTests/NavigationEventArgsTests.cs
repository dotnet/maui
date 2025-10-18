#nullable disable

using System;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class NavigationEventArgsTests
    {
        /// <summary>
        /// Tests that the NavigationEventArgs constructor throws ArgumentNullException when page parameter is null.
        /// This test covers the null validation logic in the constructor.
        /// </summary>
        [Fact]
        public void Constructor_NullPage_ThrowsArgumentNullException()
        {
            // Arrange
            Page page = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new NavigationEventArgs(page));
            Assert.Equal("page", exception.ParamName);
        }

        /// <summary>
        /// Tests that the NavigationEventArgs constructor properly initializes the Page property when given a valid page.
        /// This test verifies that the constructor correctly assigns the provided page to the Page property.
        /// </summary>
        [Fact]
        public void Constructor_ValidPage_SetsPageProperty()
        {
            // Arrange
            var page = Substitute.For<Page>();

            // Act
            var navigationEventArgs = new NavigationEventArgs(page);

            // Assert
            Assert.Same(page, navigationEventArgs.Page);
        }
    }
}
