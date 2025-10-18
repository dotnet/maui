using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Diagnostics;
using Microsoft.Maui.Diagnostics;
using Microsoft.Maui.Hosting;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests.Diagnostics;

public partial class MauiControlsDiagnosticsExtensionsTests
{
    /// <summary>
    /// Tests that ConfigureMauiControlsDiagnostics throws ArgumentNullException when the builder parameter is null.
    /// This test verifies proper null parameter validation.
    /// Expected result: ArgumentNullException is thrown.
    /// </summary>
    [Fact]
    public void ConfigureMauiControlsDiagnostics_NullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        MauiAppBuilder nullBuilder = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullBuilder.ConfigureMauiControlsDiagnostics());
    }

    /// <summary>
    /// Tests that ConfigureMauiControlsDiagnostics registers the diagnostic tagger service and returns the builder
    /// when meter support is available (default case).
    /// This test verifies the normal operation flow and service registration.
    /// Expected result: IDiagnosticTagger service is registered with ControlsViewDiagnosticTagger implementation and the same builder is returned.
    /// </summary>
    [Fact]
    public void ConfigureMauiControlsDiagnostics_MeterSupported_RegistersDiagnosticTaggerAndReturnsBuilder()
    {
        // Arrange
        var builder = MauiApp.CreateBuilder(useDefaults: false);
        var initialServiceCount = builder.Services.Count;

        // Act
        var result = builder.ConfigureMauiControlsDiagnostics();

        // Assert
        Assert.Same(builder, result);

        // Verify that a service was added
        Assert.True(builder.Services.Count > initialServiceCount);

        // Verify that the IDiagnosticTagger service is registered
        var serviceDescriptor = Assert.Single(builder.Services,
            s => s.ServiceType == typeof(IDiagnosticTagger));

        Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
        Assert.Equal(typeof(ControlsViewDiagnosticTagger), serviceDescriptor.ImplementationType);
    }

    /// <summary>
    /// Tests that ConfigureMauiControlsDiagnostics can be called multiple times without duplicating services.
    /// This test verifies that TryAddEnumerable prevents duplicate service registrations.
    /// Expected result: Only one IDiagnosticTagger service registration exists after multiple calls.
    /// </summary>
    [Fact]
    public void ConfigureMauiControlsDiagnostics_CalledMultipleTimes_DoesNotDuplicateServices()
    {
        // Arrange
        var builder = MauiApp.CreateBuilder(useDefaults: false);

        // Act
        builder.ConfigureMauiControlsDiagnostics();
        builder.ConfigureMauiControlsDiagnostics();
        builder.ConfigureMauiControlsDiagnostics();

        // Assert
        var diagnosticTaggerServices = builder.Services
            .Where(s => s.ServiceType == typeof(IDiagnosticTagger))
            .ToList();

        Assert.Single(diagnosticTaggerServices);
    }

    // NOTE: Testing the early return path when RuntimeFeature.IsMeterSupported is false is challenging
    // because RuntimeFeature.IsMeterSupported is a static readonly property that gets its value from
    // AppContext.TryGetSwitch("System.Diagnostics.Metrics.Meter.IsSupported") with a default of true.
    // This property cannot be mocked with NSubstitute, and manipulating AppContext switches in unit tests
    // can have global side effects and is not recommended for isolated unit testing.
    // 
    // To test this scenario, consider:
    // 1. Integration tests where AppContext switches can be safely manipulated
    // 2. Refactoring the code to make the meter support check injectable/testable
    // 3. Testing in an environment where the AppContext switch is explicitly set to false
}
