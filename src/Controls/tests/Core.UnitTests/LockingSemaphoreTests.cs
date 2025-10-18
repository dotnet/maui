#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class LockingSemaphoreTests
    {
        /// <summary>
        /// Tests that Release increments the internal count when no waiters are in the queue.
        /// Verifies this by checking that subsequent WaitAsync calls can proceed without blocking.
        /// Expected result: The count is incremented, allowing additional WaitAsync operations.
        /// </summary>
        [Fact]
        public void Release_WhenNoWaiters_IncrementsCount()
        {
            // Arrange
            var semaphore = new LockingSemaphore(1);

            // Act
            semaphore.Release();

            // Assert - Verify count was incremented by checking WaitAsync behavior
            var task1 = semaphore.WaitAsync(CancellationToken.None);
            var task2 = semaphore.WaitAsync(CancellationToken.None);

            Assert.True(task1.IsCompleted);
            Assert.True(task2.IsCompleted);
        }

        /// <summary>
        /// Tests that Release completes a waiting task when waiters exist in the queue.
        /// Creates a waiter by calling WaitAsync on a semaphore with zero count, then releases.
        /// Expected result: The waiting task is completed with result true.
        /// </summary>
        [Fact]
        public void Release_WhenWaitersExist_CompletesWaiter()
        {
            // Arrange
            var semaphore = new LockingSemaphore(0);
            var waitTask = semaphore.WaitAsync(CancellationToken.None);

            Assert.False(waitTask.IsCompleted);

            // Act
            semaphore.Release();

            // Assert
            Assert.True(waitTask.IsCompleted);
            Assert.Equal(TaskStatus.RanToCompletion, waitTask.Status);
            Assert.True(waitTask.Result);
        }

        /// <summary>
        /// Tests that Release completes only one waiter when multiple waiters exist in the queue.
        /// Creates multiple waiters, then calls Release once to verify FIFO behavior.
        /// Expected result: Only the first waiter is completed, others remain waiting.
        /// </summary>
        [Fact]
        public void Release_WhenMultipleWaiters_CompletesOneWaiter()
        {
            // Arrange
            var semaphore = new LockingSemaphore(0);
            var waitTask1 = semaphore.WaitAsync(CancellationToken.None);
            var waitTask2 = semaphore.WaitAsync(CancellationToken.None);
            var waitTask3 = semaphore.WaitAsync(CancellationToken.None);

            Assert.False(waitTask1.IsCompleted);
            Assert.False(waitTask2.IsCompleted);
            Assert.False(waitTask3.IsCompleted);

            // Act
            semaphore.Release();

            // Assert - Only first waiter should be completed (FIFO)
            Assert.True(waitTask1.IsCompleted);
            Assert.False(waitTask2.IsCompleted);
            Assert.False(waitTask3.IsCompleted);
        }

        /// <summary>
        /// Tests Release behavior when called multiple times in sequence.
        /// Verifies that each release either completes a waiter or increments the count appropriately.
        /// Expected result: Each release processes correctly based on current state.
        /// </summary>
        [Fact]
        public void Release_CalledMultipleTimes_ProcessesCorrectly()
        {
            // Arrange
            var semaphore = new LockingSemaphore(0);
            var waitTask1 = semaphore.WaitAsync(CancellationToken.None);
            var waitTask2 = semaphore.WaitAsync(CancellationToken.None);

            // Act - Release twice
            semaphore.Release();
            semaphore.Release();

            // Assert - Both waiters should be completed
            Assert.True(waitTask1.IsCompleted);
            Assert.True(waitTask2.IsCompleted);

            // Additional verification - next WaitAsync should complete immediately due to incremented count
            var immediateTask = semaphore.WaitAsync(CancellationToken.None);
            Assert.True(immediateTask.IsCompleted);
        }

        /// <summary>
        /// Tests Release on a semaphore initialized with zero count and no waiters.
        /// Verifies the count increment path when the waiter queue is empty.
        /// Expected result: Count is incremented, allowing subsequent WaitAsync to complete immediately.
        /// </summary>
        [Fact]
        public void Release_WithZeroInitialCountAndNoWaiters_IncrementsCount()
        {
            // Arrange
            var semaphore = new LockingSemaphore(0);

            // Act
            semaphore.Release();

            // Assert - Next WaitAsync should complete immediately
            var task = semaphore.WaitAsync(CancellationToken.None);
            Assert.True(task.IsCompleted);
        }

        /// <summary>
        /// Tests Release behavior in a mixed scenario with multiple operations.
        /// Combines waiting, releasing, and immediate completions to verify state management.
        /// Expected result: All operations behave correctly maintaining proper semaphore semantics.
        /// </summary>
        [Fact]
        public void Release_MixedScenario_MaintainsCorrectState()
        {
            // Arrange
            var semaphore = new LockingSemaphore(1);

            // Use up the initial count
            var firstTask = semaphore.WaitAsync(CancellationToken.None);
            Assert.True(firstTask.IsCompleted);

            // Create waiters
            var waitTask1 = semaphore.WaitAsync(CancellationToken.None);
            var waitTask2 = semaphore.WaitAsync(CancellationToken.None);
            Assert.False(waitTask1.IsCompleted);
            Assert.False(waitTask2.IsCompleted);

            // Act - Release to complete one waiter
            semaphore.Release();

            // Assert - First waiter completed
            Assert.True(waitTask1.IsCompleted);
            Assert.False(waitTask2.IsCompleted);

            // Act - Release again to increment count (no more waiters)
            semaphore.Release();

            // Assert - Second waiter completed
            Assert.True(waitTask2.IsCompleted);

            // Final verification - next WaitAsync should complete immediately
            var immediateTask = semaphore.WaitAsync(CancellationToken.None);
            Assert.True(immediateTask.IsCompleted);
        }

        /// <summary>
        /// Tests that WaitAsync returns a completed task immediately when semaphore has available resources (count > 0).
        /// Input: Semaphore with initial count > 0, CancellationToken.None.
        /// Expected: Returns completed task, decrements count.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(int.MaxValue)]
        public async Task WaitAsync_WithAvailableCount_ReturnsCompletedTaskImmediately(int initialCount)
        {
            // Arrange
            var semaphore = new LockingSemaphore(initialCount);
            var token = CancellationToken.None;

            // Act
            var task = semaphore.WaitAsync(token);

            // Assert
            Assert.True(task.IsCompleted);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            await task; // Should not throw
        }

        /// <summary>
        /// Tests that WaitAsync returns a pending task when semaphore has no available resources (count = 0).
        /// Input: Semaphore with initial count 0, CancellationToken.None.
        /// Expected: Returns pending task that is not completed.
        /// </summary>
        [Fact]
        public void WaitAsync_WithZeroCount_ReturnsPendingTask()
        {
            // Arrange
            var semaphore = new LockingSemaphore(0);
            var token = CancellationToken.None;

            // Act
            var task = semaphore.WaitAsync(token);

            // Assert
            Assert.False(task.IsCompleted);
            Assert.Equal(TaskStatus.WaitingForActivation, task.Status);
        }

        /// <summary>
        /// Tests that WaitAsync properly decrements the semaphore count when resources are available.
        /// Input: Semaphore with initial count > 1, multiple sequential calls.
        /// Expected: Each call decrements count until reaching zero, then returns pending tasks.
        /// </summary>
        [Fact]
        public async Task WaitAsync_MultipleCallsWithAvailableCount_ProperlyDecrementsCount()
        {
            // Arrange
            var semaphore = new LockingSemaphore(2);
            var token = CancellationToken.None;

            // Act & Assert - First call should complete immediately
            var task1 = semaphore.WaitAsync(token);
            Assert.True(task1.IsCompleted);
            await task1;

            // Act & Assert - Second call should complete immediately
            var task2 = semaphore.WaitAsync(token);
            Assert.True(task2.IsCompleted);
            await task2;

            // Act & Assert - Third call should be pending (count exhausted)
            var task3 = semaphore.WaitAsync(token);
            Assert.False(task3.IsCompleted);
        }

        /// <summary>
        /// Tests that WaitAsync with already cancelled token still creates waiter when no resources available.
        /// Input: Semaphore with count 0, already cancelled CancellationToken.
        /// Expected: Returns pending task that gets cancelled via registered callback.
        /// </summary>
        [Fact]
        public void WaitAsync_WithAlreadyCancelledToken_CreatesPendingTaskThatGetsCancelled()
        {
            // Arrange
            var semaphore = new LockingSemaphore(0);
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var token = cts.Token;

            // Act
            var task = semaphore.WaitAsync(token);

            // Assert
            // The method doesn't immediately check if token is cancelled, so task is created first
            Assert.False(task.IsCompleted); // Initially not completed

            // The cancellation callback should be invoked, eventually cancelling the task
            // We need to wait a bit for the callback to execute
            Thread.Sleep(10);
            Assert.True(task.IsCanceled || task.IsCompleted);
        }

        /// <summary>
        /// Tests that WaitAsync task gets cancelled when token is cancelled after waiter is created.
        /// Input: Semaphore with count 0, token that gets cancelled after waiter creation.
        /// Expected: Pending task gets cancelled when token is cancelled.
        /// </summary>
        [Fact]
        public async Task WaitAsync_TokenCancelledAfterWaiterCreated_CancelsTask()
        {
            // Arrange
            var semaphore = new LockingSemaphore(0);
            var cts = new CancellationTokenSource();

            // Act
            var task = semaphore.WaitAsync(cts.Token);
            Assert.False(task.IsCompleted);

            // Cancel the token
            cts.Cancel();

            // Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => task);
            Assert.True(task.IsCanceled);
        }

        /// <summary>
        /// Tests that WaitAsync with available count works with already cancelled token.
        /// Input: Semaphore with count > 0, already cancelled CancellationToken.
        /// Expected: Returns completed task immediately (cancellation doesn't matter when resource available).
        /// </summary>
        [Fact]
        public async Task WaitAsync_WithAvailableCountAndCancelledToken_ReturnsCompletedTask()
        {
            // Arrange
            var semaphore = new LockingSemaphore(1);
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var token = cts.Token;

            // Act
            var task = semaphore.WaitAsync(token);

            // Assert
            Assert.True(task.IsCompleted);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            await task; // Should not throw
        }

        /// <summary>
        /// Tests that Release can satisfy waiting tasks created by WaitAsync.
        /// Input: Semaphore with count 0, create waiter, then Release.
        /// Expected: Waiter task completes after Release is called.
        /// </summary>
        [Fact]
        public async Task WaitAsync_PendingTaskCompletedByRelease_TaskCompletes()
        {
            // Arrange
            var semaphore = new LockingSemaphore(0);
            var token = CancellationToken.None;

            // Act
            var waitTask = semaphore.WaitAsync(token);
            Assert.False(waitTask.IsCompleted);

            // Release a resource
            semaphore.Release();

            // Assert
            await waitTask; // Should complete successfully
            Assert.True(waitTask.IsCompleted);
            Assert.Equal(TaskStatus.RanToCompletion, waitTask.Status);
        }

        /// <summary>
        /// Tests that multiple waiters are handled in FIFO order.
        /// Input: Semaphore with count 0, create multiple waiters, then multiple releases.
        /// Expected: Waiters complete in the order they were created.
        /// </summary>
        [Fact]
        public async Task WaitAsync_MultipleWaiters_CompletedInFIFOOrder()
        {
            // Arrange
            var semaphore = new LockingSemaphore(0);
            var token = CancellationToken.None;

            // Act - Create multiple waiters
            var waiter1 = semaphore.WaitAsync(token);
            var waiter2 = semaphore.WaitAsync(token);
            var waiter3 = semaphore.WaitAsync(token);

            Assert.False(waiter1.IsCompleted);
            Assert.False(waiter2.IsCompleted);
            Assert.False(waiter3.IsCompleted);

            // Release resources one by one
            semaphore.Release();
            await waiter1; // First waiter should complete
            Assert.True(waiter1.IsCompleted);
            Assert.False(waiter2.IsCompleted);
            Assert.False(waiter3.IsCompleted);

            semaphore.Release();
            await waiter2; // Second waiter should complete
            Assert.True(waiter2.IsCompleted);
            Assert.False(waiter3.IsCompleted);

            semaphore.Release();
            await waiter3; // Third waiter should complete
            Assert.True(waiter3.IsCompleted);
        }

        /// <summary>
        /// Tests WaitAsync behavior at boundary condition of count transitioning from 1 to 0.
        /// Input: Semaphore with count 1, call WaitAsync twice.
        /// Expected: First call completes immediately, second call creates pending task.
        /// </summary>
        [Fact]
        public async Task WaitAsync_CountTransitionFromOneToZero_BehavesCorrectly()
        {
            // Arrange
            var semaphore = new LockingSemaphore(1);
            var token = CancellationToken.None;

            // Act & Assert - First call should complete immediately
            var firstTask = semaphore.WaitAsync(token);
            Assert.True(firstTask.IsCompleted);
            await firstTask;

            // Act & Assert - Second call should create pending task
            var secondTask = semaphore.WaitAsync(token);
            Assert.False(secondTask.IsCompleted);
        }

        /// <summary>
        /// Tests that WaitAsync works correctly with default CancellationToken.None.
        /// Input: Semaphore with various counts, CancellationToken.None.
        /// Expected: Proper behavior regardless of using default token.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void WaitAsync_WithCancellationTokenNone_WorksCorrectly(int initialCount)
        {
            // Arrange
            var semaphore = new LockingSemaphore(initialCount);
            var token = CancellationToken.None;

            // Act
            var task = semaphore.WaitAsync(token);

            // Assert
            if (initialCount > 0)
            {
                Assert.True(task.IsCompleted);
                Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            }
            else
            {
                Assert.False(task.IsCompleted);
                Assert.Equal(TaskStatus.WaitingForActivation, task.Status);
            }
        }

        /// <summary>
        /// Tests that the LockingSemaphore constructor initializes successfully with valid non-negative initial count values.
        /// </summary>
        /// <param name="initialCount">The initial count value to test</param>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(int.MaxValue)]
        public void Constructor_ValidInitialCount_InitializesSuccessfully(int initialCount)
        {
            // Arrange & Act
            var semaphore = new LockingSemaphore(initialCount);

            // Assert
            Assert.NotNull(semaphore);
        }

        /// <summary>
        /// Tests that the LockingSemaphore constructor throws ArgumentOutOfRangeException when initialized with negative initial count values.
        /// </summary>
        /// <param name="initialCount">The negative initial count value to test</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void Constructor_NegativeInitialCount_ThrowsArgumentOutOfRangeException(int initialCount)
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new LockingSemaphore(initialCount));
            Assert.Equal("initialCount", exception.ParamName);
        }

        /// <summary>
        /// Tests that the LockingSemaphore constructor initializes successfully with zero initial count (boundary case).
        /// </summary>
        [Fact]
        public void Constructor_ZeroInitialCount_InitializesSuccessfully()
        {
            // Arrange & Act
            var semaphore = new LockingSemaphore(0);

            // Assert
            Assert.NotNull(semaphore);
        }

        /// <summary>
        /// Tests that the LockingSemaphore constructor initializes successfully with maximum integer initial count (boundary case).
        /// </summary>
        [Fact]
        public void Constructor_MaxIntInitialCount_InitializesSuccessfully()
        {
            // Arrange & Act
            var semaphore = new LockingSemaphore(int.MaxValue);

            // Assert
            Assert.NotNull(semaphore);
        }
    }
}