using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests;

public partial class MauiControlsInitializerTests
{
    /// <summary>
    /// Tests that the Initialize method handles a null services parameter appropriately.
    /// Expects an ArgumentNullException when services is null since GetRequiredApplicationDispatcher() 
    /// will be called on the null reference in Windows builds.
    /// </summary>
    [Fact]
    public void Initialize_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        var initializer = new MauiControlsInitializerTestWrapper();

        // Act & Assert
#if WINDOWS
			Assert.Throws<ArgumentNullException>(() => initializer.Initialize(null));
#else
        // On non-Windows platforms, the method body is empty due to #if WINDOWS,
        // so no exception should be thrown
        var exception = Record.Exception(() => initializer.Initialize(null));
        Assert.Null(exception);
#endif
    }

    /// <summary>
    /// Tests that the Initialize method executes without throwing when provided with a valid services parameter.
    /// On Windows platforms, this tests the dispatcher interaction; on other platforms, it ensures the empty method executes correctly.
    /// </summary>
    [Fact]
    public void Initialize_ValidServices_DoesNotThrow()
    {
        // Arrange
        var services = Substitute.For<IServiceProvider>();
        var initializer = new MauiControlsInitializerTestWrapper();

#if WINDOWS
			// On Windows, we need to mock the dispatcher to prevent actual Windows API calls
			var mockDispatcher = Substitute.For<IDispatcher>();
			services.GetRequiredService<IDispatcher>().Returns(mockDispatcher);
			
			// Configure the mock to execute the callback immediately for testing
			mockDispatcher.DispatchIfRequired(Arg.Any<Action>()).Returns(x => 
			{
				var action = x.Arg<Action>();
				// Note: We cannot fully test the Windows-specific UI.Xaml.Application calls
				// as they require a Windows UI context and static dependencies
				return true;
			});
#endif

        // Act & Assert
        var exception = Record.Exception(() => initializer.Initialize(services));
        Assert.Null(exception);

#if WINDOWS
			// Verify that GetRequiredApplicationDispatcher was called
			services.Received(1).GetRequiredService<IDispatcher>();
#endif
    }

    /// <summary>
    /// Tests the Initialize method with various IServiceProvider implementations to ensure compatibility.
    /// Verifies that the method can handle different service provider configurations without throwing exceptions.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Initialize_DifferentServiceProviderConfigurations_DoesNotThrow(bool hasDispatcherService)
    {
        // Arrange
        var services = Substitute.For<IServiceProvider>();
        var initializer = new MauiControlsInitializerTestWrapper();

#if WINDOWS
			if (hasDispatcherService)
			{
				var mockDispatcher = Substitute.For<IDispatcher>();
				services.GetRequiredService<IDispatcher>().Returns(mockDispatcher);
				mockDispatcher.DispatchIfRequired(Arg.Any<Action>()).Returns(true);
			}
			else
			{
				// Configure to throw when GetRequiredApplicationDispatcher is called
				services.When(x => x.GetRequiredService<IDispatcher>())
					.Do(x => throw new InvalidOperationException("Service not registered"));
			}
#endif

        // Act & Assert
#if WINDOWS
			if (hasDispatcherService)
			{
				var exception = Record.Exception(() => initializer.Initialize(services));
				Assert.Null(exception);
			}
			else
			{
				Assert.Throws<InvalidOperationException>(() => initializer.Initialize(services));
			}
#else
        // On non-Windows platforms, the method does nothing regardless of service configuration
        var exception = Record.Exception(() => initializer.Initialize(services));
        Assert.Null(exception);
#endif
    }

    /// <summary>
    /// Wrapper class to expose the internal MauiControlsInitializer for testing purposes.
    /// This allows us to test the Initialize method directly without needing to access internal types.
    /// </summary>
    private class MauiControlsInitializerTestWrapper : IMauiInitializeService
    {
        public MauiControlsInitializerTestWrapper()
        {
            _inner = new AppHostBuilderExtensions.MauiControlsInitializer();
        }

        public void Initialize(IServiceProvider services)
        {
            _inner.Initialize(services);
        }
    }
}
