#nullable disable
//
// AsyncValueTests.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2013-2014 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for AsyncValue<T>.IsRunning property
    /// </summary>
    public partial class AsyncValueTests
    {
        /// <summary>
        /// Tests that IsRunning getter returns the current internal field value.
        /// Verifies basic property getter functionality.
        /// </summary>
        [Fact]
        public void IsRunning_Get_ReturnsCurrentValue()
        {
            // Arrange
            var completedTask = Task.FromResult(42);
            var asyncValue = new AsyncValue<int>(completedTask);

            // Act
            bool result = asyncValue.IsRunning;

            // Assert
            Assert.False(result); // Should be false for completed task
        }

        /// <summary>
        /// Tests that setting IsRunning to the same value does not trigger PropertyChanged event.
        /// Verifies the early return optimization when value hasn't changed.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsRunning_SetSameValue_DoesNotRaisePropertyChanged(bool value)
        {
            // Arrange
            var completedTask = Task.FromResult(42);
            var asyncValue = new AsyncValue<int>(completedTask);

            // First set the value
            asyncValue.IsRunning = value;

            bool eventRaised = false;
            asyncValue.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            asyncValue.IsRunning = value; // Set same value again

            // Assert
            Assert.False(eventRaised);
            Assert.Equal(value, asyncValue.IsRunning);
        }

        /// <summary>
        /// Tests that setting IsRunning to a different value updates the property and raises PropertyChanged event.
        /// Verifies property change notification with correct property name.
        /// </summary>
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void IsRunning_SetDifferentValue_UpdatesValueAndRaisesPropertyChanged(bool initialValue, bool newValue)
        {
            // Arrange
            var completedTask = Task.FromResult(42);
            var asyncValue = new AsyncValue<int>(completedTask);
            asyncValue.IsRunning = initialValue;

            string propertyName = null;
            bool eventRaised = false;
            asyncValue.PropertyChanged += (sender, args) =>
            {
                eventRaised = true;
                propertyName = args.PropertyName;
            };

            // Act
            asyncValue.IsRunning = newValue;

            // Assert
            Assert.True(eventRaised);
            Assert.Equal("IsRunning", propertyName);
            Assert.Equal(newValue, asyncValue.IsRunning);
        }

        /// <summary>
        /// Tests that multiple consecutive sets with the same value only raise PropertyChanged on the first change.
        /// Verifies that the early return optimization works consistently.
        /// </summary>
        [Fact]
        public void IsRunning_MultipleSetsSameValue_OnlyFirstChangeRaisesEvent()
        {
            // Arrange
            var completedTask = Task.FromResult(42);
            var asyncValue = new AsyncValue<int>(completedTask);
            asyncValue.IsRunning = false; // Ensure initial state

            int eventCount = 0;
            asyncValue.PropertyChanged += (sender, args) => eventCount++;

            // Act
            asyncValue.IsRunning = true;  // Should raise event
            asyncValue.IsRunning = true;  // Should not raise event
            asyncValue.IsRunning = true;  // Should not raise event

            // Assert
            Assert.Equal(1, eventCount);
            Assert.True(asyncValue.IsRunning);
        }

        /// <summary>
        /// Tests that alternating between different values raises PropertyChanged event for each change.
        /// Verifies that the property correctly handles multiple value changes.
        /// </summary>
        [Fact]
        public void IsRunning_AlternatingValues_RaisesEventForEachChange()
        {
            // Arrange
            var completedTask = Task.FromResult(42);
            var asyncValue = new AsyncValue<int>(completedTask);
            asyncValue.IsRunning = false; // Ensure initial state

            int eventCount = 0;
            asyncValue.PropertyChanged += (sender, args) => eventCount++;

            // Act
            asyncValue.IsRunning = true;   // Should raise event (1)
            asyncValue.IsRunning = false;  // Should raise event (2)
            asyncValue.IsRunning = true;   // Should raise event (3)
            asyncValue.IsRunning = false;  // Should raise event (4)

            // Assert
            Assert.Equal(4, eventCount);
            Assert.False(asyncValue.IsRunning);
        }

        /// <summary>
        /// Tests that Value property returns the default value when the task has not completed successfully.
        /// Tests with a pending/running task that has not reached RanToCompletion status.
        /// Expected result: Should return the provided default value.
        /// </summary>
        [Fact]
        public void Value_WhenTaskIsRunning_ReturnsDefaultValue()
        {
            // Arrange
            var tcs = new TaskCompletionSource<string>();
            var defaultValue = "default";
            var asyncValue = new AsyncValue<string>(tcs.Task, defaultValue);

            // Act
            var result = asyncValue.Value;

            // Assert
            Assert.Equal(defaultValue, result);
        }

        /// <summary>
        /// Tests that Value property returns the default value when the task is canceled.
        /// Tests with a task that has been canceled and is not in RanToCompletion status.
        /// Expected result: Should return the provided default value.
        /// </summary>
        [Fact]
        public void Value_WhenTaskIsCanceled_ReturnsDefaultValue()
        {
            // Arrange
            var tcs = new TaskCompletionSource<int>();
            var defaultValue = 42;
            var asyncValue = new AsyncValue<int>(tcs.Task, defaultValue);
            tcs.SetCanceled();

            // Act
            var result = asyncValue.Value;

            // Assert
            Assert.Equal(defaultValue, result);
        }

        /// <summary>
        /// Tests that Value property returns the default value when the task has faulted.
        /// Tests with a task that has thrown an exception and is not in RanToCompletion status.
        /// Expected result: Should return the provided default value.
        /// </summary>
        [Fact]
        public void Value_WhenTaskIsFaulted_ReturnsDefaultValue()
        {
            // Arrange
            var tcs = new TaskCompletionSource<double>();
            var defaultValue = 3.14;
            var asyncValue = new AsyncValue<double>(tcs.Task, defaultValue);
            tcs.SetException(new InvalidOperationException("Test exception"));

            // Act
            var result = asyncValue.Value;

            // Assert
            Assert.Equal(defaultValue, result);
        }

        /// <summary>
        /// Tests that Value property returns the task result when the task has completed successfully.
        /// Tests with a task that has reached RanToCompletion status.
        /// Expected result: Should return the task's result value.
        /// </summary>
        [Fact]
        public void Value_WhenTaskIsCompletedSuccessfully_ReturnsTaskResult()
        {
            // Arrange
            var taskResult = "completed result";
            var defaultValue = "default";
            var completedTask = Task.FromResult(taskResult);
            var asyncValue = new AsyncValue<string>(completedTask, defaultValue);

            // Act
            var result = asyncValue.Value;

            // Assert
            Assert.Equal(taskResult, result);
        }

        /// <summary>
        /// Tests that Value property returns the task result when task completes after AsyncValue creation.
        /// Tests with a task that transitions from running to RanToCompletion status.
        /// Expected result: Should return the task's result value after completion.
        /// </summary>
        [Fact]
        public async Task Value_WhenTaskCompletesLater_ReturnsTaskResult()
        {
            // Arrange
            var tcs = new TaskCompletionSource<bool>();
            var defaultValue = false;
            var asyncValue = new AsyncValue<bool>(tcs.Task, defaultValue);

            // Verify initial state returns default
            Assert.Equal(defaultValue, asyncValue.Value);

            // Act
            var taskResult = true;
            tcs.SetResult(taskResult);
            await tcs.Task;

            // Assert
            Assert.Equal(taskResult, asyncValue.Value);
        }

        /// <summary>
        /// Tests that Value property works correctly with null default values for reference types.
        /// Tests with a null default value when task is not completed.
        /// Expected result: Should return null as the default value.
        /// </summary>
        [Fact]
        public void Value_WithNullDefaultValue_ReturnsNull()
        {
            // Arrange
            var tcs = new TaskCompletionSource<string>();
            string defaultValue = null;
            var asyncValue = new AsyncValue<string>(tcs.Task, defaultValue);

            // Act
            var result = asyncValue.Value;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that Value property works correctly with null task results for reference types.
        /// Tests with a completed task that has a null result.
        /// Expected result: Should return the null task result.
        /// </summary>
        [Fact]
        public void Value_WithNullTaskResult_ReturnsNull()
        {
            // Arrange
            string taskResult = null;
            var defaultValue = "default";
            var completedTask = Task.FromResult(taskResult);
            var asyncValue = new AsyncValue<string>(completedTask, defaultValue);

            // Act
            var result = asyncValue.Value;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that Value property works correctly with value types and their default values.
        /// Tests with various value types using their default values.
        /// Expected result: Should return the appropriate default values when task is not completed.
        /// </summary>
        [Theory]
        [InlineData(0, 42)]
        [InlineData(false, true)]
        [InlineData(0.0, 1.5)]
        public void Value_WithValueTypes_ReturnsDefaultValue<T>(T defaultValue, T taskResult)
        {
            // Arrange
            var tcs = new TaskCompletionSource<T>();
            var asyncValue = new AsyncValue<T>(tcs.Task, defaultValue);

            // Act - before task completion
            var resultBeforeCompletion = asyncValue.Value;

            // Assert - should return default value
            Assert.Equal(defaultValue, resultBeforeCompletion);

            // Act - complete task and check again
            tcs.SetResult(taskResult);
            var resultAfterCompletion = asyncValue.Value;

            // Assert - should return task result
            Assert.Equal(taskResult, resultAfterCompletion);
        }

        /// <summary>
        /// Tests that Value property works correctly with complex reference types.
        /// Tests with custom objects as both default value and task result.
        /// Expected result: Should return the appropriate object references.
        /// </summary>
        [Fact]
        public void Value_WithCustomObjects_ReturnsCorrectReferences()
        {
            // Arrange
            var defaultObject = new TestObject { Name = "Default" };
            var taskResultObject = new TestObject { Name = "Result" };
            var tcs = new TaskCompletionSource<TestObject>();
            var asyncValue = new AsyncValue<TestObject>(tcs.Task, defaultObject);

            // Act - before completion
            var resultBefore = asyncValue.Value;

            // Assert - should return default object
            Assert.Same(defaultObject, resultBefore);
            Assert.Equal("Default", resultBefore.Name);

            // Act - complete task
            tcs.SetResult(taskResultObject);
            var resultAfter = asyncValue.Value;

            // Assert - should return task result object
            Assert.Same(taskResultObject, resultAfter);
            Assert.Equal("Result", resultAfter.Name);
        }

        /// <summary>
        /// Helper class for testing with custom reference types.
        /// </summary>
        private class TestObject
        {
            public string Name { get; set; }
        }

        /// <summary>
        /// Tests that the Null property returns a non-null AsyncValue instance with default behavior for value types.
        /// Input: Accessing AsyncValue&lt;int&gt;.Null
        /// Expected: Returns non-null instance with IsRunning=false and Value=0
        /// </summary>
        [Fact]
        public void Null_WithValueType_ReturnsNonNullInstanceWithDefaultValue()
        {
            // Act
            var result = AsyncValue<int>.Null;

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsRunning);
            Assert.Equal(0, result.Value);
        }

        /// <summary>
        /// Tests that the Null property returns a non-null AsyncValue instance with default behavior for reference types.
        /// Input: Accessing AsyncValue&lt;string&gt;.Null
        /// Expected: Returns non-null instance with IsRunning=false and Value=null
        /// </summary>
        [Fact]
        public void Null_WithReferenceType_ReturnsNonNullInstanceWithDefaultValue()
        {
            // Act
            var result = AsyncValue<string>.Null;

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsRunning);
            Assert.Null(result.Value);
        }

        /// <summary>
        /// Tests that the Null property returns a non-null AsyncValue instance with default behavior for custom types.
        /// Input: Accessing AsyncValue&lt;DateTime&gt;.Null
        /// Expected: Returns non-null instance with IsRunning=false and Value=DateTime.MinValue
        /// </summary>
        [Fact]
        public void Null_WithDateTime_ReturnsNonNullInstanceWithDefaultValue()
        {
            // Act
            var result = AsyncValue<DateTime>.Null;

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsRunning);
            Assert.Equal(DateTime.MinValue, result.Value);
        }

        /// <summary>
        /// Tests that the Null property returns a non-null AsyncValue instance with default behavior for boolean types.
        /// Input: Accessing AsyncValue&lt;bool&gt;.Null
        /// Expected: Returns non-null instance with IsRunning=false and Value=false
        /// </summary>
        [Fact]
        public void Null_WithBool_ReturnsNonNullInstanceWithDefaultValue()
        {
            // Act
            var result = AsyncValue<bool>.Null;

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsRunning);
            Assert.False(result.Value);
        }

        /// <summary>
        /// Tests that the Null property returns a non-null AsyncValue instance with default behavior for object types.
        /// Input: Accessing AsyncValue&lt;object&gt;.Null
        /// Expected: Returns non-null instance with IsRunning=false and Value=null
        /// </summary>
        [Fact]
        public void Null_WithObject_ReturnsNonNullInstanceWithDefaultValue()
        {
            // Act
            var result = AsyncValue<object>.Null;

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsRunning);
            Assert.Null(result.Value);
        }

        /// <summary>
        /// Tests that multiple calls to the Null property return different instances.
        /// Input: Multiple accesses to AsyncValue&lt;int&gt;.Null
        /// Expected: Each call returns a different instance (not cached)
        /// </summary>
        [Fact]
        public void Null_MultipleCalls_ReturnsDifferentInstances()
        {
            // Act
            var result1 = AsyncValue<int>.Null;
            var result2 = AsyncValue<int>.Null;

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that the Null property creates an AsyncValue that implements INotifyPropertyChanged correctly.
        /// Input: Accessing AsyncValue&lt;string&gt;.Null
        /// Expected: Returns instance that implements INotifyPropertyChanged
        /// </summary>
        [Fact]
        public void Null_ImplementsINotifyPropertyChanged()
        {
            // Act
            var result = AsyncValue<string>.Null;

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<INotifyPropertyChanged>(result);
        }

        /// <summary>
        /// Tests that the Null property works correctly with generic constraints for different numeric types.
        /// Input: Accessing AsyncValue&lt;T&gt;.Null for various numeric types
        /// Expected: Returns non-null instances with appropriate default values
        /// </summary>
        [Theory]
        [InlineData(typeof(byte), (byte)0)]
        [InlineData(typeof(short), (short)0)]
        [InlineData(typeof(long), (long)0)]
        [InlineData(typeof(float), (float)0)]
        [InlineData(typeof(double), (double)0)]
        [InlineData(typeof(decimal), 0)]
        public void Null_WithNumericTypes_ReturnsCorrectDefaultValues(Type type, object expectedDefault)
        {
            // Arrange & Act
            var method = typeof(AsyncValue<>).MakeGenericType(type).GetProperty("Null");
            var result = method.GetValue(null);

            // Assert
            Assert.NotNull(result);
            var isRunningProperty = result.GetType().GetProperty("IsRunning");
            var valueProperty = result.GetType().GetProperty("Value");

            Assert.False((bool)isRunningProperty.GetValue(result));
            Assert.Equal(expectedDefault, valueProperty.GetValue(result));
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when valueTask parameter is null.
        /// </summary>
        [Fact]
        public void Constructor_WithNullValueTask_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new AsyncValue<string>(null));

            Assert.Equal("valueTask", exception.ParamName);
        }

        /// <summary>
        /// Tests that the constructor properly initializes with a completed task and default value.
        /// The IsRunning should be false immediately and PropertyChanged should be fired for Value.
        /// </summary>
        [Fact]
        public void Constructor_WithCompletedTask_SetsIsRunningFalseAndFiresPropertyChanged()
        {
            // Arrange
            var completedTask = Task.FromResult("test value");
            var propertyChangedEvents = new List<string>();

            // Act
            var asyncValue = new AsyncValue<string>(completedTask, "default");
            asyncValue.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName);

            // Wait a moment to ensure any continuations have executed
            Thread.Sleep(10);

            // Assert
            Assert.False(asyncValue.IsRunning);
            Assert.Contains("Value", propertyChangedEvents);
        }

        /// <summary>
        /// Tests that the constructor properly handles a faulted task.
        /// IsRunning should be false but PropertyChanged should not be fired for Value.
        /// </summary>
        [Fact]
        public void Constructor_WithFaultedTask_SetsIsRunningFalseWithoutValuePropertyChanged()
        {
            // Arrange
            var faultedException = new InvalidOperationException("Test exception");
            var faultedTask = Task.FromException<string>(faultedException);
            var propertyChangedEvents = new List<string>();

            // Act
            var asyncValue = new AsyncValue<string>(faultedTask, "default");
            asyncValue.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName);

            // Wait a moment to ensure any continuations have executed
            Thread.Sleep(10);

            // Assert
            Assert.False(asyncValue.IsRunning);
            Assert.DoesNotContain("Value", propertyChangedEvents);
        }

        /// <summary>
        /// Tests that the constructor properly handles a canceled task.
        /// IsRunning should be false but PropertyChanged should not be fired for Value.
        /// </summary>
        [Fact]
        public void Constructor_WithCanceledTask_SetsIsRunningFalseWithoutValuePropertyChanged()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var canceledTask = Task.FromCanceled<string>(cts.Token);
            var propertyChangedEvents = new List<string>();

            // Act
            var asyncValue = new AsyncValue<string>(canceledTask, "default");
            asyncValue.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName);

            // Wait a moment to ensure any continuations have executed
            Thread.Sleep(10);

            // Assert
            Assert.False(asyncValue.IsRunning);
            Assert.DoesNotContain("Value", propertyChangedEvents);
        }

        /// <summary>
        /// Tests that the constructor properly handles a not-yet-completed task.
        /// IsRunning should initially be true, and continuations should be set up.
        /// </summary>
        [Fact]
        public void Constructor_WithIncompleteTask_LeavesIsRunningTrueInitially()
        {
            // Arrange
            var tcs = new TaskCompletionSource<string>();
            var incompleteTask = tcs.Task;

            // Act
            var asyncValue = new AsyncValue<string>(incompleteTask, "default");

            // Assert
            Assert.True(asyncValue.IsRunning);
        }

        /// <summary>
        /// Tests that when an incomplete task later completes successfully,
        /// IsRunning becomes false and PropertyChanged is fired for Value.
        /// </summary>
        [Fact]
        public async Task Constructor_WhenIncompleteTaskCompletesSuccessfully_UpdatesIsRunningAndFiresPropertyChanged()
        {
            // Arrange
            var tcs = new TaskCompletionSource<string>();
            var propertyChangedEvents = new List<string>();

            var asyncValue = new AsyncValue<string>(tcs.Task, "default");
            asyncValue.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName);

            // Act
            tcs.SetResult("completed value");

            // Wait for continuations to execute
            await Task.Delay(50);

            // Assert
            Assert.False(asyncValue.IsRunning);
            Assert.Contains("IsRunning", propertyChangedEvents);
            Assert.Contains("Value", propertyChangedEvents);
        }

        /// <summary>
        /// Tests that when an incomplete task later faults,
        /// IsRunning becomes false but PropertyChanged is not fired for Value.
        /// </summary>
        [Fact]
        public async Task Constructor_WhenIncompleteTaskFaults_UpdatesIsRunningWithoutValuePropertyChanged()
        {
            // Arrange
            var tcs = new TaskCompletionSource<string>();
            var propertyChangedEvents = new List<string>();

            var asyncValue = new AsyncValue<string>(tcs.Task, "default");
            asyncValue.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName);

            // Act
            tcs.SetException(new InvalidOperationException("Test exception"));

            // Wait for continuations to execute
            await Task.Delay(50);

            // Assert
            Assert.False(asyncValue.IsRunning);
            Assert.Contains("IsRunning", propertyChangedEvents);
            Assert.DoesNotContain("Value", propertyChangedEvents);
        }

        /// <summary>
        /// Tests that the constructor properly stores the default value parameter.
        /// </summary>
        [Theory]
        [InlineData("default string")]
        [InlineData("")]
        [InlineData(null)]
        public void Constructor_WithDefaultValue_StoresDefaultValueCorrectly(string defaultValue)
        {
            // Arrange
            var task = Task.FromResult("task result");

            // Act
            var asyncValue = new AsyncValue<string>(task, defaultValue);

            // Assert - we can't directly access _defaultValue, but we can test indirectly
            // by creating an incomplete task and checking that Value returns the default
            var incompleteTask = new TaskCompletionSource<string>().Task;
            var asyncValueWithIncomplete = new AsyncValue<string>(incompleteTask, defaultValue);

            Assert.Equal(defaultValue, asyncValueWithIncomplete.Value);
        }

        /// <summary>
        /// Tests the constructor behavior with value types to ensure generic constraints work properly.
        /// </summary>
        [Fact]
        public void Constructor_WithValueType_WorksCorrectly()
        {
            // Arrange
            var completedTask = Task.FromResult(42);
            var propertyChangedEvents = new List<string>();

            // Act
            var asyncValue = new AsyncValue<int>(completedTask, 0);
            asyncValue.PropertyChanged += (sender, args) => propertyChangedEvents.Add(args.PropertyName);

            // Wait a moment to ensure any continuations have executed
            Thread.Sleep(10);

            // Assert
            Assert.False(asyncValue.IsRunning);
            Assert.Equal(42, asyncValue.Value);
            Assert.Contains("Value", propertyChangedEvents);
        }

        /// <summary>
        /// Tests the constructor with extreme default values for value types.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_WithValueTypeDefaultValues_HandlesExtremeValues(int defaultValue)
        {
            // Arrange
            var incompleteTask = new TaskCompletionSource<int>().Task;

            // Act
            var asyncValue = new AsyncValue<int>(incompleteTask, defaultValue);

            // Assert
            Assert.Equal(defaultValue, asyncValue.Value);
            Assert.True(asyncValue.IsRunning);
        }

        /// <summary>
        /// Tests that PropertyChanged event handlers can be attached during construction
        /// and receive the appropriate notifications.
        /// </summary>
        [Fact]
        public void Constructor_WithPropertyChangedSubscription_FiresEventsCorrectly()
        {
            // Arrange
            var completedTask = Task.FromResult("test");
            PropertyChangedEventArgs valuePropertyChangedArgs = null;
            PropertyChangedEventArgs isRunningPropertyChangedArgs = null;

            // Act
            var asyncValue = new AsyncValue<string>(completedTask, "default");
            asyncValue.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Value")
                    valuePropertyChangedArgs = args;
                else if (args.PropertyName == "IsRunning")
                    isRunningPropertyChangedArgs = args;
            };

            // Wait for events
            Thread.Sleep(10);

            // Assert
            Assert.NotNull(valuePropertyChangedArgs);
            Assert.Equal("Value", valuePropertyChangedArgs.PropertyName);
            Assert.NotNull(isRunningPropertyChangedArgs);
            Assert.Equal("IsRunning", isRunningPropertyChangedArgs.PropertyName);
        }
    }

    public class AsyncValueExtensionsTests
    {
        /// <summary>
        /// Tests that AsAsyncValue throws ArgumentNullException when valueTask parameter is null.
        /// </summary>
        [Fact]
        public void AsAsyncValue_NullValueTask_ThrowsArgumentNullException()
        {
            // Arrange
            Task<int> nullTask = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => nullTask.AsAsyncValue());
        }

        /// <summary>
        /// Tests that AsAsyncValue throws ArgumentNullException when valueTask parameter is null with explicit default value.
        /// </summary>
        [Fact]
        public void AsAsyncValue_NullValueTaskWithDefaultValue_ThrowsArgumentNullException()
        {
            // Arrange
            Task<string> nullTask = null;
            string defaultValue = "test";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => nullTask.AsAsyncValue(defaultValue));
        }

        /// <summary>
        /// Tests that AsAsyncValue creates AsyncValue successfully with a completed task and default defaultValue.
        /// </summary>
        [Fact]
        public void AsAsyncValue_CompletedTaskWithDefaultValue_CreatesAsyncValue()
        {
            // Arrange
            var completedTask = Task.FromResult(42);

            // Act
            var result = completedTask.AsAsyncValue();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AsyncValue<int>>(result);
        }

        /// <summary>
        /// Tests that AsAsyncValue creates AsyncValue successfully with a completed task and explicit default value.
        /// </summary>
        [Fact]
        public void AsAsyncValue_CompletedTaskWithExplicitDefaultValue_CreatesAsyncValue()
        {
            // Arrange
            var completedTask = Task.FromResult("result");
            var defaultValue = "default";

            // Act
            var result = completedTask.AsAsyncValue(defaultValue);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AsyncValue<string>>(result);
        }

        /// <summary>
        /// Tests that AsAsyncValue creates AsyncValue successfully with different task states and types.
        /// </summary>
        [Theory]
        [InlineData(0, 100)] // int type
        [InlineData("", "default")] // string type
        public void AsAsyncValue_WithDifferentTypesAndValues_CreatesAsyncValue<T>(T taskResult, T defaultValue)
        {
            // Arrange
            var task = Task.FromResult(taskResult);

            // Act
            var result = task.AsAsyncValue(defaultValue);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AsyncValue<T>>(result);
        }

        /// <summary>
        /// Tests that AsAsyncValue creates AsyncValue successfully with a running task.
        /// </summary>
        [Fact]
        public void AsAsyncValue_RunningTask_CreatesAsyncValue()
        {
            // Arrange
            var tcs = new TaskCompletionSource<int>();
            var runningTask = tcs.Task;

            // Act
            var result = runningTask.AsAsyncValue(10);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AsyncValue<int>>(result);

            // Cleanup
            tcs.SetResult(5);
        }

        /// <summary>
        /// Tests that AsAsyncValue creates AsyncValue successfully with a faulted task.
        /// </summary>
        [Fact]
        public void AsAsyncValue_FaultedTask_CreatesAsyncValue()
        {
            // Arrange
            var faultedTask = Task.FromException<string>(new InvalidOperationException("Test exception"));

            // Act
            var result = faultedTask.AsAsyncValue("default");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AsyncValue<string>>(result);
        }

        /// <summary>
        /// Tests that AsAsyncValue creates AsyncValue successfully with a canceled task.
        /// </summary>
        [Fact]
        public void AsAsyncValue_CanceledTask_CreatesAsyncValue()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var canceledTask = Task.FromCanceled<double>(cts.Token);

            // Act
            var result = canceledTask.AsAsyncValue(3.14);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AsyncValue<double>>(result);
        }

        /// <summary>
        /// Tests that AsAsyncValue creates AsyncValue successfully with nullable reference type.
        /// </summary>
        [Fact]
        public void AsAsyncValue_NullableReferenceType_CreatesAsyncValue()
        {
            // Arrange
            var task = Task.FromResult<string>(null);

            // Act
            var result = task.AsAsyncValue();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AsyncValue<string>>(result);
        }

        /// <summary>
        /// Tests that AsAsyncValue creates AsyncValue successfully with value type using explicit default.
        /// </summary>
        [Fact]
        public void AsAsyncValue_ValueTypeWithExplicitDefault_CreatesAsyncValue()
        {
            // Arrange
            var task = Task.FromResult(DateTime.Now);
            var defaultValue = DateTime.MinValue;

            // Act
            var result = task.AsAsyncValue(defaultValue);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AsyncValue<DateTime>>(result);
        }
    }
}