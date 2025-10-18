#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Tests for the PoppedToRootEventArgs class.
    /// </summary>
    public partial class PoppedToRootEventArgsTests
    {
        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when poppedPages parameter is null.
        /// This test covers the null check validation that throws an exception for invalid input.
        /// Expected result: ArgumentNullException with parameter name "poppedPages".
        /// </summary>
        [Fact]
        public void Constructor_NullPoppedPages_ThrowsArgumentNullException()
        {
            // Arrange
            var page = Substitute.For<Page>();
            IEnumerable<Page> poppedPages = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new PoppedToRootEventArgs(page, poppedPages));
            Assert.Equal("poppedPages", exception.ParamName);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when poppedPages parameter is null, even with null page.
        /// This test verifies that the poppedPages validation occurs regardless of the page parameter value.
        /// Expected result: ArgumentNullException with parameter name "poppedPages".
        /// </summary>
        [Fact]
        public void Constructor_NullPageAndNullPoppedPages_ThrowsArgumentNullException()
        {
            // Arrange
            Page page = null;
            IEnumerable<Page> poppedPages = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new PoppedToRootEventArgs(page, poppedPages));
            Assert.Equal("poppedPages", exception.ParamName);
        }

        /// <summary>
        /// Tests that the constructor succeeds with a valid page and empty poppedPages collection.
        /// This test verifies that an empty collection is valid input and the PoppedPages property is set correctly.
        /// Expected result: Successful construction with PoppedPages property containing an empty collection.
        /// </summary>
        [Fact]
        public void Constructor_ValidPageAndEmptyPoppedPages_SetsPropertiesCorrectly()
        {
            // Arrange
            var page = Substitute.For<Page>();
            var poppedPages = new Page[0];

            // Act
            var eventArgs = new PoppedToRootEventArgs(page, poppedPages);

            // Assert
            Assert.NotNull(eventArgs.PoppedPages);
            Assert.Same(poppedPages, eventArgs.PoppedPages);
            Assert.Empty(eventArgs.PoppedPages);
        }

        /// <summary>
        /// Tests that the constructor succeeds with a valid page and a collection containing multiple pages.
        /// This test verifies that the constructor correctly handles multiple pages and preserves the collection reference.
        /// Expected result: Successful construction with PoppedPages property containing the provided pages.
        /// </summary>
        [Fact]
        public void Constructor_ValidPageAndMultiplePoppedPages_SetsPropertiesCorrectly()
        {
            // Arrange
            var page = Substitute.For<Page>();
            var page1 = Substitute.For<Page>();
            var page2 = Substitute.For<Page>();
            var page3 = Substitute.For<Page>();
            var poppedPages = new[] { page1, page2, page3 };

            // Act
            var eventArgs = new PoppedToRootEventArgs(page, poppedPages);

            // Assert
            Assert.NotNull(eventArgs.PoppedPages);
            Assert.Same(poppedPages, eventArgs.PoppedPages);
            Assert.Equal(3, eventArgs.PoppedPages.Count());
            Assert.Contains(page1, eventArgs.PoppedPages);
            Assert.Contains(page2, eventArgs.PoppedPages);
            Assert.Contains(page3, eventArgs.PoppedPages);
        }

        /// <summary>
        /// Tests that the constructor succeeds with a null page and valid poppedPages collection.
        /// This test verifies the behavior when the page parameter is null but poppedPages is valid.
        /// Expected result: Successful construction (behavior depends on base class NavigationEventArgs).
        /// </summary>
        [Fact]
        public void Constructor_NullPageAndValidPoppedPages_SetsPropertiesCorrectly()
        {
            // Arrange
            Page page = null;
            var poppedPages = new[] { Substitute.For<Page>() };

            // Act
            var eventArgs = new PoppedToRootEventArgs(page, poppedPages);

            // Assert
            Assert.NotNull(eventArgs.PoppedPages);
            Assert.Same(poppedPages, eventArgs.PoppedPages);
            Assert.Single(eventArgs.PoppedPages);
        }

        /// <summary>
        /// Tests that the constructor succeeds with a single page in the poppedPages collection.
        /// This test verifies that the constructor correctly handles a collection with one element.
        /// Expected result: Successful construction with PoppedPages property containing the single page.
        /// </summary>
        [Fact]
        public void Constructor_ValidPageAndSinglePoppedPage_SetsPropertiesCorrectly()
        {
            // Arrange
            var page = Substitute.For<Page>();
            var poppedPage = Substitute.For<Page>();
            var poppedPages = new[] { poppedPage };

            // Act
            var eventArgs = new PoppedToRootEventArgs(page, poppedPages);

            // Assert
            Assert.NotNull(eventArgs.PoppedPages);
            Assert.Same(poppedPages, eventArgs.PoppedPages);
            Assert.Single(eventArgs.PoppedPages);
            Assert.Same(poppedPage, eventArgs.PoppedPages.First());
        }

        /// <summary>
        /// Tests that the constructor succeeds when poppedPages collection contains null elements.
        /// This test verifies that the constructor does not validate individual elements within the collection.
        /// Expected result: Successful construction with PoppedPages property containing the collection with null elements.
        /// </summary>
        [Fact]
        public void Constructor_ValidPageAndPoppedPagesWithNullElements_SetsPropertiesCorrectly()
        {
            // Arrange
            var page = Substitute.For<Page>();
            var validPage = Substitute.For<Page>();
            var poppedPages = new Page[] { validPage, null, validPage };

            // Act
            var eventArgs = new PoppedToRootEventArgs(page, poppedPages);

            // Assert
            Assert.NotNull(eventArgs.PoppedPages);
            Assert.Same(poppedPages, eventArgs.PoppedPages);
            Assert.Equal(3, eventArgs.PoppedPages.Count());
            Assert.Contains(validPage, eventArgs.PoppedPages);
            Assert.Contains(null, eventArgs.PoppedPages);
        }
    }
}
