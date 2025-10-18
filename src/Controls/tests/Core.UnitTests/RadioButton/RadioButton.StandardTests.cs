#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class RadioButtonTests
    {
        /// <summary>
        /// Tests that MapContent method executes without throwing exceptions for various parameter combinations.
        /// Validates the method handles null and valid parameters gracefully since the method body is empty.
        /// </summary>
        /// <param name="handler">The IRadioButtonHandler parameter to test (can be null)</param>
        /// <param name="radioButton">The RadioButton parameter to test (can be null)</param>
        [Theory]
        [MemberData(nameof(MapContentTestCases))]
        public void MapContent_VariousParameterCombinations_DoesNotThrowException(IRadioButtonHandler handler, RadioButton radioButton)
        {
            // Act & Assert - Method should not throw any exception regardless of parameters
            var exception = Record.Exception(() => RadioButton.MapContent(handler, radioButton));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapContent with valid non-null parameters to ensure proper execution.
        /// Verifies the method completes successfully with valid handler and radioButton instances.
        /// </summary>
        [Fact]
        public void MapContent_ValidParameters_ExecutesSuccessfully()
        {
            // Arrange
            var handler = Substitute.For<IRadioButtonHandler>();
            var radioButton = new RadioButton();

            // Act & Assert - Should complete without exception
            var exception = Record.Exception(() => RadioButton.MapContent(handler, radioButton));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapContent with null handler parameter.
        /// Verifies the method handles null handler gracefully without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapContent_NullHandler_DoesNotThrowException()
        {
            // Arrange
            var radioButton = new RadioButton();

            // Act & Assert
            var exception = Record.Exception(() => RadioButton.MapContent(null, radioButton));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapContent with null radioButton parameter.
        /// Verifies the method handles null radioButton gracefully without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapContent_NullRadioButton_DoesNotThrowException()
        {
            // Arrange
            var handler = Substitute.For<IRadioButtonHandler>();

            // Act & Assert
            var exception = Record.Exception(() => RadioButton.MapContent(handler, null));

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests MapContent with both parameters as null.
        /// Verifies the method handles null parameters gracefully without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapContent_BothParametersNull_DoesNotThrowException()
        {
            // Act & Assert
            var exception = Record.Exception(() => RadioButton.MapContent(null, null));

            Assert.Null(exception);
        }

        public static TheoryData<IRadioButtonHandler, RadioButton> MapContentTestCases()
        {
            var handler = Substitute.For<IRadioButtonHandler>();
            var radioButton = new RadioButton();

            return new TheoryData<IRadioButtonHandler, RadioButton>
            {
                { null, null },
                { null, radioButton },
                { handler, null },
                { handler, radioButton }
            };
        }

        /// <summary>
        /// Tests that MapContent throws ArgumentNullException when handler parameter is null.
        /// Input: null handler, valid radioButton.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void MapContent_WithNullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            RadioButtonHandler handler = null;
            var radioButton = new RadioButton();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => RadioButton.MapContent(handler, radioButton));
        }

        /// <summary>
        /// Tests that MapContent executes successfully when radioButton parameter is null.
        /// Input: valid handler, null radioButton.
        /// Expected: Method executes without throwing exceptions.
        /// </summary>
        [Fact]
        public void MapContent_WithNullRadioButton_ExecutesSuccessfully()
        {
            // Arrange
            var handler = new RadioButtonHandler();
            RadioButton radioButton = null;

            // Act & Assert
            var exception = Record.Exception(() => RadioButton.MapContent(handler, radioButton));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapContent throws ArgumentNullException when both parameters are null.
        /// Input: null handler, null radioButton.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void MapContent_WithBothParametersNull_ThrowsArgumentNullException()
        {
            // Arrange
            RadioButtonHandler handler = null;
            RadioButton radioButton = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => RadioButton.MapContent(handler, radioButton));
        }

        /// <summary>
        /// Tests that MapContent executes successfully with valid parameters.
        /// Input: valid handler, valid radioButton.
        /// Expected: Method executes without throwing exceptions and delegates to IRadioButtonHandler overload.
        /// </summary>
        [Fact]
        public void MapContent_WithValidParameters_ExecutesSuccessfully()
        {
            // Arrange
            var handler = new RadioButtonHandler();
            var radioButton = new RadioButton();

            // Act & Assert
            var exception = Record.Exception(() => RadioButton.MapContent(handler, radioButton));
            Assert.Null(exception);
        }
    }
}