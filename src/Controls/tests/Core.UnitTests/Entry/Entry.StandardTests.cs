#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class EntryTests
    {
        /// <summary>
        /// Tests that MapText method with IEntryHandler parameter executes without throwing exceptions when both parameters are null.
        /// Verifies the method handles null inputs gracefully.
        /// Expected result: No exception should be thrown.
        /// </summary>
        [Fact]
        public void MapText_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            IEntryHandler handler = null;
            Entry entry = null;

            // Act & Assert
            Entry.MapText(handler, entry);
        }

        /// <summary>
        /// Tests that MapText method with IEntryHandler parameter executes without throwing exceptions when handler is null and entry is not null.
        /// Verifies the method handles null handler input gracefully.
        /// Expected result: No exception should be thrown.
        /// </summary>
        [Fact]
        public void MapText_HandlerNullEntryNotNull_DoesNotThrow()
        {
            // Arrange
            IEntryHandler handler = null;
            Entry entry = new Entry();

            // Act & Assert
            Entry.MapText(handler, entry);
        }

        /// <summary>
        /// Tests that MapText method with IEntryHandler parameter executes without throwing exceptions when handler is not null and entry is null.
        /// Verifies the method handles null entry input gracefully.
        /// Expected result: No exception should be thrown.
        /// </summary>
        [Fact]
        public void MapText_HandlerNotNullEntryNull_DoesNotThrow()
        {
            // Arrange
            IEntryHandler handler = Substitute.For<IEntryHandler>();
            Entry entry = null;

            // Act & Assert
            Entry.MapText(handler, entry);
        }

        /// <summary>
        /// Tests that MapText method with IEntryHandler parameter executes without throwing exceptions when both parameters are not null.
        /// Verifies the method handles valid inputs gracefully.
        /// Expected result: No exception should be thrown.
        /// </summary>
        [Fact]
        public void MapText_BothParametersNotNull_DoesNotThrow()
        {
            // Arrange
            IEntryHandler handler = Substitute.For<IEntryHandler>();
            Entry entry = new Entry();

            // Act & Assert
            Entry.MapText(handler, entry);
        }

        /// <summary>
        /// Tests that MapText method with IEntryHandler parameter executes without throwing exceptions with various parameter combinations.
        /// Verifies the method handles all possible null/non-null combinations gracefully.
        /// Expected result: No exception should be thrown for any combination.
        /// </summary>
        [Theory]
        [InlineData(true, true)]   // Both not null
        [InlineData(true, false)]  // Handler not null, entry null
        [InlineData(false, true)]  // Handler null, entry not null
        [InlineData(false, false)] // Both null
        public void MapText_VariousParameterCombinations_DoesNotThrow(bool handlerNotNull, bool entryNotNull)
        {
            // Arrange
            IEntryHandler handler = handlerNotNull ? Substitute.For<IEntryHandler>() : null;
            Entry entry = entryNotNull ? new Entry() : null;

            // Act & Assert
            Entry.MapText(handler, entry);
        }

        /// <summary>
        /// Tests that MapText method executes successfully without throwing exceptions when provided with valid non-null parameters.
        /// </summary>
        [Fact]
        public void MapText_WithValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = new EntryHandler();
            var entry = new Entry();

            // Act & Assert
            var exception = Record.Exception(() => Entry.MapText(handler, entry));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method handles null handler parameter without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapText_WithNullHandler_DoesNotThrow()
        {
            // Arrange
            EntryHandler handler = null;
            var entry = new Entry();

            // Act & Assert
            var exception = Record.Exception(() => Entry.MapText(handler, entry));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method handles null entry parameter without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapText_WithNullEntry_DoesNotThrow()
        {
            // Arrange
            var handler = new EntryHandler();
            Entry entry = null;

            // Act & Assert
            var exception = Record.Exception(() => Entry.MapText(handler, entry));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapText method handles both null parameters without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapText_WithBothParametersNull_DoesNotThrow()
        {
            // Arrange
            EntryHandler handler = null;
            Entry entry = null;

            // Act & Assert
            var exception = Record.Exception(() => Entry.MapText(handler, entry));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapText method with different combinations of null and non-null parameters.
        /// The method should not throw exceptions regardless of parameter values since it has an empty implementation.
        /// </summary>
        [Theory]
        [InlineData(true, true)]   // Both non-null
        [InlineData(true, false)]  // Handler non-null, entry null
        [InlineData(false, true)]  // Handler null, entry non-null
        [InlineData(false, false)] // Both null
        public void MapText_ParameterCombinations_DoesNotThrow(bool handlerNotNull, bool entryNotNull)
        {
            // Arrange
            var handler = handlerNotNull ? new EntryHandler() : null;
            var entry = entryNotNull ? new Entry() : null;

            // Act & Assert
            var exception = Record.Exception(() => Entry.MapText(handler, entry));
            Assert.Null(exception);
        }
    }
}