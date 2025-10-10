using System;

using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Handlers.UnitTests
{
    public partial class ProgressBarHandlerTests
    {
        /// <summary>
        /// Tests that MapProgress does not throw an exception when called with valid handler and progress instances.
        /// This verifies the method can be invoked successfully with proper input parameters.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapProgress_ValidInputs_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IProgressBarHandler>();
            var progress = Substitute.For<IProgress>();

            // Act & Assert
            var exception = Record.Exception(() => ProgressBarHandler.MapProgress(handler, progress));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapProgress does not throw an exception when called with a null handler parameter.
        /// This verifies the method handles null handler gracefully in the Standard implementation.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapProgress_NullHandler_DoesNotThrow()
        {
            // Arrange
            IProgressBarHandler handler = null;
            var progress = Substitute.For<IProgress>();

            // Act & Assert
            var exception = Record.Exception(() => ProgressBarHandler.MapProgress(handler, progress));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapProgress does not throw an exception when called with a null progress parameter.
        /// This verifies the method handles null progress gracefully in the Standard implementation.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapProgress_NullProgress_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IProgressBarHandler>();
            IProgress progress = null;

            // Act & Assert
            var exception = Record.Exception(() => ProgressBarHandler.MapProgress(handler, progress));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapProgress does not throw an exception when called with both null parameters.
        /// This verifies the method handles completely null input gracefully in the Standard implementation.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapProgress_BothParametersNull_DoesNotThrow()
        {
            // Arrange
            IProgressBarHandler handler = null;
            IProgress progress = null;

            // Act & Assert
            var exception = Record.Exception(() => ProgressBarHandler.MapProgress(handler, progress));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapProgress can be called multiple times without side effects.
        /// This verifies the method is idempotent and safe to call repeatedly.
        /// Expected result: No exceptions are thrown on multiple invocations.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapProgress_MultipleInvocations_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IProgressBarHandler>();
            var progress = Substitute.For<IProgress>();

            // Act & Assert
            var exception1 = Record.Exception(() => ProgressBarHandler.MapProgress(handler, progress));
            var exception2 = Record.Exception(() => ProgressBarHandler.MapProgress(handler, progress));
            var exception3 = Record.Exception(() => ProgressBarHandler.MapProgress(handler, progress));

            Assert.Null(exception1);
            Assert.Null(exception2);
            Assert.Null(exception3);
        }

        /// <summary>
        /// Tests that MapProgressColor method executes without throwing when called with valid handler and progress parameters.
        /// This test verifies the method's basic functionality with non-null inputs.
        /// Expected result: Method executes successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapProgressColor_ValidParameters_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IProgressBarHandler>();
            var progress = Substitute.For<IProgress>();

            // Act & Assert
            var exception = Record.Exception(() => ProgressBarHandler.MapProgressColor(handler, progress));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapProgressColor method does not throw when called with null handler parameter.
        /// This test verifies the method handles null handler gracefully since it has no implementation.
        /// Expected result: Method executes successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapProgressColor_NullHandler_DoesNotThrow()
        {
            // Arrange
            var progress = Substitute.For<IProgress>();

            // Act & Assert
            var exception = Record.Exception(() => ProgressBarHandler.MapProgressColor(null, progress));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapProgressColor method does not throw when called with null progress parameter.
        /// This test verifies the method handles null progress gracefully since it has no implementation.
        /// Expected result: Method executes successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapProgressColor_NullProgress_DoesNotThrow()
        {
            // Arrange
            var handler = Substitute.For<IProgressBarHandler>();

            // Act & Assert
            var exception = Record.Exception(() => ProgressBarHandler.MapProgressColor(handler, null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MapProgressColor method does not throw when called with both null parameters.
        /// This test verifies the method handles completely null inputs gracefully since it has no implementation.
        /// Expected result: Method executes successfully without throwing any exceptions.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void MapProgressColor_BothParametersNull_DoesNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => ProgressBarHandler.MapProgressColor(null, null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that CreatePlatformView method throws NotImplementedException when called.
        /// This verifies that the Standard implementation correctly throws an exception
        /// since platform views are not supported in the standard/fallback implementation.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void CreatePlatformView_Always_ThrowsNotImplementedException()
        {
            // Arrange
            var handler = new TestableProgressBarHandler();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => handler.CreatePlatformView());
        }

        /// <summary>
        /// Helper class to expose the protected CreatePlatformView method for testing.
        /// </summary>
        private class TestableProgressBarHandler : ProgressBarHandler
        {
            public new object CreatePlatformView()
            {
                return base.CreatePlatformView();
            }
        }
    }
}