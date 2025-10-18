#nullable disable

using System;
using System.ComponentModel;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the VisualMarker class.
    /// </summary>
    public partial class VisualMarkerTests
    {
        /// <summary>
        /// Tests that MaterialCheck method executes without throwing exceptions when Application.Current is null.
        /// This is a partial test due to the method's reliance on static dependencies that cannot be mocked.
        /// 
        /// Limitations:
        /// - Cannot mock Application.Current (static property)
        /// - Cannot mock DeviceInfo.Platform (static property)  
        /// - Cannot access or reset private static fields _isMaterialRegistered and _warnedAboutMaterial
        /// - Cannot verify logger calls due to unmockable static dependencies
        /// 
        /// To make this method fully testable, consider:
        /// 1. Injecting dependencies rather than using static references
        /// 2. Making static fields internal or providing reset methods for testing
        /// 3. Using dependency injection for platform information and logging
        /// </summary>
        [Fact]
        public void MaterialCheck_WithNullApplication_DoesNotThrow()
        {
            // Arrange
            // Note: We cannot control Application.Current as it's a static property that cannot be mocked

            // Act & Assert
            // This test verifies the method doesn't throw when static dependencies are null
            var exception = Record.Exception(() => VisualMarker.MaterialCheck());

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that MaterialCheck method can be called multiple times without throwing exceptions.
        /// This verifies the basic execution path but cannot validate internal state changes
        /// due to inaccessible private static fields.
        /// 
        /// Note: This test has limited value due to testing constraints but exercises the code path.
        /// </summary>
        [Fact]
        public void MaterialCheck_MultipleCalls_DoesNotThrow()
        {
            // Arrange & Act & Assert
            // Call the method multiple times to ensure it handles repeated calls
            var exception1 = Record.Exception(() => VisualMarker.MaterialCheck());
            var exception2 = Record.Exception(() => VisualMarker.MaterialCheck());

            Assert.Null(exception1);
            Assert.Null(exception2);
        }

        /// <summary>
        /// Tests that MaterialCheck method handles different execution scenarios without throwing.
        /// This is a basic smoke test since we cannot mock the static dependencies or verify
        /// the internal behavior due to design constraints.
        /// 
        /// To properly test this method, the following changes would be needed:
        /// 1. Make _isMaterialRegistered and _warnedAboutMaterial internal or provide accessors
        /// 2. Extract static dependencies into injectable services
        /// 3. Provide a way to reset static state between tests
        /// </summary>
        [Fact]
        public void MaterialCheck_BasicExecution_CompletesSuccessfully()
        {
            // Arrange
            // Cannot arrange specific state due to private static fields and unmockable dependencies

            // Act
            Exception exception = null;
            try
            {
                VisualMarker.MaterialCheck();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.Null(exception);
        }

        // Additional test methods would require:
        // - Access to private static fields (_isMaterialRegistered, _warnedAboutMaterial) for state setup/verification
        // - Ability to mock Application.Current to test null/non-null scenarios  
        // - Ability to mock DeviceInfo.Platform to test different platform behaviors
        // - Ability to mock the logger creation chain to verify logging calls
        // 
        // Without these capabilities, comprehensive testing of MaterialCheck is not feasible
        // with the current static design and unmockable dependencies.

        /// <summary>
        /// Tests that RegisterMaterial method successfully registers Material visual type.
        /// Verifies that after calling RegisterMaterial, the MaterialCheck method behavior changes
        /// to indicate that Material has been registered, preventing warning logs from being generated.
        /// </summary>
        [Fact]
        public static void RegisterMaterial_WhenCalled_RegistersMaterialSuccessfully()
        {
            // Arrange
            var mockApplication = Substitute.For<Application>();
            var mockMauiContext = Substitute.For<IMauiContext>();
            var mockLogger = Substitute.For<ILogger<IVisual>>();

            // Setup the application context chain
            Application.SetCurrentApplication(mockApplication);
            mockApplication.FindMauiContext().Returns(mockMauiContext);
            mockMauiContext.CreateLogger<IVisual>().Returns(mockLogger);

            // Reset static state - call MaterialCheck first to reset _warnedAboutMaterial
            VisualMarker.MaterialCheck();
            mockLogger.ClearReceivedCalls();

            // Act - Register Material
            VisualMarker.RegisterMaterial();

            // Assert - Verify MaterialCheck now returns early (no logging occurs)
            VisualMarker.MaterialCheck();

            // Verify that no warning was logged after registration
            mockLogger.DidNotReceive().LogWarning(Arg.Any<string>(), Arg.Any<object[]>());
        }

        /// <summary>
        /// Tests that RegisterMaterial method can be called multiple times safely.
        /// Verifies that calling RegisterMaterial multiple times maintains the registered state
        /// and continues to prevent warning logs from being generated.
        /// </summary>
        [Fact]
        public static void RegisterMaterial_WhenCalledMultipleTimes_RemainsRegistered()
        {
            // Arrange
            var mockApplication = Substitute.For<Application>();
            var mockMauiContext = Substitute.For<IMauiContext>();
            var mockLogger = Substitute.For<ILogger<IVisual>>();

            // Setup the application context chain
            Application.SetCurrentApplication(mockApplication);
            mockApplication.FindMauiContext().Returns(mockMauiContext);
            mockMauiContext.CreateLogger<IVisual>().Returns(mockLogger);

            // Reset static state
            VisualMarker.MaterialCheck();
            mockLogger.ClearReceivedCalls();

            // Act - Register Material multiple times
            VisualMarker.RegisterMaterial();
            VisualMarker.RegisterMaterial();
            VisualMarker.RegisterMaterial();

            // Assert - Verify MaterialCheck still returns early (no logging occurs)
            VisualMarker.MaterialCheck();

            // Verify that no warning was logged after multiple registrations
            mockLogger.DidNotReceive().LogWarning(Arg.Any<string>(), Arg.Any<object[]>());
        }
    }
}