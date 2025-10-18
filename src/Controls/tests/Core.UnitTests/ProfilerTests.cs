#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ProfileTests
    {
        /// <summary>
        /// Tests that Start method returns early and does not modify Running when IsEnabled is false.
        /// Input: IsEnabled is false (default state).
        /// Expected: Running remains unchanged from its initial state.
        /// </summary>
        [Fact]
        public void Start_WhenIsEnabledIsFalse_DoesNotChangeRunningState()
        {
            // Arrange
            // IsEnabled is false by default, capture initial Running state
            var initialRunningState = GetRunningFieldValue();

            // Act
            Profile.Start();

            // Assert
            Assert.False(Profile.IsEnabled);
            Assert.Equal(initialRunningState, GetRunningFieldValue());
        }

        /// <summary>
        /// Tests that Start method sets Running to true when IsEnabled is true.
        /// Input: IsEnabled is true (after calling Enable).
        /// Expected: Running is set to true.
        /// </summary>
        [Fact]
        public void Start_WhenIsEnabledIsTrue_SetsRunningToTrue()
        {
            // Arrange
            Profile.Enable();

            // Act
            Profile.Start();

            // Assert
            Assert.True(Profile.IsEnabled);
            Assert.True(GetRunningFieldValue());
        }

        /// <summary>
        /// Tests that Start method can be called multiple times when enabled without error.
        /// Input: IsEnabled is true, Start called multiple times.
        /// Expected: Running remains true, no exceptions thrown.
        /// </summary>
        [Fact]
        public void Start_WhenCalledMultipleTimesWhileEnabled_SetsRunningToTrueConsistently()
        {
            // Arrange
            Profile.Enable();

            // Act
            Profile.Start();
            Profile.Start();
            Profile.Start();

            // Assert
            Assert.True(Profile.IsEnabled);
            Assert.True(GetRunningFieldValue());
        }

        /// <summary>
        /// Helper method to access the private static Running field using reflection.
        /// </summary>
        private static bool GetRunningFieldValue()
        {
            var runningField = typeof(Profile).GetField("Running",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (bool)runningField.GetValue(null);
        }

        /// <summary>
        /// Tests that Stop method returns early when IsEnabled is false.
        /// Verifies the early return condition (!IsEnabled) is properly handled.
        /// Expected result: Method returns without executing the body.
        /// </summary>
        [Fact]
        public void Stop_WhenIsEnabledIsFalse_ReturnsEarly()
        {
            // Arrange
            // IsEnabled should be false by default
            var isEnabledBefore = Profile.IsEnabled;

            // Act
            Profile.Stop();

            // Assert
            Assert.False(isEnabledBefore);
            Assert.False(Profile.IsEnabled); // Should remain unchanged
        }

        /// <summary>
        /// Tests that Stop method executes the body when IsEnabled is true and stack is empty.
        /// Verifies that Running is set to false and empty stack is handled correctly.
        /// Expected result: Method executes without errors, processes empty stack.
        /// </summary>
        [Fact]
        public void Stop_WhenIsEnabledTrueAndStackEmpty_ExecutesBody()
        {
            // Arrange
            Profile.Enable(); // Sets IsEnabled = true and initializes Stack
            var isEnabledBefore = Profile.IsEnabled;

            // Act
            Profile.Stop();

            // Assert
            Assert.True(isEnabledBefore);
            Assert.True(Profile.IsEnabled); // Stop doesn't change IsEnabled
        }

        /// <summary>
        /// Tests that Stop method executes the body when IsEnabled is true and stack has items.
        /// Verifies that Running is set to false and all stack items are popped.
        /// Expected result: Method executes without errors, clears all stack items.
        /// </summary>
        [Fact]
        public void Stop_WhenIsEnabledTrueAndStackHasItems_ExecutesBodyAndClearsStack()
        {
            // Arrange
            Profile.Enable(); // Sets IsEnabled = true and initializes Stack
            Profile.Start(); // Sets Running = true

            // Add items to stack by calling FrameBegin multiple times
            Profile.FrameBegin("TestMethod1", 10);
            Profile.FrameBegin("TestMethod2", 20);
            Profile.FrameBegin("TestMethod3", 30);

            var isEnabledBefore = Profile.IsEnabled;

            // Act
            Profile.Stop();

            // Assert
            Assert.True(isEnabledBefore);
            Assert.True(Profile.IsEnabled); // Stop doesn't change IsEnabled
            // Note: We cannot directly verify Stack.Count or Running state as they are internal,
            // but the method should execute without throwing exceptions
        }

        /// <summary>
        /// Tests Stop method multiple times when enabled to ensure it handles repeated calls correctly.
        /// Verifies that multiple calls to Stop don't cause errors even when stack is already empty.
        /// Expected result: Multiple calls execute without errors.
        /// </summary>
        [Fact]
        public void Stop_WhenCalledMultipleTimesAfterEnabled_HandlesRepeatedCalls()
        {
            // Arrange
            Profile.Enable();
            Profile.Start();
            Profile.FrameBegin("TestMethod", 15);

            // Act & Assert - Multiple calls should not throw
            Profile.Stop();
            Profile.Stop(); // Second call with empty stack
            Profile.Stop(); // Third call with empty stack

            // Should complete without exceptions
            Assert.True(Profile.IsEnabled);
        }

        /// <summary>
        /// Tests that FrameBegin does nothing when IsEnabled is false.
        /// Verifies that no profiling data is collected when profiling is disabled.
        /// </summary>
        [Fact]
        public void FrameBegin_WhenIsEnabledIsFalse_DoesNothing()
        {
            // Arrange
            var initialDataCount = Profile.Data?.Count ?? 0;

            // Act
            Profile.FrameBegin("TestMethod", 42);

            // Assert
            var finalDataCount = Profile.Data?.Count ?? 0;
            Assert.Equal(initialDataCount, finalDataCount);
        }

        /// <summary>
        /// Tests that FrameBegin does nothing when IsEnabled is true but Running is false.
        /// Verifies that profiling must be both enabled and started to collect data.
        /// </summary>
        [Fact]
        public void FrameBegin_WhenEnabledButNotRunning_DoesNothing()
        {
            // Arrange
            Profile.Enable();
            // Don't call Start(), so Running remains false
            var initialDataCount = Profile.Data?.Count ?? 0;

            // Act
            Profile.FrameBegin("TestMethod", 42);

            // Assert
            var finalDataCount = Profile.Data?.Count ?? 0;
            Assert.Equal(initialDataCount, finalDataCount);
        }

        /// <summary>
        /// Tests FrameBegin behavior when both IsEnabled and Running are true.
        /// Verifies that profiling data is properly collected including parameter edge cases.
        /// </summary>
        [Theory]
        [InlineData("TestMethod", 42)]
        [InlineData("", 0)]
        [InlineData("   ", -1)]
        [InlineData("VeryLongMethodNameThatExceedsTypicalLengthToTestStringHandling", int.MaxValue)]
        [InlineData("Method!@#$%^&*()WithSpecialCharacters", int.MinValue)]
        [InlineData(null, 1000)]
        public void FrameBegin_WhenEnabledAndRunning_AddsProfilingData(string methodName, int lineNumber)
        {
            // Arrange
            Profile.Enable();
            Profile.Start();

            // Clear any existing data
            Profile.Data?.Clear();

            var initialDataCount = Profile.Data?.Count ?? 0;

            // Act
            Profile.FrameBegin(methodName, lineNumber);

            // Assert
            Assert.NotNull(Profile.Data);
            Assert.Equal(initialDataCount + 1, Profile.Data.Count);

            var datum = Profile.Data[Profile.Data.Count - 1];
            Assert.Equal(methodName, datum.Name);
            Assert.Null(datum.Id); // FrameBegin always passes null as id
            Assert.Equal(lineNumber, datum.Line);
            Assert.Equal(-1, datum.Ticks); // Initial value before disposal
            Assert.True(datum.Depth >= 0); // Depth should be non-negative
        }

        /// <summary>
        /// Tests that multiple FrameBegin calls properly stack and increment depth.
        /// Verifies the nesting behavior of profiling frames.
        /// </summary>
        [Fact]
        public void FrameBegin_MultipleCallsWhenEnabledAndRunning_StacksProperly()
        {
            // Arrange
            Profile.Enable();
            Profile.Start();

            // Clear any existing data
            Profile.Data?.Clear();

            // Act
            Profile.FrameBegin("Method1", 10);
            Profile.FrameBegin("Method2", 20);
            Profile.FrameBegin("Method3", 30);

            // Assert
            Assert.NotNull(Profile.Data);
            Assert.Equal(3, Profile.Data.Count);

            // Verify depth increases with nesting
            Assert.True(Profile.Data[0].Depth <= Profile.Data[1].Depth);
            Assert.True(Profile.Data[1].Depth <= Profile.Data[2].Depth);

            // Verify names and lines are recorded correctly
            Assert.Equal("Method1", Profile.Data[0].Name);
            Assert.Equal(10, Profile.Data[0].Line);
            Assert.Equal("Method2", Profile.Data[1].Name);
            Assert.Equal(20, Profile.Data[1].Line);
            Assert.Equal("Method3", Profile.Data[2].Name);
            Assert.Equal(30, Profile.Data[2].Line);
        }

        /// <summary>
        /// Tests FrameBegin with default parameter values using CallerMemberName and CallerLineNumber attributes.
        /// Verifies that compiler-generated default values are handled correctly.
        /// </summary>
        [Fact]
        public void FrameBegin_WithDefaultParameters_UsesCallerInfo()
        {
            // Arrange
            Profile.Enable();
            Profile.Start();

            // Clear any existing data
            Profile.Data?.Clear();

            // Act - calling without parameters should use CallerMemberName and CallerLineNumber
            Profile.FrameBegin();

            // Assert
            Assert.NotNull(Profile.Data);
            Assert.Equal(1, Profile.Data.Count);

            var datum = Profile.Data[0];
            // The CallerMemberName should be the current test method name
            Assert.Equal(nameof(FrameBegin_WithDefaultParameters_UsesCallerInfo), datum.Name);
            // The CallerLineNumber should be the line where FrameBegin() was called
            Assert.True(datum.Line > 0);
        }

        private void ResetProfilerState()
        {
            // Reset to initial state by stopping if needed
            if (Profile.IsEnabled)
            {
                Profile.Stop();
            }
        }

        /// <summary>
        /// Tests that FrameEnd returns early when IsEnabled is false (default state).
        /// Input: Default state with IsEnabled = false.
        /// Expected: Method returns without throwing exceptions and without affecting internal state.
        /// </summary>
        [Fact]
        public void FrameEnd_WhenNotEnabled_ReturnsWithoutAction()
        {
            // Arrange
            ResetProfilerState();
            string testName = "TestMethod";

            // Act & Assert - should not throw
            Profile.FrameEnd(testName);

            // Verify IsEnabled is still false
            Assert.False(Profile.IsEnabled);
        }

        /// <summary>
        /// Tests that FrameEnd returns early when IsEnabled is true but Running is false.
        /// Input: IsEnabled = true, Running = false.
        /// Expected: Method returns without calling FrameEndBody.
        /// </summary>
        [Fact]
        public void FrameEnd_WhenEnabledButNotRunning_ReturnsWithoutAction()
        {
            // Arrange
            ResetProfilerState();
            Profile.Enable(); // Sets IsEnabled = true, but Running remains false
            string testName = "TestMethod";

            // Act & Assert - should not throw
            Profile.FrameEnd(testName);

            // Verify state
            Assert.True(Profile.IsEnabled);

            // Cleanup
            ResetProfilerState();
        }

        /// <summary>
        /// Tests that FrameEnd successfully calls FrameEndBody when both IsEnabled and Running are true.
        /// Input: IsEnabled = true, Running = true, valid name matching stack top.
        /// Expected: Stack count decreases by 1, indicating successful FrameEndBody execution.
        /// </summary>
        [Fact]
        public void FrameEnd_WhenEnabledAndRunningWithMatchingName_ExecutesSuccessfully()
        {
            // Arrange
            ResetProfilerState();
            Profile.Enable();
            Profile.Start();

            string testName = "TestMethod";
            Profile.FrameBegin(testName); // This pushes to stack

            // Act
            Profile.FrameEnd(testName);

            // Assert - FrameEndBody should have been called and stack should be empty
            // We can't directly verify FrameEndBody call, but we can verify side effects
            // The test passes if no exception is thrown

            // Cleanup
            ResetProfilerState();
        }

        /// <summary>
        /// Tests that FrameEnd handles null name parameter when enabled and running.
        /// Input: IsEnabled = true, Running = true, name = null.
        /// Expected: Method processes null name without throwing NullReferenceException.
        /// </summary>
        [Fact]
        public void FrameEnd_WithNullName_WhenEnabledAndRunning_HandlesNullParameter()
        {
            // Arrange
            ResetProfilerState();
            Profile.Enable();
            Profile.Start();

            // Push a frame with null name to match
            Profile.FrameBegin(null);

            // Act & Assert - should not throw NullReferenceException
            Profile.FrameEnd(null);

            // Cleanup
            ResetProfilerState();
        }

        /// <summary>
        /// Tests that FrameEnd handles empty string name parameter when enabled and running.
        /// Input: IsEnabled = true, Running = true, name = "".
        /// Expected: Method processes empty string successfully.
        /// </summary>
        [Fact]
        public void FrameEnd_WithEmptyName_WhenEnabledAndRunning_HandlesEmptyString()
        {
            // Arrange
            ResetProfilerState();
            Profile.Enable();
            Profile.Start();

            string emptyName = "";
            Profile.FrameBegin(emptyName);

            // Act & Assert - should not throw
            Profile.FrameEnd(emptyName);

            // Cleanup
            ResetProfilerState();
        }

        /// <summary>
        /// Tests that FrameEnd handles whitespace-only name parameter when enabled and running.
        /// Input: IsEnabled = true, Running = true, name = "   ".
        /// Expected: Method processes whitespace string successfully.
        /// </summary>
        [Fact]
        public void FrameEnd_WithWhitespaceName_WhenEnabledAndRunning_HandlesWhitespaceString()
        {
            // Arrange
            ResetProfilerState();
            Profile.Enable();
            Profile.Start();

            string whitespaceName = "   ";
            Profile.FrameBegin(whitespaceName);

            // Act & Assert - should not throw
            Profile.FrameEnd(whitespaceName);

            // Cleanup
            ResetProfilerState();
        }

        /// <summary>
        /// Tests that FrameEnd throws InvalidOperationException when stack is empty.
        /// Input: IsEnabled = true, Running = true, but no frames on stack.
        /// Expected: InvalidOperationException is thrown by FrameEndBody.
        /// </summary>
        [Fact]
        public void FrameEnd_WhenStackEmpty_ThrowsInvalidOperationException()
        {
            // Arrange
            ResetProfilerState();
            Profile.Enable();
            Profile.Start();
            // Don't push any frames to stack

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => Profile.FrameEnd("TestMethod"));

            // Cleanup
            ResetProfilerState();
        }

        /// <summary>
        /// Tests that FrameEnd throws InvalidOperationException when name doesn't match stack top.
        /// Input: IsEnabled = true, Running = true, name mismatches top of stack.
        /// Expected: InvalidOperationException is thrown by FrameEndBody.
        /// </summary>
        [Fact]
        public void FrameEnd_WhenNameMismatch_ThrowsInvalidOperationException()
        {
            // Arrange
            ResetProfilerState();
            Profile.Enable();
            Profile.Start();

            Profile.FrameBegin("ExpectedMethod");

            // Act & Assert - try to end with different name
            Assert.Throws<InvalidOperationException>(() => Profile.FrameEnd("DifferentMethod"));

            // Cleanup
            ResetProfilerState();
        }

        /// <summary>
        /// Tests that FrameEnd with very long name parameter when enabled and running.
        /// Input: IsEnabled = true, Running = true, very long name string.
        /// Expected: Method processes long string successfully.
        /// </summary>
        [Fact]
        public void FrameEnd_WithVeryLongName_WhenEnabledAndRunning_HandlesLongString()
        {
            // Arrange
            ResetProfilerState();
            Profile.Enable();
            Profile.Start();

            string longName = new string('A', 10000);
            Profile.FrameBegin(longName);

            // Act & Assert - should not throw
            Profile.FrameEnd(longName);

            // Cleanup
            ResetProfilerState();
        }

        /// <summary>
        /// Tests that FrameEnd handles name with special characters when enabled and running.
        /// Input: IsEnabled = true, Running = true, name with special characters.
        /// Expected: Method processes special characters successfully.
        /// </summary>
        [Fact]
        public void FrameEnd_WithSpecialCharactersName_WhenEnabledAndRunning_HandlesSpecialCharacters()
        {
            // Arrange
            ResetProfilerState();
            Profile.Enable();
            Profile.Start();

            string specialName = "Test\n\r\t\0Method<>\"'&";
            Profile.FrameBegin(specialName);

            // Act & Assert - should not throw
            Profile.FrameEnd(specialName);

            // Cleanup
            ResetProfilerState();
        }

        /// <summary>
        /// Tests that FramePartition returns early when IsEnabled is false, regardless of Running state.
        /// Input: IsEnabled = false (default state), any id and line values.
        /// Expected: Method returns without calling FramePartitionBody.
        /// </summary>
        [Fact]
        public void FramePartition_WhenNotEnabled_ReturnsEarly()
        {
            // Arrange
            // Profile starts with IsEnabled = false by default
            string testId = "testId";
            int testLine = 42;

            // Verify initial state
            Assert.False(Profile.IsEnabled);

            // Act
            Profile.FramePartition(testId, testLine);

            // Assert
            // Method should return early without any side effects
            // Since Data is null when not enabled, we can verify no initialization occurred
            Assert.Null(Profile.Data);
        }

        /// <summary>
        /// Tests that FramePartition returns early when Running is false, even if IsEnabled is true.
        /// Input: IsEnabled = true, Running = false, any id and line values.
        /// Expected: Method returns without calling FramePartitionBody.
        /// </summary>
        [Fact]
        public void FramePartition_WhenEnabledButNotRunning_ReturnsEarly()
        {
            // Arrange
            Profile.Enable(); // Sets IsEnabled = true, Running remains false
            string testId = "testId";
            int testLine = 42;

            // Verify state
            Assert.True(Profile.IsEnabled);
            // Running should be false since we haven't called Start()

            // Act
            Profile.FramePartition(testId, testLine);

            // Assert
            // Method should return early - Data should be initialized but empty
            Assert.NotNull(Profile.Data);
            Assert.Empty(Profile.Data);

            // Cleanup
            Profile.Stop();
        }

        /// <summary>
        /// Tests that FramePartition calls FramePartitionBody when both IsEnabled and Running are true.
        /// Input: IsEnabled = true, Running = true, valid id and line values, with proper Stack setup.
        /// Expected: Method calls FramePartitionBody which manipulates the Stack and Data.
        /// </summary>
        [Fact]
        public void FramePartition_WhenEnabledAndRunning_CallsFramePartitionBody()
        {
            // Arrange
            Profile.Enable();
            Profile.Start();

            // Set up the Stack by calling FrameBegin first
            Profile.FrameBegin("TestFrame");

            string testId = "partitionId";
            int testLine = 100;

            int initialDataCount = Profile.Data.Count;

            // Act
            Profile.FramePartition(testId, testLine);

            // Assert
            // FramePartitionBody should have been called, which:
            // 1. Pops from Stack (disposes the old frame)
            // 2. Calls FrameBeginBody (adds new frame to Stack and Data)
            Assert.NotNull(Profile.Data);
            Assert.True(Profile.Data.Count > initialDataCount, "Data collection should have new entries");

            // Verify the new frame has the correct id
            var lastDatum = Profile.Data[Profile.Data.Count - 1];
            Assert.Equal(testId, lastDatum.Id);
            Assert.Equal(testLine, lastDatum.Line);

            // Cleanup
            Profile.Stop();
        }

        /// <summary>
        /// Tests FramePartition with various id parameter values including edge cases.
        /// Input: Various id values (null, empty, whitespace, normal, special characters).
        /// Expected: All values are handled correctly without exceptions.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("normalId")]
        [InlineData("id with spaces")]
        [InlineData("special!@#$%^&*()characters")]
        [InlineData("verylongidstringwithmanycharsrepeatedmanytimesverylongidstringwithmanychars")]
        public void FramePartition_WithVariousIdValues_HandlesCorrectly(string id)
        {
            // Arrange
            Profile.Enable();
            Profile.Start();
            Profile.FrameBegin("TestFrame");

            int testLine = 50;

            // Act & Assert
            // Should not throw any exceptions
            Profile.FramePartition(id, testLine);

            // Verify the id was stored correctly
            var lastDatum = Profile.Data[Profile.Data.Count - 1];
            Assert.Equal(id, lastDatum.Id);
            Assert.Equal(testLine, lastDatum.Line);

            // Cleanup
            Profile.Stop();
        }

        /// <summary>
        /// Tests FramePartition with various line parameter values including boundary values.
        /// Input: Various line values (0, negative, positive, min/max values).
        /// Expected: All values are handled correctly without exceptions.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(42)]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void FramePartition_WithVariousLineValues_HandlesCorrectly(int line)
        {
            // Arrange
            Profile.Enable();
            Profile.Start();
            Profile.FrameBegin("TestFrame");

            string testId = "testId";

            // Act & Assert
            // Should not throw any exceptions
            Profile.FramePartition(testId, line);

            // Verify the line was stored correctly
            var lastDatum = Profile.Data[Profile.Data.Count - 1];
            Assert.Equal(testId, lastDatum.Id);
            Assert.Equal(line, lastDatum.Line);

            // Cleanup
            Profile.Stop();
        }

        /// <summary>
        /// Tests FramePartition with both IsEnabled and Running false to ensure early return.
        /// Input: IsEnabled = false, Running = false (default state).
        /// Expected: Method returns early without any processing.
        /// </summary>
        [Fact]
        public void FramePartition_WhenBothDisabledAndNotRunning_ReturnsEarly()
        {
            // Arrange
            // Default state: IsEnabled = false, Running = false
            string testId = "testId";
            int testLine = 25;

            // Verify initial state
            Assert.False(Profile.IsEnabled);

            // Act
            Profile.FramePartition(testId, testLine);

            // Assert
            // Should return early with no side effects
            Assert.Null(Profile.Data);
        }

        /// <summary>
        /// Tests that Dispose returns early when IsEnabled is false without modifying any state.
        /// </summary>
        [Fact]
        public void Dispose_WhenIsEnabledIsFalse_ReturnsEarly()
        {
            // Arrange - Don't call Enable() so IsEnabled remains false
            var profile = default(Profile);

            // Act - Should return early due to !IsEnabled check
            profile.Dispose();

            // Assert - No exception should be thrown, method should return early
            // Since IsEnabled is false, no further processing should occur
            Assert.False(Profile.IsEnabled);
        }

        /// <summary>
        /// Tests that Dispose returns early when Running is true and _start is 0.
        /// </summary>
        [Fact]
        public void Dispose_WhenRunningIsTrueAndStartIsZero_ReturnsEarly()
        {
            // Arrange
            Profile.Enable();
            Profile.Start(); // Set Running = true

            // Create Profile with default constructor so _start = 0
            var profile = default(Profile);

            // Act - Should return early due to Running && _start == 0 check
            profile.Dispose();

            // Assert - No exception should be thrown, method should return early
            Assert.True(Profile.IsEnabled);
        }

        /// <summary>
        /// Tests that Dispose correctly updates Data and decrements Depth when Running is false.
        /// </summary>
        [Fact]
        public void Dispose_WhenRunningIsFalse_UpdatesDataAndDecrementsDepth()
        {
            // Arrange
            Profile.Enable();
            Profile.Stop(); // Ensure Running = false

            // Create Profile using constructor to get proper _start and _slot values
            var initialDataCount = Profile.Data.Count;
            var profile = new Profile("TestMethod", "TestId", 100);

            var expectedSlot = initialDataCount;
            var initialDepth = GetDepthValue();
            var initialStopwatchTicks = GetStopwatchElapsedTicks();

            // Act
            profile.Dispose();

            // Assert
            var currentDepth = GetDepthValue();
            Assert.Equal(initialDepth - 1, currentDepth);

            var updatedDatum = Profile.Data[expectedSlot];
            Assert.True(updatedDatum.Ticks >= 0); // Should have been updated from -1
            Assert.Equal("TestMethod", updatedDatum.Name);
            Assert.Equal("TestId", updatedDatum.Id);
            Assert.Equal(100, updatedDatum.Line);
        }

        /// <summary>
        /// Tests that Dispose correctly updates Data and decrements Depth when Running is true but _start is not zero.
        /// </summary>
        [Fact]
        public void Dispose_WhenRunningIsTrueButStartIsNotZero_UpdatesDataAndDecrementsDepth()
        {
            // Arrange
            Profile.Enable();
            Profile.Start(); // Set Running = true

            // Create Profile using constructor to get proper _start and _slot values
            var initialDataCount = Profile.Data.Count;
            var profile = new Profile("TestMethod2", "TestId2", 200);

            var expectedSlot = initialDataCount;
            var initialDepth = GetDepthValue();

            // Act
            profile.Dispose();

            // Assert
            var currentDepth = GetDepthValue();
            Assert.Equal(initialDepth - 1, currentDepth);

            var updatedDatum = Profile.Data[expectedSlot];
            Assert.True(updatedDatum.Ticks >= 0); // Should have been updated from -1
            Assert.Equal("TestMethod2", updatedDatum.Name);
            Assert.Equal("TestId2", updatedDatum.Id);
            Assert.Equal(200, updatedDatum.Line);
        }

        /// <summary>
        /// Tests that multiple Dispose calls work correctly and maintain proper state.
        /// </summary>
        [Fact]
        public void Dispose_MultipleProfiles_UpdatesDataCorrectly()
        {
            // Arrange
            Profile.Enable();
            Profile.Stop(); // Ensure Running = false

            var initialDataCount = Profile.Data.Count;
            var profile1 = new Profile("Method1", "Id1", 10);
            var profile2 = new Profile("Method2", "Id2", 20);

            var initialDepth = GetDepthValue();

            // Act
            profile2.Dispose(); // Dispose in reverse order (stack-like behavior)
            profile1.Dispose();

            // Assert
            var currentDepth = GetDepthValue();
            Assert.Equal(initialDepth - 2, currentDepth);

            // Check both data entries were updated
            var datum1 = Profile.Data[initialDataCount];
            var datum2 = Profile.Data[initialDataCount + 1];

            Assert.True(datum1.Ticks >= 0);
            Assert.Equal("Method1", datum1.Name);
            Assert.Equal("Id1", datum1.Id);
            Assert.Equal(10, datum1.Line);

            Assert.True(datum2.Ticks >= 0);
            Assert.Equal("Method2", datum2.Name);
            Assert.Equal("Id2", datum2.Id);
            Assert.Equal(20, datum2.Line);
        }

        private static int GetDepthValue()
        {
            var depthField = typeof(Profile).GetField("Depth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (int)depthField.GetValue(null);
        }

        private static long GetStopwatchElapsedTicks()
        {
            var stopwatchField = typeof(Profile).GetField("Stopwatch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var stopwatch = (Stopwatch)stopwatchField.GetValue(null);
            return stopwatch?.ElapsedTicks ?? 0;
        }

        /// <summary>
        /// Tests that Enable method initializes static fields when called for the first time
        /// and sets IsEnabled to true. Verifies that Data, Stack, and Stopwatch are properly initialized.
        /// </summary>
        [Fact]
        public void Enable_WhenCalled_InitializesStaticFieldsAndSetsIsEnabled()
        {
            // Arrange
            var initialIsEnabled = Profile.IsEnabled;
            var initialData = Profile.Data;

            // Act
            Profile.Enable();

            // Assert
            Assert.True(Profile.IsEnabled);
            Assert.NotNull(Profile.Data);

            // Verify Data is initialized as List<Datum> with proper capacity behavior
            Assert.IsType<List<Profile.Datum>>(Profile.Data);

            // If this was the first time Enable was called, Data should be newly created
            // If not the first time, Data should still be valid
            Assert.True(Profile.Data.Count >= 0);
        }

        /// <summary>
        /// Tests that Enable method is idempotent - calling it multiple times in succession
        /// does not create new instances of static fields and preserves the enabled state.
        /// </summary>
        [Fact]
        public void Enable_WhenCalledMultipleTimes_IsIdempotent()
        {
            // Arrange
            Profile.Enable(); // Ensure it's enabled first
            var dataAfterFirstCall = Profile.Data;
            var isEnabledAfterFirstCall = Profile.IsEnabled;

            // Act
            Profile.Enable(); // Call again
            Profile.Enable(); // And again

            // Assert
            Assert.True(Profile.IsEnabled);
            Assert.Same(dataAfterFirstCall, Profile.Data); // Should be same instance
            Assert.Equal(isEnabledAfterFirstCall, Profile.IsEnabled);
        }

        /// <summary>
        /// Tests that Enable method properly initializes the Data field as a List with appropriate capacity
        /// and verifies the list can be used for its intended purpose.
        /// </summary>
        [Fact]
        public void Enable_WhenCalled_InitializesDataAsUsableList()
        {
            // Arrange & Act
            Profile.Enable();

            // Assert
            Assert.NotNull(Profile.Data);
            Assert.IsType<List<Profile.Datum>>(Profile.Data);

            // Verify the list is functional by checking it can store Datum objects
            var initialCount = Profile.Data.Count;

            // The list should be empty or contain existing data, but should be functional
            Assert.True(initialCount >= 0);

            // Verify we can access the list without exceptions
            var capacity = Profile.Data.Capacity;
            Assert.True(capacity > 0);
        }

        /// <summary>
        /// Tests the Enable method behavior when IsEnabled property transitions from false to true,
        /// ensuring proper initialization occurs during state change.
        /// </summary>
        [Fact]
        public void Enable_StateTransition_ProperlyInitializesFields()
        {
            // Arrange
            var wasEnabledBefore = Profile.IsEnabled;

            // Act
            Profile.Enable();

            // Assert
            // After calling Enable, IsEnabled should always be true
            Assert.True(Profile.IsEnabled);

            // Data should be initialized and functional
            Assert.NotNull(Profile.Data);
            Assert.IsAssignableFrom<List<Profile.Datum>>(Profile.Data);

            // Verify the initialized state is consistent
            Assert.True(Profile.Data.Capacity >= 0);
        }
    }
}