using System;

using Microsoft.Maui;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Platform.UnitTests
{
    public class StrokeExtensionsTests
    {
        /// <summary>
        /// Tests that UpdateStrokeShape does not throw exceptions when called with null platformView and null border.
        /// This verifies the method handles null parameters gracefully.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeShape_NullPlatformViewAndNullBorder_DoesNotThrow()
        {
            // Arrange
            object platformView = null;
            IBorderStroke border = null;

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeShape(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeShape does not throw exceptions when called with null platformView and valid border.
        /// This verifies the method handles null platformView gracefully.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeShape_NullPlatformViewAndValidBorder_DoesNotThrow()
        {
            // Arrange
            object platformView = null;
            IBorderStroke border = Substitute.For<IBorderStroke>();

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeShape(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeShape does not throw exceptions when called with valid platformView and null border.
        /// This verifies the method handles null border gracefully.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeShape_ValidPlatformViewAndNullBorder_DoesNotThrow()
        {
            // Arrange
            object platformView = new object();
            IBorderStroke border = null;

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeShape(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeShape does not throw exceptions when called with valid platformView and valid border.
        /// This verifies the method executes successfully with valid parameters.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeShape_ValidPlatformViewAndValidBorder_DoesNotThrow()
        {
            // Arrange
            object platformView = new object();
            IBorderStroke border = Substitute.For<IBorderStroke>();

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeShape(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeShape works with different types of platformView objects.
        /// This verifies the extension method works on various object types.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(true)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeShape_DifferentPlatformViewTypes_DoesNotThrow(object platformView)
        {
            // Arrange
            IBorderStroke border = Substitute.For<IBorderStroke>();

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeShape(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeShape completes execution successfully.
        /// This verifies the method runs to completion without hanging or infinite loops.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeShape_ValidParameters_CompletesExecution()
        {
            // Arrange
            object platformView = new object();
            IBorderStroke border = Substitute.For<IBorderStroke>();
            bool executionCompleted = false;

            // Act
            platformView.UpdateStrokeShape(border);
            executionCompleted = true;

            // Assert
            Assert.True(executionCompleted);
        }

        /// <summary>
        /// Tests that UpdateStrokeThickness completes successfully with valid parameters.
        /// Verifies the method can be called with a valid platform view and border without throwing exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeThickness_WithValidParameters_CompletesSuccessfully()
        {
            // Arrange
            var platformView = new object();
            var border = Substitute.For<IBorderStroke>();

            // Act & Assert - Should not throw
            platformView.UpdateStrokeThickness(border);
        }

        /// <summary>
        /// Tests that UpdateStrokeThickness handles null platform view without throwing exceptions.
        /// Since this is an extension method, it should be callable on null objects.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeThickness_WithNullPlatformView_CompletesSuccessfully()
        {
            // Arrange
            object platformView = null;
            var border = Substitute.For<IBorderStroke>();

            // Act & Assert - Should not throw
            platformView.UpdateStrokeThickness(border);
        }

        /// <summary>
        /// Tests that UpdateStrokeThickness handles null border parameter without throwing exceptions.
        /// Verifies the method's resilience to null border inputs.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeThickness_WithNullBorder_CompletesSuccessfully()
        {
            // Arrange
            var platformView = new object();
            IBorderStroke border = null;

            // Act & Assert - Should not throw
            platformView.UpdateStrokeThickness(border);
        }

        /// <summary>
        /// Tests that UpdateStrokeThickness works with various platform view types.
        /// Verifies the method accepts different object types as platform views.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(true)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeThickness_WithVariousPlatformViewTypes_CompletesSuccessfully(object platformView)
        {
            // Arrange
            var border = Substitute.For<IBorderStroke>();

            // Act & Assert - Should not throw
            platformView.UpdateStrokeThickness(border);
        }

        /// <summary>
        /// Tests that UpdateStrokeThickness handles both null parameters without throwing exceptions.
        /// Verifies the method's robustness when both parameters are null.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeThickness_WithBothParametersNull_CompletesSuccessfully()
        {
            // Arrange
            object platformView = null;
            IBorderStroke border = null;

            // Act & Assert - Should not throw
            platformView.UpdateStrokeThickness(border);
        }

        /// <summary>
        /// Tests that UpdateStrokeDashPattern executes without throwing an exception when called with valid parameters.
        /// This test validates the basic functionality with a mocked IBorderStroke instance.
        /// Expected result: Method completes without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeDashPattern_WithValidParameters_DoesNotThrow()
        {
            // Arrange
            var platformView = new object();
            var borderStroke = Substitute.For<IBorderStroke>();

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeDashPattern(borderStroke));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeDashPattern executes without throwing an exception when called with a null border parameter.
        /// This test validates the method's behavior when the border parameter is null.
        /// Expected result: Method completes without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeDashPattern_WithNullBorder_DoesNotThrow()
        {
            // Arrange
            var platformView = new object();
            IBorderStroke border = null;

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeDashPattern(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeDashPattern executes without throwing an exception when called with different types of platform view objects.
        /// This test validates the method works with various object types as the extension target.
        /// Expected result: Method completes without throwing any exceptions for all object types.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(DateTime))]
        [Trait("Category", "auto-generated")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        public void UpdateStrokeDashPattern_WithDifferentPlatformViewTypes_DoesNotThrow(Type platformViewType)
        {
            // Arrange
            object platformView;
            if (platformViewType == typeof(string))
                platformView = "test";
            else if (platformViewType == typeof(int))
                platformView = 42;
            else if (platformViewType == typeof(DateTime))
                platformView = DateTime.Now;
            else
                platformView = Activator.CreateInstance(platformViewType);
            
            var borderStroke = Substitute.For<IBorderStroke>();

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeDashPattern(borderStroke));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeDashPattern executes without throwing an exception when called with both null platform view and null border.
        /// This test validates the method's behavior with multiple null parameters.
        /// Expected result: Method completes without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeDashPattern_WithBothParametersNull_DoesNotThrow()
        {
            // Arrange
            object platformView = null;
            IBorderStroke border = null;

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeDashPattern(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeDashOffset executes successfully with valid parameters.
        /// Verifies the method can be called on a non-null platform view with a valid border stroke.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeDashOffset_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var platformView = new object();
            var border = Substitute.For<IBorderStroke>();

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeDashOffset(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeDashOffset executes successfully when called on a null platform view.
        /// Verifies the extension method can handle null receivers.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeDashOffset_NullPlatformView_DoesNotThrow()
        {
            // Arrange
            object platformView = null;
            var border = Substitute.For<IBorderStroke>();

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeDashOffset(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests UpdateStrokeDashOffset with various platform view types.
        /// Verifies the method works with different object types including strings, arrays, and custom objects.
        /// Expected result: No exception is thrown for any object type.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(true)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeDashOffset_VariousPlatformViewTypes_DoesNotThrow(object platformView)
        {
            // Arrange
            var border = Substitute.For<IBorderStroke>();

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeDashOffset(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests UpdateStrokeDashOffset with complex platform view objects.
        /// Verifies the method works with arrays and collections as platform views.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeDashOffset_ComplexPlatformViewObjects_DoesNotThrow()
        {
            // Arrange
            var border = Substitute.For<IBorderStroke>();
            var arrayPlatformView = new int[] { 1, 2, 3 };
            var customObject = new { Name = "Test", Value = 123 };

            // Act & Assert
            var arrayException = Record.Exception(() => arrayPlatformView.UpdateStrokeDashOffset(border));
            Assert.Null(arrayException);

            var customException = Record.Exception(() => customObject.UpdateStrokeDashOffset(border));
            Assert.Null(customException);
        }

        /// <summary>
        /// Tests that UpdateStrokeMiterLimit can be called with valid parameters without throwing exceptions.
        /// This test validates the basic functionality when both platformView and border are non-null.
        /// Expected result: Method executes successfully without throwing.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeMiterLimit_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var platformView = new object();
            var border = Substitute.For<IBorderStroke>();

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeMiterLimit(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeMiterLimit can be called with null platformView without throwing exceptions.
        /// This test validates that extension methods can be called on null instances when the method body is empty.
        /// Expected result: Method executes successfully without throwing.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeMiterLimit_NullPlatformView_DoesNotThrow()
        {
            // Arrange
            object platformView = null;
            var border = Substitute.For<IBorderStroke>();

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeMiterLimit(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeMiterLimit can be called with null border parameter without throwing exceptions.
        /// This test validates that the method handles null border parameters gracefully since the method body is empty.
        /// Expected result: Method executes successfully without throwing.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeMiterLimit_NullBorder_DoesNotThrow()
        {
            // Arrange
            var platformView = new object();
            IBorderStroke border = null;

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeMiterLimit(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeMiterLimit can be called with both parameters as null without throwing exceptions.
        /// This test validates the method's robustness when called with all null parameters.
        /// Expected result: Method executes successfully without throwing.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeMiterLimit_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            object platformView = null;
            IBorderStroke border = null;

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeMiterLimit(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests UpdateStrokeLineCap extension method with various parameter combinations.
        /// Verifies that the method executes without throwing exceptions regardless of input values,
        /// including null parameters and different object types.
        /// </summary>
        /// <param name="platformView">The platform view object to test with</param>
        /// <param name="borderIsNull">Whether to pass null for the border parameter</param>
        [Theory]
        [InlineData(null, true)]
        [InlineData(null, false)]
        [InlineData("testObject", true)]
        [InlineData("testObject", false)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeLineCap_WithVariousParameters_DoesNotThrow(object platformView, bool borderIsNull)
        {
            // Arrange
            IBorderStroke border = borderIsNull ? null : Substitute.For<IBorderStroke>();

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeLineCap(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests UpdateStrokeLineCap extension method with different object types for platformView.
        /// Verifies that the method handles various object types without throwing exceptions.
        /// </summary>
        /// <param name="platformViewType">The type of object to create for platformView</param>
        [Theory]
        [InlineData("string")]
        [InlineData("int")]
        [InlineData("object")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeLineCap_WithDifferentObjectTypes_DoesNotThrow(string platformViewType)
        {
            // Arrange
            object platformView = platformViewType switch
            {
                "string" => "test string",
                "int" => 42,
                "object" => new object(),
                _ => throw new ArgumentException("Invalid platform view type")
            };
            IBorderStroke border = Substitute.For<IBorderStroke>();

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeLineCap(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeLineJoin executes without throwing exceptions when both parameters are valid.
        /// Input: Valid object and mocked IBorderStroke.
        /// Expected: Method executes successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeLineJoin_ValidParameters_ExecutesWithoutException()
        {
            // Arrange
            var platformView = new object();
            var border = Substitute.For<IBorderStroke>();

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeLineJoin(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeLineJoin executes without throwing exceptions when platformView is null.
        /// Input: Null platformView and valid IBorderStroke.
        /// Expected: Method executes successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeLineJoin_NullPlatformView_ExecutesWithoutException()
        {
            // Arrange
            object platformView = null;
            var border = Substitute.For<IBorderStroke>();

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeLineJoin(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeLineJoin executes without throwing exceptions when border is null.
        /// Input: Valid object and null IBorderStroke.
        /// Expected: Method executes successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeLineJoin_NullBorder_ExecutesWithoutException()
        {
            // Arrange
            var platformView = new object();
            IBorderStroke border = null;

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeLineJoin(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeLineJoin executes without throwing exceptions when both parameters are null.
        /// Input: Null platformView and null IBorderStroke.
        /// Expected: Method executes successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeLineJoin_BothParametersNull_ExecutesWithoutException()
        {
            // Arrange
            object platformView = null;
            IBorderStroke border = null;

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeLineJoin(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStrokeLineJoin executes without throwing exceptions with different object types for platformView.
        /// Input: Various object types for platformView and valid IBorderStroke.
        /// Expected: Method executes successfully without throwing any exceptions regardless of object type.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStrokeLineJoin_DifferentPlatformViewTypes_ExecutesWithoutException(object platformView)
        {
            // Arrange
            var border = Substitute.For<IBorderStroke>();

            // Act & Assert
            var exception = Record.Exception(() => platformView.UpdateStrokeLineJoin(border));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UpdateStroke can be called with valid non-null parameters without throwing an exception.
        /// Input: Valid object and mocked IBorderStroke.
        /// Expected: Method completes successfully without throwing.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStroke_ValidParameters_CompletesSuccessfully()
        {
            // Arrange
            var platformView = new object();
            var border = Substitute.For<IBorderStroke>();

            // Act & Assert
            platformView.UpdateStroke(border);
        }

        /// <summary>
        /// Tests that UpdateStroke can be called with null platformView without throwing an exception.
        /// Input: null platformView and mocked IBorderStroke.
        /// Expected: Method completes successfully without throwing.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStroke_NullPlatformView_CompletesSuccessfully()
        {
            // Arrange
            object platformView = null;
            var border = Substitute.For<IBorderStroke>();

            // Act & Assert
            platformView.UpdateStroke(border);
        }

        /// <summary>
        /// Tests that UpdateStroke can be called with null border without throwing an exception.
        /// Input: Valid object and null IBorderStroke.
        /// Expected: Method completes successfully without throwing.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStroke_NullBorder_CompletesSuccessfully()
        {
            // Arrange
            var platformView = new object();
            IBorderStroke border = null;

            // Act & Assert
            platformView.UpdateStroke(border);
        }

        /// <summary>
        /// Tests that UpdateStroke can be called with both null parameters without throwing an exception.
        /// Input: null platformView and null IBorderStroke.
        /// Expected: Method completes successfully without throwing.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStroke_BothParametersNull_CompletesSuccessfully()
        {
            // Arrange
            object platformView = null;
            IBorderStroke border = null;

            // Act & Assert
            platformView.UpdateStroke(border);
        }

        /// <summary>
        /// Tests that UpdateStroke can be called with different object types for platformView.
        /// Input: Various object types and mocked IBorderStroke.
        /// Expected: Method completes successfully for all object types without throwing.
        /// </summary>
        [Theory]
        [InlineData("string object")]
        [InlineData(42)]
        [InlineData(true)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void UpdateStroke_DifferentPlatformViewTypes_CompletesSuccessfully(object platformView)
        {
            // Arrange
            var border = Substitute.For<IBorderStroke>();

            // Act & Assert
            platformView.UpdateStroke(border);
        }
    }
}