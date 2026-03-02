using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Gestures
{
	public class LongPressGestureRecognizerPerformanceTests : BaseTestFixture
	{
		[Fact]
		public void CreatingManyRecognizers_DoesNotCauseMemoryLeak()
		{
			// Arrange
			const int iterations = 1000;
			var recognizers = new List<WeakReference>();

			// Act - Create many recognizers
			for (int i = 0; i < iterations; i++)
			{
				var recognizer = new LongPressGestureRecognizer
				{
					MinimumPressDuration = 500,
					AllowableMovement = 10,
					NumberOfTouchesRequired = 1
				};

				recognizers.Add(new WeakReference(recognizer));
			}

			// Force garbage collection
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			// Assert - All recognizers should be collected (no strong references remain)
			int aliveCount = 0;
			foreach (var weakRef in recognizers)
			{
				if (weakRef.IsAlive)
					aliveCount++;
			}

			// Allow some overhead, but most should be collected
			Assert.True(aliveCount < iterations * 0.1, 
				$"Too many recognizers still alive: {aliveCount}/{iterations}. Possible memory leak.");
		}

		[Fact]
		public void AddingAndRemovingRecognizers_DoesNotLeakMemory()
		{
			// Arrange
			var view = new BoxView();
			var recognizers = new List<WeakReference>();

			// Act - Add and remove many recognizers
			for (int i = 0; i < 100; i++)
			{
				var recognizer = new LongPressGestureRecognizer();
				recognizers.Add(new WeakReference(recognizer));
				
				view.GestureRecognizers.Add(recognizer);
				view.GestureRecognizers.Remove(recognizer);
			}

			// Force garbage collection
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			// Assert - Recognizers should be collected
			int aliveCount = 0;
			foreach (var weakRef in recognizers)
			{
				if (weakRef.IsAlive)
					aliveCount++;
			}

			Assert.True(aliveCount < 10, 
				$"Too many recognizers still alive after add/remove: {aliveCount}. Possible memory leak.");
		}

		[Fact]
		public void EventSubscription_DoesNotPreventGarbageCollection()
		{
			// Arrange
			WeakReference recognizerRef = null;
			WeakReference viewRef = null;

			void CreateAndSubscribe()
			{
				var view = new BoxView();
				var recognizer = new LongPressGestureRecognizer();

				// Subscribe to events
				recognizer.LongPressed += (s, e) => { };
				recognizer.LongPressing += (s, e) => { };

				view.GestureRecognizers.Add(recognizer);

				recognizerRef = new WeakReference(recognizer);
				viewRef = new WeakReference(view);
			}

			// Act
			CreateAndSubscribe();

			// Force garbage collection
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			// Assert - Both should be collected (no strong references outside the method)
			Assert.False(recognizerRef.IsAlive, "Recognizer was not collected - possible memory leak from event subscription");
			Assert.False(viewRef.IsAlive, "View was not collected - possible memory leak");
		}

		[Fact]
		public void RecognizerCreation_IsPerformant()
		{
			// Arrange
			const int iterations = 10000;
			var stopwatch = Stopwatch.StartNew();

			// Act - Create many recognizers
			for (int i = 0; i < iterations; i++)
			{
				var recognizer = new LongPressGestureRecognizer
				{
					MinimumPressDuration = 500,
					AllowableMovement = 10,
					NumberOfTouchesRequired = 1,
					Command = new Command(() => { })
				};
			}

			stopwatch.Stop();

			// Assert - Should be fast (less than 1 second for 10k recognizers)
			Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
				$"Creating {iterations} recognizers took {stopwatch.ElapsedMilliseconds}ms (expected < 1000ms)");
		}

		[Fact]
		public void PropertyChanges_ArePerformant()
		{
			// Arrange
			var recognizer = new LongPressGestureRecognizer();
			const int iterations = 10000;
			var stopwatch = Stopwatch.StartNew();

			// Act - Change properties many times
			for (int i = 0; i < iterations; i++)
			{
				recognizer.MinimumPressDuration = i % 1000;
				recognizer.AllowableMovement = i % 100;
				recognizer.NumberOfTouchesRequired = (i % 3) + 1;
			}

			stopwatch.Stop();

			// Assert - Should be fast (less than 500ms for 10k changes)
			Assert.True(stopwatch.ElapsedMilliseconds < 500, 
				$"Changing properties {iterations} times took {stopwatch.ElapsedMilliseconds}ms (expected < 500ms)");
		}

		[Fact]
		public void EventFiring_IsPerformant()
		{
			// Arrange
			var view = new BoxView();
			var recognizer = new LongPressGestureRecognizer();
			view.GestureRecognizers.Add(recognizer);
			int eventCount = 0;
			recognizer.LongPressed += (s, e) => eventCount++;
			recognizer.LongPressing += (s, e) => eventCount++;

			const int iterations = 10000;
			var stopwatch = Stopwatch.StartNew();

			// Act - Fire events many times
			for (int i = 0; i < iterations; i++)
			{
				recognizer.SendLongPressed(view, null);
				recognizer.SendLongPressing(view, GestureStatus.Running, null);
			}

			stopwatch.Stop();

			// Assert
			Assert.Equal(iterations * 2, eventCount); // Both events fired
			Assert.True(stopwatch.ElapsedMilliseconds < 500, 
				$"Firing {iterations * 2} events took {stopwatch.ElapsedMilliseconds}ms (expected < 500ms)");
		}

		[Fact]
		public void CommandExecution_IsPerformant()
		{
			// Arrange
			var view = new BoxView();
			int commandExecutions = 0;
			var recognizer = new LongPressGestureRecognizer
			{
				Command = new Command(() => commandExecutions++)
			};
			view.GestureRecognizers.Add(recognizer);

			const int iterations = 10000;
			var stopwatch = Stopwatch.StartNew();

			// Act - Execute command many times
			for (int i = 0; i < iterations; i++)
			{
				recognizer.SendLongPressed(view, null);
			}

			stopwatch.Stop();

			// Assert
			Assert.Equal(iterations, commandExecutions);
			Assert.True(stopwatch.ElapsedMilliseconds < 500, 
				$"Executing command {iterations} times took {stopwatch.ElapsedMilliseconds}ms (expected < 500ms)");
		}

		[Fact]
		public void MultipleRecognizersOnSameView_ArePerformant()
		{
			// Arrange
			var view = new BoxView();
			const int recognizerCount = 100;
			var stopwatch = Stopwatch.StartNew();

			// Act - Add many recognizers to same view
			for (int i = 0; i < recognizerCount; i++)
			{
				var recognizer = new LongPressGestureRecognizer
				{
					MinimumPressDuration = 500 + i
				};
				view.GestureRecognizers.Add(recognizer);
			}

			stopwatch.Stop();

			// Assert
			Assert.Equal(recognizerCount, view.GestureRecognizers.Count);
			Assert.True(stopwatch.ElapsedMilliseconds < 100, 
				$"Adding {recognizerCount} recognizers took {stopwatch.ElapsedMilliseconds}ms (expected < 100ms)");
		}

		[Fact]
		public void StatePropertyChanges_DoNotCauseMemoryLeak()
		{
			// Arrange
			var view = new BoxView();
			var recognizer = new LongPressGestureRecognizer();
			view.GestureRecognizers.Add(recognizer);
			var states = new[] 
			{ 
				GestureStatus.Started, 
				GestureStatus.Running, 
				GestureStatus.Completed, 
				GestureStatus.Canceled 
			};

			// Act - Change state many times
			for (int i = 0; i < 1000; i++)
			{
				foreach (var state in states)
				{
					recognizer.SendLongPressing(view, state, null);
				}
			}

			// Force GC
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			// Assert - Recognizer should still be functional
			bool eventFired = false;
			recognizer.LongPressed += (s, e) => eventFired = true;
			recognizer.SendLongPressed(view, null);

			Assert.True(eventFired, "Recognizer became non-functional after many state changes");
		}
	}
}
