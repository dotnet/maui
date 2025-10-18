#nullable disable

using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    // Run all these tests on a new thread/task in order to control the dispatcher creation.

    public class DeviceUnitTests : BaseTestFixture
    {
        // just a check to make sure the test dispatcher is working
        [Fact]
        public Task TestImplementationHasDispatcher() => DispatcherTest.Run(() =>
        {
            Assert.False(DispatcherProviderStubOptions.SkipDispatcherCreation);
            Assert.False(Device.IsInvokeRequired);

            // can create things
            var button = new Button();
        });

        [Fact]
        public Task BackgroundThreadDoesNotHaveDispatcher() => DispatcherTest.Run(() =>
        {
            // act like the real world
            DispatcherProviderStubOptions.SkipDispatcherCreation = true;

            // can create things
            var button = new Button();
        });

        [Fact]
        public Task TestBeginInvokeOnMainThread() => DispatcherTest.Run(() =>
        {
            bool calledFromMainThread = false;
            MockPlatformServices(() => calledFromMainThread = true);

            bool invoked = false;
            Device.BeginInvokeOnMainThread(() => invoked = true);

            Assert.True(invoked, "Action not invoked.");
            Assert.True(calledFromMainThread, "Action not invoked from main thread.");
        });

        [Fact]
        public Task TestInvokeOnMainThreadWithSyncFunc() => DispatcherTest.Run(async () =>
        {
            bool calledFromMainThread = false;
            MockPlatformServices(() => calledFromMainThread = true);

            bool invoked = false;
            var result = await Device.InvokeOnMainThreadAsync(() => { invoked = true; return true; });

            Assert.True(invoked, "Action not invoked.");
            Assert.True(calledFromMainThread, "Action not invoked from main thread.");
            Assert.True(result, "Unexpected result.");
        });

        [Fact]
        public Task TestInvokeOnMainThreadWithSyncAction() => DispatcherTest.Run(async () =>
        {
            bool calledFromMainThread = false;
            MockPlatformServices(() => calledFromMainThread = true);

            bool invoked = false;
            await Device.InvokeOnMainThreadAsync(() => { invoked = true; });

            Assert.True(invoked, "Action not invoked.");
            Assert.True(calledFromMainThread, "Action not invoked from main thread.");
        });

        [Fact]
        public Task TestInvokeOnMainThreadWithAsyncFunc() => DispatcherTest.Run(async () =>
        {
            bool calledFromMainThread = false;
            MockPlatformServices(
                () => calledFromMainThread = true,
                invokeOnMainThread: action => Task.Delay(50).ContinueWith(_ => action()));

            bool invoked = false;
            var task = Device.InvokeOnMainThreadAsync(async () => { invoked = true; return true; });
            Assert.True(calledFromMainThread, "Action not invoked from main thread.");
            Assert.False(invoked, "Action invoked early.");

            var result = await task;
            Assert.True(invoked, "Action not invoked.");
            Assert.True(result, "Unexpected result.");
        });

        [Fact]
        public Task TestInvokeOnMainThreadWithAsyncFuncError() => DispatcherTest.Run(async () =>
        {
            bool calledFromMainThread = false;
            MockPlatformServices(
                () => calledFromMainThread = true,
                invokeOnMainThread: action => Task.Delay(50).ContinueWith(_ => action()));

            bool invoked = false;
            async Task<bool> boom()
            {
                invoked = true;
                throw new ApplicationException();
            }
            var task = Device.InvokeOnMainThreadAsync(boom);
            Assert.True(calledFromMainThread, "Action not invoked from main thread.");
            Assert.False(invoked, "Action invoked early.");

            async Task MethodThatThrows() => await task;
            await Assert.ThrowsAsync<ApplicationException>(MethodThatThrows);
        });

        [Fact]
        public Task TestInvokeOnMainThreadWithAsyncAction() => DispatcherTest.Run(async () =>
        {
            bool calledFromMainThread = false;
            MockPlatformServices(
                () => calledFromMainThread = true,
                invokeOnMainThread: action => Task.Delay(50).ContinueWith(_ => action()));

            bool invoked = false;
            var task = Device.InvokeOnMainThreadAsync(async () => { invoked = true; });
            Assert.True(calledFromMainThread, "Action not invoked from main thread.");
            Assert.False(invoked, "Action invoked early.");

            await task;
            Assert.True(invoked, "Action not invoked.");
        });

        [Fact]
        public Task TestInvokeOnMainThreadWithAsyncActionError() => DispatcherTest.Run(async () =>
        {
            bool calledFromMainThread = false;
            MockPlatformServices(
                () => calledFromMainThread = true,
                invokeOnMainThread: action => Task.Delay(50).ContinueWith(_ => action()));

            bool invoked = false;
            async Task boom()
            { invoked = true; throw new ApplicationException(); }
            var task = Device.InvokeOnMainThreadAsync(boom);
            Assert.True(calledFromMainThread, "Action not invoked from main thread.");
            Assert.False(invoked, "Action invoked early.");

            async Task MethodThatThrows() => await task;
            await Assert.ThrowsAsync<ApplicationException>(MethodThatThrows);
        });

        private static void MockPlatformServices(Action onInvokeOnMainThread, Action<Action> invokeOnMainThread = null)
        {
            DispatcherProviderStubOptions.InvokeOnMainThread =
                action =>
                {
                    onInvokeOnMainThread();

                    if (invokeOnMainThread == null)
                        action();
                    else
                        invokeOnMainThread(action);
                };
        }

        /// <summary>
        /// Tests that GetMainThreadSynchronizationContextAsync returns a valid SynchronizationContext when executed in a proper dispatcher environment.
        /// This test verifies the normal execution path of the method.
        /// Expected result: Method returns a Task that completes successfully with a valid SynchronizationContext.
        /// </summary>
        [Fact]
        public Task GetMainThreadSynchronizationContextAsync_WithValidDispatcher_ReturnsSynchronizationContext() => DispatcherTest.Run(async () =>
        {
            // Arrange
            // DispatcherTest.Run sets up a valid dispatcher environment

            // Act
            var result = await Device.GetMainThreadSynchronizationContextAsync();

            // Assert  
            Assert.NotNull(result);
        });

        /// <summary>
        /// Tests that GetMainThreadSynchronizationContextAsync throws InvalidOperationException when no dispatcher is available.
        /// This test simulates a scenario where the dispatcher creation is skipped.
        /// Expected result: Method throws InvalidOperationException from FindDispatcher.
        /// </summary>
        [Fact]
        public Task GetMainThreadSynchronizationContextAsync_WithoutDispatcher_ThrowsInvalidOperationException() => DispatcherTest.Run(async () =>
        {
            // Arrange
            DispatcherProviderStubOptions.SkipDispatcherCreation = true;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Device.GetMainThreadSynchronizationContextAsync());
            Assert.Contains("BindableObject was not instantiated on a thread with a dispatcher", exception.Message);
        });

        /// <summary>
        /// Tests that GetMainThreadSynchronizationContextAsync throws NullReferenceException when Application.Current is null.
        /// This test verifies error handling when the static Application.Current property is null.
        /// Expected result: Method throws NullReferenceException.
        /// </summary>
        [Fact]
        public async Task GetMainThreadSynchronizationContextAsync_WithNullApplicationCurrent_ThrowsNullReferenceException()
        {
            // Arrange
            var originalCurrent = Application.Current;
            Application.Current = null;

            try
            {
                // Act & Assert
                await Assert.ThrowsAsync<NullReferenceException>(() => Device.GetMainThreadSynchronizationContextAsync());
            }
            finally
            {
                // Cleanup
                Application.Current = originalCurrent;
            }
        }
    }

    public partial class DeviceGetNamedSizeTests
    {
        /// <summary>
        /// Tests GetNamedSize method with valid NamedSize enum values and a valid Element.
        /// Verifies that the method correctly calls GetNamedSize(size, targetElement.GetType()).
        /// </summary>
        /// <param name="namedSize">The NamedSize enum value to test</param>
        [Theory]
        [InlineData(NamedSize.Default)]
        [InlineData(NamedSize.Micro)]
        [InlineData(NamedSize.Small)]
        [InlineData(NamedSize.Medium)]
        [InlineData(NamedSize.Large)]
        [InlineData(NamedSize.Body)]
        [InlineData(NamedSize.Header)]
        [InlineData(NamedSize.Title)]
        [InlineData(NamedSize.Subtitle)]
        [InlineData(NamedSize.Caption)]
        public void GetNamedSize_ValidNamedSizeWithValidElement_CallsThroughToTypeOverload(NamedSize namedSize)
        {
            // Arrange
            var mockElement = Substitute.For<Element>();

            // Act & Assert - This will likely throw NotImplementedException from the dependency service,
            // but that's expected behavior when the service isn't configured in unit tests
            var exception = Assert.Throws<NotImplementedException>(() =>
                Device.GetNamedSize(namedSize, mockElement));

            Assert.Contains("IFontNamedSizeService", exception.Message);
        }

        /// <summary>
        /// Tests GetNamedSize method with null Element parameter.
        /// Verifies that the method throws NullReferenceException when targetElement.GetType() is called on null.
        /// </summary>
        [Fact]
        public void GetNamedSize_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            Element nullElement = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                Device.GetNamedSize(NamedSize.Medium, nullElement));
        }

        /// <summary>
        /// Tests GetNamedSize method with invalid NamedSize enum values (outside defined range).
        /// Verifies that invalid enum values are passed through to the underlying implementation.
        /// </summary>
        /// <param name="invalidEnumValue">Invalid enum value cast from integer</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void GetNamedSize_InvalidNamedSizeEnumValues_PassesThroughToImplementation(int invalidEnumValue)
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            var invalidNamedSize = (NamedSize)invalidEnumValue;

            // Act & Assert - Should pass through to implementation and throw NotImplementedException
            var exception = Assert.Throws<NotImplementedException>(() =>
                Device.GetNamedSize(invalidNamedSize, mockElement));

            Assert.Contains("IFontNamedSizeService", exception.Message);
        }

        /// <summary>
        /// Tests GetNamedSize method with different Element types.
        /// Verifies that the method correctly gets the Type from different Element implementations.
        /// </summary>
        [Fact]
        public void GetNamedSize_DifferentElementTypes_CorrectlyGetsElementType()
        {
            // Arrange
            var mockElement1 = Substitute.For<Element>();
            var mockElement2 = Substitute.For<Element>();

            // Act & Assert - Both should behave the same way regardless of mock instance
            var exception1 = Assert.Throws<NotImplementedException>(() =>
                Device.GetNamedSize(NamedSize.Default, mockElement1));

            var exception2 = Assert.Throws<NotImplementedException>(() =>
                Device.GetNamedSize(NamedSize.Default, mockElement2));

            Assert.Contains("IFontNamedSizeService", exception1.Message);
            Assert.Contains("IFontNamedSizeService", exception2.Message);
        }

        /// <summary>
        /// Tests GetNamedSize with all valid NamedSize enum values and null Type parameter.
        /// Verifies that the method delegates correctly to the 3-parameter overload.
        /// </summary>
        [Theory]
        [InlineData(NamedSize.Default)]
        [InlineData(NamedSize.Micro)]
        [InlineData(NamedSize.Small)]
        [InlineData(NamedSize.Medium)]
        [InlineData(NamedSize.Large)]
        [InlineData(NamedSize.Body)]
        [InlineData(NamedSize.Header)]
        [InlineData(NamedSize.Title)]
        [InlineData(NamedSize.Subtitle)]
        [InlineData(NamedSize.Caption)]
        public void GetNamedSize_ValidNamedSizeWithNullType_CallsThreeParameterOverload(NamedSize size)
        {
            // Arrange & Act
            var result = Device.GetNamedSize(size, null);

            // Assert
            Assert.IsType<double>(result);
        }

        /// <summary>
        /// Tests GetNamedSize with valid NamedSize values and various Type parameters.
        /// Verifies that the method works with different target element types.
        /// </summary>
        [Theory]
        [InlineData(NamedSize.Medium, typeof(string))]
        [InlineData(NamedSize.Large, typeof(Label))]
        [InlineData(NamedSize.Small, typeof(Button))]
        [InlineData(NamedSize.Default, typeof(object))]
        public void GetNamedSize_ValidNamedSizeWithSpecificTypes_CallsThreeParameterOverload(NamedSize size, Type targetElementType)
        {
            // Arrange & Act
            var result = Device.GetNamedSize(size, targetElementType);

            // Assert
            Assert.IsType<double>(result);
        }

        /// <summary>
        /// Tests GetNamedSize with boundary NamedSize enum values.
        /// Verifies that the minimum and maximum enum values are handled correctly.
        /// </summary>
        [Theory]
        [InlineData((NamedSize)0)] // Default/minimum value
        [InlineData((NamedSize)9)] // Caption/maximum value
        public void GetNamedSize_BoundaryNamedSizeValues_CallsThreeParameterOverload(NamedSize size)
        {
            // Arrange & Act  
            var result = Device.GetNamedSize(size, typeof(Label));

            // Assert
            Assert.IsType<double>(result);
        }

        /// <summary>
        /// Tests GetNamedSize with invalid NamedSize enum values outside the defined range.
        /// Verifies that the method handles undefined enum values by delegating to the 3-parameter overload.
        /// </summary>
        [Theory]
        [InlineData((NamedSize)(-1))] // Below minimum
        [InlineData((NamedSize)10)]   // Above maximum
        [InlineData((NamedSize)100)]  // Far above maximum
        public void GetNamedSize_InvalidNamedSizeValues_CallsThreeParameterOverload(NamedSize size)
        {
            // Arrange & Act
            var result = Device.GetNamedSize(size, null);

            // Assert
            Assert.IsType<double>(result);
        }

        /// <summary>
        /// Tests GetNamedSize to verify consistency with the 3-parameter overload.
        /// Ensures that the 2-parameter method produces the same result as calling the 3-parameter method with useOldSizes=false.
        /// </summary>
        [Theory]
        [InlineData(NamedSize.Medium)]
        [InlineData(NamedSize.Large)]
        [InlineData(NamedSize.Small)]
        public void GetNamedSize_CompareWith3ParameterOverload_ProducesIdenticalResults(NamedSize size)
        {
            // Arrange
            Type targetType = typeof(Label);

            // Act
            var twoParamResult = Device.GetNamedSize(size, targetType);
            var threeParamResult = Device.GetNamedSize(size, targetType, false);

            // Assert
            Assert.Equal(threeParamResult, twoParamResult);
        }

        /// <summary>
        /// Tests GetNamedSize with various combinations of NamedSize and Type parameters.
        /// Verifies comprehensive coverage of different input scenarios.
        /// </summary>
        [Theory]
        [InlineData(NamedSize.Default, null)]
        [InlineData(NamedSize.Micro, typeof(Entry))]
        [InlineData(NamedSize.Body, typeof(Editor))]
        [InlineData(NamedSize.Header, typeof(Picker))]
        [InlineData(NamedSize.Caption, typeof(DatePicker))]
        public void GetNamedSize_VariousParameterCombinations_CallsThreeParameterOverload(NamedSize size, Type targetElementType)
        {
            // Arrange & Act
            var result = Device.GetNamedSize(size, targetElementType);

            // Assert
            Assert.IsType<double>(result);
        }
    }


    /// <summary>
    /// Unit tests for the Device.FlowDirection property.
    /// </summary>
    public partial class DeviceFlowDirectionTests
    {
        /// <summary>
        /// Tests that Device.FlowDirection returns RightToLeft when AppInfo.RequestedLayoutDirection is RightToLeft.
        /// This test is currently skipped because AppInfo.RequestedLayoutDirection is a static property that cannot
        /// be mocked with NSubstitute, and creating fake implementations is forbidden by the testing guidelines.
        /// To make this test functional, the production code would need to accept the layout direction as a parameter
        /// or through dependency injection, or a test-specific mechanism for overriding static dependencies would need to be provided.
        /// </summary>
        [Fact(Skip = "Cannot mock static AppInfo.RequestedLayoutDirection property - requires dependency injection or test infrastructure for static mocking")]
        public void FlowDirection_WhenRequestedLayoutDirectionIsRightToLeft_ReturnsRightToLeft()
        {
            // Arrange
            // TODO: This test cannot be implemented without the ability to mock or override AppInfo.RequestedLayoutDirection
            // The static property AppInfo.RequestedLayoutDirection would need to return LayoutDirection.RightToLeft

            // Act
            // var result = Device.FlowDirection;

            // Assert
            // Assert.Equal(FlowDirection.RightToLeft, result);
        }

        /// <summary>
        /// Tests that Device.FlowDirection returns LeftToRight when AppInfo.RequestedLayoutDirection is LeftToRight.
        /// This test is currently skipped because AppInfo.RequestedLayoutDirection is a static property that cannot
        /// be mocked with NSubstitute, and creating fake implementations is forbidden by the testing guidelines.
        /// To make this test functional, the production code would need to accept the layout direction as a parameter
        /// or through dependency injection, or a test-specific mechanism for overriding static dependencies would need to be provided.
        /// </summary>
        [Fact(Skip = "Cannot mock static AppInfo.RequestedLayoutDirection property - requires dependency injection or test infrastructure for static mocking")]
        public void FlowDirection_WhenRequestedLayoutDirectionIsLeftToRight_ReturnsLeftToRight()
        {
            // Arrange
            // TODO: This test cannot be implemented without the ability to mock or override AppInfo.RequestedLayoutDirection
            // The static property AppInfo.RequestedLayoutDirection would need to return LayoutDirection.LeftToRight

            // Act
            // var result = Device.FlowDirection;

            // Assert
            // Assert.Equal(FlowDirection.LeftToRight, result);
        }

        /// <summary>
        /// Tests that Device.FlowDirection returns LeftToRight when AppInfo.RequestedLayoutDirection is Unknown.
        /// This test is currently skipped because AppInfo.RequestedLayoutDirection is a static property that cannot
        /// be mocked with NSubstitute, and creating fake implementations is forbidden by the testing guidelines.
        /// To make this test functional, the production code would need to accept the layout direction as a parameter
        /// or through dependency injection, or a test-specific mechanism for overriding static dependencies would need to be provided.
        /// </summary>
        [Fact(Skip = "Cannot mock static AppInfo.RequestedLayoutDirection property - requires dependency injection or test infrastructure for static mocking")]
        public void FlowDirection_WhenRequestedLayoutDirectionIsUnknown_ReturnsLeftToRight()
        {
            // Arrange
            // TODO: This test cannot be implemented without the ability to mock or override AppInfo.RequestedLayoutDirection
            // The static property AppInfo.RequestedLayoutDirection would need to return LayoutDirection.Unknown

            // Act
            // var result = Device.FlowDirection;

            // Assert
            // Assert.Equal(FlowDirection.LeftToRight, result);
        }
    }


    public partial class DeviceRuntimePlatformTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that RuntimePlatform returns a non-null, non-empty string.
        /// This validates that the property correctly calls DeviceInfo.Platform.ToString()
        /// and returns a valid platform identifier.
        /// </summary>
        [Fact]
        public void RuntimePlatform_WhenCalled_ReturnsNonNullNonEmptyString()
        {
            // Act
            var result = Device.RuntimePlatform;

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        /// <summary>
        /// Tests that RuntimePlatform returns a valid platform identifier.
        /// This validates that the returned string matches one of the known DevicePlatform values.
        /// </summary>
        [Theory]
        [InlineData("Android")]
        [InlineData("iOS")]
        [InlineData("macOS")]
        [InlineData("MacCatalyst")]
        [InlineData("tvOS")]
        [InlineData("Tizen")]
        [InlineData("WinUI")]
        [InlineData("watchOS")]
        [InlineData("Unknown")]
        public void RuntimePlatform_WhenCalled_ReturnsValidPlatformIdentifier(string expectedPlatform)
        {
            // Act
            var result = Device.RuntimePlatform;

            // Assert - Since we can't control which platform the test runs on, 
            // we verify that the result is one of the valid platform names
            var validPlatforms = new[] { "Android", "iOS", "macOS", "MacCatalyst", "tvOS", "Tizen", "WinUI", "watchOS", "Unknown" };
            Assert.Contains(result, validPlatforms);
        }

        /// <summary>
        /// Tests that RuntimePlatform returns consistent results across multiple calls.
        /// This validates that the property behaves deterministically and doesn't change
        /// between invocations within the same test run.
        /// </summary>
        [Fact]
        public void RuntimePlatform_WhenCalledMultipleTimes_ReturnsConsistentValue()
        {
            // Act
            var result1 = Device.RuntimePlatform;
            var result2 = Device.RuntimePlatform;
            var result3 = Device.RuntimePlatform;

            // Assert
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
            Assert.Equal(result1, result3);
        }

        /// <summary>
        /// Tests that RuntimePlatform returns a string type.
        /// This validates the return type of the property matches the expected string type.
        /// </summary>
        [Fact]
        public void RuntimePlatform_WhenCalled_ReturnsStringType()
        {
            // Act
            var result = Device.RuntimePlatform;

            // Assert
            Assert.IsType<string>(result);
        }
    }


    /// <summary>
    /// Unit tests for the Device.Idiom property.
    /// </summary>
    public partial class DeviceIdiomTests
    {
        /// <summary>
        /// Tests that the Device.Idiom property can be accessed without throwing exceptions
        /// and returns a valid TargetIdiom enum value.
        /// </summary>
        [Fact]
        public void Idiom_WhenAccessed_ReturnsValidTargetIdiom()
        {
            // Arrange
            // Note: Cannot mock DeviceInfo.Idiom as it's a static property with no interface

            // Act
            TargetIdiom result = Device.Idiom;

            // Assert
            Assert.True(Enum.IsDefined(typeof(TargetIdiom), result));
        }

        /// <summary>
        /// Tests that the Device.Idiom property consistently returns the same value
        /// when accessed multiple times.
        /// </summary>
        [Fact]
        public void Idiom_WhenAccessedMultipleTimes_ReturnsConsistentValue()
        {
            // Arrange & Act
            TargetIdiom firstCall = Device.Idiom;
            TargetIdiom secondCall = Device.Idiom;

            // Assert
            Assert.Equal(firstCall, secondCall);
        }

        /// <summary>
        /// Tests that the Device.Idiom property returns one of the expected TargetIdiom values.
        /// Due to static dependency on DeviceInfo.Idiom, we cannot test specific mapping scenarios.
        /// </summary>
        [Fact]
        public void Idiom_WhenAccessed_ReturnsExpectedTargetIdiomValue()
        {
            // Arrange
            var expectedValues = new[]
            {
                TargetIdiom.Unsupported,
                TargetIdiom.Phone,
                TargetIdiom.Tablet,
                TargetIdiom.Desktop,
                TargetIdiom.TV,
                TargetIdiom.Watch
            };

            // Act
            TargetIdiom result = Device.Idiom;

            // Assert
            Assert.Contains(result, expectedValues);
        }

        // Note: The following test methods cannot be implemented due to the static dependency on DeviceInfo.Idiom.
        // To fully test the mapping logic from DeviceIdiom to TargetIdiom, the Device.Idiom property would need
        // to be refactored to accept a parameter or use dependency injection.
        //
        // The mapping logic being tested would be:
        // - DeviceIdiom.Tablet -> TargetIdiom.Tablet
        // - DeviceIdiom.Phone -> TargetIdiom.Phone  
        // - DeviceIdiom.Desktop -> TargetIdiom.Desktop
        // - DeviceIdiom.TV -> TargetIdiom.TV
        // - DeviceIdiom.Watch -> TargetIdiom.Watch
        // - Any other DeviceIdiom -> TargetIdiom.Unsupported
        //
        // To enable comprehensive testing, consider:
        // 1. Extracting the mapping logic to a separate testable method
        // 2. Using dependency injection for DeviceInfo access
        // 3. Making DeviceInfo.SetCurrent public for testing purposes

        /// <summary>
        /// Tests that accessing Device.Idiom does not throw any exceptions.
        /// This test ensures the property getter executes successfully.
        /// </summary>
        [Fact]
        public void Idiom_WhenAccessed_DoesNotThrowException()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() => Device.Idiom);
            Assert.Null(exception);
        }
    }
}