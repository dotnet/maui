using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting.Internal;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.Lifecycle)]
	public class AbstractViewHandlerTests
	{
		[Fact]
		public void ConnectAndDisconnectFireAppropriateNumberOfTimes()
		{
			HandlerStub handlerStub = new HandlerStub();
			handlerStub.SetVirtualView(new Maui.Controls.Button());

			Assert.Equal(1, handlerStub.ConnectHandlerCount);
			Assert.Equal(0, handlerStub.DisconnectHandlerCount);

			handlerStub.SetVirtualView(new Maui.Controls.Button());
			handlerStub.SetVirtualView(new Maui.Controls.Button());
			handlerStub.SetVirtualView(new Maui.Controls.Button());
			Assert.Equal(1, handlerStub.ConnectHandlerCount);
			Assert.Equal(0, handlerStub.DisconnectHandlerCount);

			(handlerStub as IViewHandler).DisconnectHandler();
			Assert.Equal(1, handlerStub.ConnectHandlerCount);
			Assert.Equal(1, handlerStub.DisconnectHandlerCount);

			(handlerStub as IViewHandler).DisconnectHandler();
			Assert.Equal(1, handlerStub.ConnectHandlerCount);
			Assert.Equal(1, handlerStub.DisconnectHandlerCount);


			handlerStub.SetVirtualView(new Maui.Controls.Button());
			Assert.Equal(2, handlerStub.ConnectHandlerCount);
			Assert.Equal(1, handlerStub.DisconnectHandlerCount);
			(handlerStub as IViewHandler).DisconnectHandler();
			Assert.Equal(2, handlerStub.ConnectHandlerCount);
			Assert.Equal(2, handlerStub.DisconnectHandlerCount);
		}

		[Fact]
		public void GetRequiredServiceThrowsOnNoContext()
		{
			HandlerStub handlerStub = new HandlerStub();

			Assert.Null((handlerStub as IViewHandler).MauiContext);

			var ex = Assert.Throws<InvalidOperationException>(() => handlerStub.GetRequiredService<IFooService>());

			Assert.Contains("the context", ex.Message, StringComparison.Ordinal);
			Assert.Contains("MauiContext", ex.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void GetRequiredServiceThrowsOnNoServices()
		{
			HandlerStub handlerStub = new HandlerStub();

			handlerStub.SetMauiContext(new InvalidHandlersContextStub());

			Assert.NotNull(handlerStub.MauiContext);
			Assert.Null(handlerStub.MauiContext.Services);

			var ex = Assert.Throws<InvalidOperationException>(() => handlerStub.GetRequiredService<IFooService>());

			Assert.Contains("the service provider", ex.Message, StringComparison.Ordinal);
			Assert.Contains("MauiContext", ex.Message, StringComparison.Ordinal);
		}

		[Fact]
		public void GetRequiredServiceRetrievesService()
		{
			HandlerStub handlerStub = new HandlerStub();

			var collection = new MauiHandlersCollection();
			collection.TryAddSingleton<IMauiHandlersFactory>(new MauiHandlersFactory(collection));
			collection.TryAddSingleton<IFooService, FooService>();

			var provider = new MauiFactory(collection);

			handlerStub.SetMauiContext(new HandlersContextStub(provider));

			Assert.NotNull(handlerStub.MauiContext);
			Assert.NotNull(handlerStub.MauiContext.Services);

			var foo = handlerStub.GetRequiredService<IFooService>();

			Assert.IsType<FooService>(foo);
		}

		[Fact]
		public void SettingVirtualViewOnHandlerRemovesHandlerFromPreviousVirtualView()
		{
			HandlerStub handlerStub = new HandlerStub();
			var button1 = new Maui.Controls.Button();
			var button2 = new Maui.Controls.Button();
			handlerStub.SetVirtualView(button1);
			handlerStub.SetVirtualView(button2);

			Assert.Null(button1.Handler);
		}

		[Fact]
		public void ChainingToLessTypedParentWorks()
		{
			bool wasMapper1Called = false;
			bool wasMapper2Called = false;
			var mapper1 = new PropertyMapper<IView, HandlerStub>
			{
				[nameof(IView.Background)] = (r, v) => wasMapper1Called = true
			};


			var mapper2 = new PropertyMapper<Button, HandlerStub>(mapper1)
			{
				[nameof(IView.Background)] = (r, v) => wasMapper2Called = true
			};

			HandlerStub handlerStub = new HandlerStub(mapper2);
			handlerStub.SetVirtualView(new ButtonStub());
			handlerStub.UpdateValue(nameof(IView.Background));

			Assert.True(wasMapper1Called);
			Assert.False(wasMapper2Called);
		}

		[Fact]
		public void BatchedPropertyUpdatesAreCoalescedInLastOccurrenceOrder()
		{
			var mapped = new List<string>();
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["First"] = (handler, view) => mapped.Add("First"),
				["Second"] = (handler, view) => mapped.Add("Second"),
			};
			var handler = new HandlerStub(mapper);
			var view = new AlwaysBatchingButton();

			handler.SetVirtualView(view);
			mapped.Clear();

			handler.UpdateValue("First");
			handler.UpdateValue("Second");
			handler.UpdateValue("First");

			Assert.Empty(mapped);

			((IPropertyUpdateBatchingHandler)handler).FlushPendingPropertyUpdates();

			Assert.Equal(new[] { "Second", "First" }, mapped);
		}

		[Fact]
		public void BatchedPropertyUpdatesResolveChainedMapperAtFlushTime()
		{
			var mapped = new List<string>();
			var chainedMapper = new PropertyMapper<IView, HandlerStub>
			{
				["Value"] = (handler, view) => mapped.Add("original"),
			};
			var mapper = new PropertyMapper<IView, HandlerStub>(chainedMapper);
			var handler = new HandlerStub(mapper);
			var view = new AlwaysBatchingButton();

			handler.SetVirtualView(view);
			mapped.Clear();

			view.BatchBegin();
			handler.UpdateValue("Value");
			chainedMapper.AppendToMapping("Value", (handler, view) => mapped.Add("customized"));
			view.BatchCommit();

			Assert.Equal(new[] { "original", "customized" }, mapped);
		}

		[Fact]
		public void ReentrantExplicitFlushDuringMapperExecutionDrainsRemainingQueue()
		{
			var mapped = new List<string>();
			HandlerStub handler = null;
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["First"] = (h, view) =>
				{
					mapped.Add("First");

					// Simulate BatchCommit() being reached reentrantly from inside a mapper
					// callback (for example an animation tick or a BatchCommitted/property-
					// changed handler reached synchronously from mapper code), while
					// _mapperExecutionDepth is still > 0 (we're inside InvokeCurrentPropertyMapper
					// for "First"). "Second" is still sitting in the queue at this point.
					((IPropertyUpdateBatchingHandler)handler).FlushPendingPropertyUpdates();
				},
				["Second"] = (h, view) => mapped.Add("Second"),
			};
			handler = new HandlerStub(mapper);
			var view = new AlwaysBatchingButton();

			handler.SetVirtualView(view);
			mapped.Clear();

			handler.UpdateValue("First");
			handler.UpdateValue("Second");
			((IPropertyUpdateBatchingHandler)handler).FlushPendingPropertyUpdates();

			// "Second" must not be silently stranded: here it is drained by the still-active
			// outer flush loop continuing once "First" returns (this specific nested-in-the-
			// same-drain-loop shape is safe even without the fix below, because the loop that
			// invoked "First" keeps running against live, freshly-read queue state). See
			// ReentrantExplicitFlushOutsideActiveDrainLoopDefersUntilDepthUnwinds for the
			// narrower case the fix actually guards: a reentrant commit reached from *outside*
			// any active drain loop while depth is still non-zero.
			Assert.Equal(new[] { "First", "Second" }, mapped);
		}

		[Fact]
		public void CommandMapperReentrantBatchCommitDoesNotThrowOrStrandQueuedUpdates()
		{
			// A public-API-only variant of the reentrancy scenario, reached through a command
			// mapper (Invoke) rather than a property mapper -- e.g. a command handler that
			// synchronously raises an event/animation callback which commits an unrelated,
			// still-open explicit batch on the same view. Invoke() flushes any pending property
			// updates before running the command (the fix for a different review thread), so by
			// the time the reentrant BatchCommit() below is reached the queue already happens to
			// be empty -- this does not discriminate the fix on drained *values* (see
			// ReentrantExplicitFlushOutsideActiveDrainLoopDefersUntilDepthUnwinds for that), but
			// it does exercise the real, non-reflection call path into
			// IPropertyUpdateBatchingHandler.FlushPendingPropertyUpdates() /
			// FlushPendingExplicitUpdatesIfUnwound() from inside InvokeCommandMapper, confirming
			// there is no exception, no recursion, and no stuck batching state afterward.
			var mapped = new List<string>();
			CommandHandlerStub handler = null;
			AlwaysBatchingButton view = null;
			var mapper = new PropertyMapper<IView, CommandHandlerStub>
			{
				["First"] = (h, v) => mapped.Add("First"),
				["Second"] = (h, v) => mapped.Add("Second"),
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>
			{
				["Custom"] = (h, v, args) =>
				{
					mapped.Add("Custom");
					view.BatchCommit();
				},
			};
			handler = new CommandHandlerStub(mapper, commandMapper);
			view = new AlwaysBatchingButton();

			handler.SetVirtualView(view);
			mapped.Clear();

			view.BatchBegin();
			handler.UpdateValue("First");
			handler.UpdateValue("Second");
			handler.Invoke("Custom", null);

			// "First"/"Second" are flushed by Invoke() before "Custom" runs; the reentrant
			// BatchCommit() inside "Custom" must not throw, and must actually turn explicit
			// batching off (matching a normal, non-reentrant commit) rather than leaving it
			// stuck on.
			Assert.Equal(new[] { "First", "Second", "Custom" }, mapped);

			mapped.Clear();
			handler.UpdateValue("First");

			Assert.Equal(new[] { "First" }, mapped);
		}

		[Fact]
		public void ReentrantExplicitFlushOutsideActiveDrainLoopDefersUntilDepthUnwinds()
		{
			// IPropertyUpdateBatchingHandler.FlushPendingPropertyUpdates() must not silently
			// drop a pending queue when it is reached while _mapperExecutionDepth > 0 from
			// *outside* the internal flush loop (for example a command mapper, or the automatic
			// leading-update mapper, that synchronously runs code which commits an unrelated,
			// still-open explicit batch on the same handler). Constructing that exact call
			// stack requires depth > 0 while nothing is actively iterating the pending queue --
			// a shape the other fixes in this PR (flushing before every command and platform-
			// view access) make unreachable through the public API alone. _mapperExecutionDepth
			// and FlushPendingExplicitUpdatesIfUnwound are internal implementation details, so
			// this test drives them directly via reflection to pin down the two halves of the
			// fix: (1) a reentrant commit while depth > 0 defers instead of silently no-op'ing,
			// and (2) the deferred commit actually drains once depth unwinds back to 0.
			var mapped = new List<string>();
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["First"] = (h, view) => mapped.Add("First"),
			};
			var handler = new HandlerStub(mapper);
			var view = new AlwaysBatchingButton();

			handler.SetVirtualView(view);
			mapped.Clear();

			var handlerType = typeof(ElementHandler);
			var depthField = handlerType.GetField("_mapperExecutionDepth", BindingFlags.NonPublic | BindingFlags.Instance);
			var unwindMethod = handlerType.GetMethod("FlushPendingExplicitUpdatesIfUnwound", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.NotNull(depthField);
			Assert.NotNull(unwindMethod);

			handler.UpdateValue("First");
			Assert.Empty(mapped);

			// Simulate being reentrantly reached from inside some other mapper/command
			// invocation (depth > 0) that is not the flush loop draining this same queue.
			depthField.SetValue(handler, 1);
			try
			{
				((IPropertyUpdateBatchingHandler)handler).FlushPendingPropertyUpdates();

				// Must defer, not drop: nothing has run yet, and "First" is still queued.
				Assert.Empty(mapped);
			}
			finally
			{
				depthField.SetValue(handler, 0);
			}

			// Simulate that outer invocation unwinding back to depth 0.
			unwindMethod.Invoke(handler, null);

			Assert.Equal(new[] { "First" }, mapped);
		}

		[Fact]
		public void BatchedPropertyUpdatesProcessReentrantUpdates()
		{
			var mapped = new List<string>();
			HandlerStub handler = null;
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["First"] = (h, view) =>
				{
					mapped.Add("First");
					handler!.UpdateValue("Second");
				},
				["Second"] = (h, view) => mapped.Add("Second"),
			};
			handler = new HandlerStub(mapper);
			var view = new AlwaysBatchingButton();

			handler.SetVirtualView(view);
			mapped.Clear();

			handler.UpdateValue("First");
			((IPropertyUpdateBatchingHandler)handler).FlushPendingPropertyUpdates();

			Assert.Equal(new[] { "First", "Second" }, mapped);
		}

		[Fact]
		public void MapperDependenciesRunSynchronouslyWithoutInterruptingAutomaticBatch()
		{
			var mapped = new List<string>();
			HandlerStub handler = null;
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["Leading"] = (currentHandler, view) =>
				{
					mapped.Add("Leading");
					handler!.UpdateValue("Dependency");
					Assert.NotNull(handler.PlatformView);
				},
				["Dependency"] = (currentHandler, view) => mapped.Add("Dependency"),
				["Trailing"] = (currentHandler, view) => mapped.Add("Trailing"),
			};
			var dispatcher = new QueuedDispatcher();
			handler = CreateHandlerWithDispatcher(mapper, dispatcher);
			var view = new AutomaticBatchingButton();

			handler.SetVirtualView(view);
			mapped.Clear();

			handler.UpdateValue("Leading");
			handler.UpdateValue("Trailing");
			handler.UpdateValue("Trailing");

			Assert.Equal(new[] { "Leading", "Dependency" }, mapped);
			Assert.Equal(1, dispatcher.PendingCount);

			dispatcher.RunNext();

			Assert.Equal(new[] { "Leading", "Dependency", "Trailing" }, mapped);
		}

		[Fact]
		public void BatchCommitFlushesPendingHandlerUpdates()
		{
			var mapCount = 0;
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["Value"] = (handler, view) => mapCount++,
			};
			var handler = new HandlerStub(mapper);
			var view = new AlwaysBatchingButton();

			handler.SetVirtualView(view);
			mapCount = 0;

			view.BatchBegin();
			handler.UpdateValue("Value");
			handler.UpdateValue("Value");

			Assert.Equal(0, mapCount);

			view.BatchCommit();

			Assert.Equal(1, mapCount);
		}

		[Fact]
		public void AutomaticPropertyUpdatesSkipBatchingBookkeepingWhenNoDispatcherIsAvailable()
		{
			// None of the dispatcher-based tests cover the fallback where MauiContext (and so
			// GetOptionalDispatcher()) is null. Deliberately do NOT call SetMauiContext here.
			var mapped = new List<string>();
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["Leading"] = (handler, view) => mapped.Add("Leading"),
				["Trailing"] = (handler, view) => mapped.Add("Trailing"),
			};
			var handler = new HandlerStub(mapper);
			var view = new AutomaticBatchingButton();

			handler.SetVirtualView(view);
			mapped.Clear();

			var pendingViewField = typeof(ElementHandler).GetField("_pendingPropertyUpdateView", BindingFlags.NonPublic | BindingFlags.Instance);
			var leadingFlagField = typeof(ElementHandler).GetField("_hasMappedAutomaticLeadingUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
			var scheduledFlagField = typeof(ElementHandler).GetField("_isPropertyUpdateFlushScheduled", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.NotNull(pendingViewField);
			Assert.NotNull(leadingFlagField);
			Assert.NotNull(scheduledFlagField);

			// With no dispatcher able to defer a flush, every update must take the plain
			// immediate mapper path with no batching bookkeeping at all -- in particular,
			// _pendingPropertyUpdateView must remain the current virtual view throughout.
			// A broken implementation that still enters the leading branch and then relies on
			// SchedulePropertyUpdateFlush's synchronous fallback ends up running a full
			// (empty-queue) flush-and-clear cycle after every single call, which resets
			// _pendingPropertyUpdateView back to null each time even though every applied
			// value is still individually correct -- so asserting only on `mapped` would not
			// catch that regression.
			handler.UpdateValue("Leading");
			Assert.Same(view, pendingViewField.GetValue(handler));
			Assert.False((bool)leadingFlagField.GetValue(handler));
			Assert.False((bool)scheduledFlagField.GetValue(handler));

			handler.UpdateValue("Trailing");
			Assert.Same(view, pendingViewField.GetValue(handler));

			handler.UpdateValue("Trailing");
			Assert.Same(view, pendingViewField.GetValue(handler));

			Assert.Equal(new[] { "Leading", "Trailing", "Trailing" }, mapped);
		}

		class FailingDispatcher : IDispatcher
		{
			public int DispatchAttempts { get; private set; }

			public bool IsDispatchRequired => false;

			public bool Dispatch(Action action)
			{
				DispatchAttempts++;
				return false;
			}

			public bool DispatchDelayed(TimeSpan delay, Action action) =>
				Dispatch(action);

			public IDispatcherTimer CreateTimer() =>
				throw new NotSupportedException();
		}

		[Fact]
		public void AutomaticPropertyUpdatesSkipBatchingBookkeepingWhenDispatchFails()
		{
			// Covers the other half of the fallback: a real MauiContext/dispatcher is present,
			// but Dispatch(...) itself returns false every time it is called (e.g. the
			// dispatcher has shut down). Behavior must match the no-dispatcher case exactly.
			var mapped = new List<string>();
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["Leading"] = (handler, view) => mapped.Add("Leading"),
				["Trailing"] = (handler, view) => mapped.Add("Trailing"),
			};
			var dispatcher = new FailingDispatcher();
			var handler = CreateHandlerWithDispatcher(mapper, dispatcher);
			var view = new AutomaticBatchingButton();

			handler.SetVirtualView(view);
			mapped.Clear();

			var pendingViewField = typeof(ElementHandler).GetField("_pendingPropertyUpdateView", BindingFlags.NonPublic | BindingFlags.Instance);
			var scheduledFlagField = typeof(ElementHandler).GetField("_isPropertyUpdateFlushScheduled", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.NotNull(pendingViewField);
			Assert.NotNull(scheduledFlagField);

			handler.UpdateValue("Leading");
			Assert.Same(view, pendingViewField.GetValue(handler));
			Assert.False((bool)scheduledFlagField.GetValue(handler));

			handler.UpdateValue("Trailing");
			Assert.Same(view, pendingViewField.GetValue(handler));

			handler.UpdateValue("Trailing");
			Assert.Same(view, pendingViewField.GetValue(handler));

			Assert.Equal(new[] { "Leading", "Trailing", "Trailing" }, mapped);
			// Every leading update legitimately retries Dispatch (the dispatcher could recover
			// later); it must not be permanently given up on after the first failure.
			Assert.Equal(3, dispatcher.DispatchAttempts);
		}

		[Fact]
		public void AutomaticPropertyUpdatesApplyLeadingValueAndCoalesceTrailingValue()
		{
			var mapCount = 0;
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["Value"] = (handler, view) => mapCount++,
			};
			var dispatcher = new QueuedDispatcher();
			var handler = CreateHandlerWithDispatcher(mapper, dispatcher);
			var view = new AutomaticBatchingButton();

			handler.SetVirtualView(view);
			mapCount = 0;

			handler.UpdateValue("Value");
			handler.UpdateValue("Value");

			Assert.Equal(1, mapCount);
			Assert.Equal(1, dispatcher.PendingCount);

			dispatcher.RunNext();

			Assert.Equal(2, mapCount);
		}

		[Fact]
		public void AutomaticPropertyUpdatesPreserveTrailingLastOccurrenceOrder()
		{
			var mapped = new List<string>();
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["Leading"] = (handler, view) => mapped.Add("Leading"),
				["First"] = (handler, view) => mapped.Add("First"),
				["Second"] = (handler, view) => mapped.Add("Second"),
			};
			var dispatcher = new QueuedDispatcher();
			var handler = CreateHandlerWithDispatcher(mapper, dispatcher);
			var view = new AutomaticBatchingButton();

			handler.SetVirtualView(view);
			mapped.Clear();

			handler.UpdateValue("Leading");
			handler.UpdateValue("First");
			handler.UpdateValue("Second");
			handler.UpdateValue("First");

			dispatcher.RunNext();

			Assert.Equal(new[] { "Leading", "Second", "First" }, mapped);
		}

		[Fact]
		public void TrailingMapperDependenciesPreserveRequestedOrder()
		{
			var mapped = new List<string>();
			HandlerStub handler = null;
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["Leading"] = (currentHandler, view) => mapped.Add("Leading"),
				["Formatting"] = (currentHandler, view) =>
				{
					mapped.Add("Formatting");
					handler!.UpdateValue("LineHeight");
					handler.UpdateValue("Alignment");
				},
				["Alignment"] = (currentHandler, view) => mapped.Add("Alignment"),
				["LineHeight"] = (currentHandler, view) => mapped.Add("LineHeight"),
			};
			var dispatcher = new QueuedDispatcher();
			handler = CreateHandlerWithDispatcher(mapper, dispatcher);
			var view = new AutomaticBatchingButton();

			handler.SetVirtualView(view);
			mapped.Clear();

			handler.UpdateValue("Leading");
			handler.UpdateValue("Formatting");

			dispatcher.RunNext();

			Assert.Equal(new[] { "Leading", "Formatting", "LineHeight", "Alignment" }, mapped);
		}

		[Fact]
		public void MeasureInvalidationsRemainSynchronousDuringAutomaticBatch()
		{
			var mapped = new List<string>();
			var mapper = new PropertyMapper<IView, CommandHandlerStub>
			{
				["Value"] = (handler, view) => mapped.Add("Value"),
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.InvalidateMeasure)] = (handler, view, args) => mapped.Add("InvalidateMeasure"),
			};
			var dispatcher = new QueuedDispatcher();
			var handler = CreateCommandHandlerWithDispatcher(mapper, commandMapper, dispatcher);
			var view = new AutomaticBatchingButton();

			handler.SetVirtualView(view);
			mapped.Clear();

			handler.UpdateValue("Value");
			handler.UpdateValue("Value");
			handler.Invoke(nameof(IView.InvalidateMeasure), null);
			handler.Invoke(nameof(IView.InvalidateMeasure), null);

			// The first InvalidateMeasure flushes the second (queued) "Value" update before
			// running, so both mapped "Value" entries appear before either InvalidateMeasure.
			Assert.Equal(new[] { "Value", "Value", "InvalidateMeasure", "InvalidateMeasure" }, mapped);
			Assert.Equal(1, dispatcher.PendingCount);

			dispatcher.RunNext();

			// The queue was already drained by the InvalidateMeasure flush above, so the stale
			// dispatcher callback safely no-ops instead of replaying "Value" a third time.
			Assert.Equal(new[] { "Value", "Value", "InvalidateMeasure", "InvalidateMeasure" }, mapped);
			Assert.Equal(0, dispatcher.PendingCount);
		}

		[Fact]
		public void MeasureInvalidationsRemainSynchronousDuringExplicitBatch()
		{
			var mapped = new List<string>();
			var mapper = new PropertyMapper<IView, CommandHandlerStub>
			{
				["Value"] = (handler, view) => mapped.Add("Value"),
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.InvalidateMeasure)] = (handler, view, args) => mapped.Add("InvalidateMeasure"),
			};
			var handler = new CommandHandlerStub(mapper, commandMapper);
			var view = new AlwaysBatchingButton();

			handler.SetVirtualView(view);
			mapped.Clear();

			view.BatchBegin();
			handler.UpdateValue("Value");
			handler.Invoke(nameof(IView.InvalidateMeasure), null);
			handler.Invoke(nameof(IView.InvalidateMeasure), null);

			// The first InvalidateMeasure flushes the queued "Value" update before running, even
			// though the explicit batch is still open.
			Assert.Equal(new[] { "Value", "InvalidateMeasure", "InvalidateMeasure" }, mapped);

			view.BatchCommit();

			// The queue was already drained by the InvalidateMeasure flush above, so committing
			// the batch has nothing left to flush.
			Assert.Equal(new[] { "Value", "InvalidateMeasure", "InvalidateMeasure" }, mapped);
		}

		[Fact]
		public void ReentrantMeasureInvalidationRunsSynchronously()
		{
			var mapped = new List<string>();
			CommandHandlerStub handler = null;
			var mapper = new PropertyMapper<IView, CommandHandlerStub>
			{
				["First"] = (currentHandler, view) =>
				{
					mapped.Add("First");
					handler!.Invoke(nameof(IView.InvalidateMeasure), null);
					handler.UpdateValue("Second");
				},
				["Second"] = (currentHandler, view) => mapped.Add("Second"),
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.InvalidateMeasure)] = (currentHandler, view, args) => mapped.Add("InvalidateMeasure"),
			};
			handler = new CommandHandlerStub(mapper, commandMapper);
			var view = new AlwaysBatchingButton();

			handler.SetVirtualView(view);
			mapped.Clear();

			handler.UpdateValue("First");
			((IPropertyUpdateBatchingHandler)handler).FlushPendingPropertyUpdates();

			Assert.Equal(new[] { "First", "InvalidateMeasure", "Second" }, mapped);
		}

		[Fact]
		public void NonMeasureCommandFlushesPendingPropertiesFirst()
		{
			var mapped = new List<string>();
			var mapper = new PropertyMapper<IView, CommandHandlerStub>
			{
				["Value"] = (handler, view) => mapped.Add("Value"),
			};
			var commandMapper = new CommandMapper<IView, IViewHandler>
			{
				["Command"] = (handler, view, args) => mapped.Add("Command"),
			};
			var dispatcher = new QueuedDispatcher();
			var handler = CreateCommandHandlerWithDispatcher(mapper, commandMapper, dispatcher);
			var view = new AutomaticBatchingButton();

			handler.SetVirtualView(view);
			mapped.Clear();

			handler.UpdateValue("Value");
			handler.UpdateValue("Value");
			handler.Invoke("Command", null);

			Assert.Equal(new[] { "Value", "Value", "Command" }, mapped);
		}

		[Fact]
		public void PlatformViewAccessDoesNotReplaySynchronousMeasureInvalidation()
		{
			var invalidationCount = 0;
			var mapper = new PropertyMapper<IView, CommandHandlerStub>();
			var commandMapper = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.InvalidateMeasure)] = (handler, view, args) => invalidationCount++,
			};
			var dispatcher = new QueuedDispatcher();
			var handler = CreateCommandHandlerWithDispatcher(mapper, commandMapper, dispatcher);
			var view = new AutomaticBatchingButton();

			handler.SetVirtualView(view);
			invalidationCount = 0;
			handler.Invoke(nameof(IView.InvalidateMeasure), null);

			Assert.Equal(1, invalidationCount);
			Assert.NotNull(handler.PlatformView);
			Assert.Equal(1, invalidationCount);
		}

		[Fact]
		public void DisconnectDoesNotReplaySynchronousMeasureInvalidation()
		{
			var invalidationCount = 0;
			var mapper = new PropertyMapper<IView, CommandHandlerStub>();
			var commandMapper = new CommandMapper<IView, IViewHandler>
			{
				[nameof(IView.InvalidateMeasure)] = (handler, view, args) => invalidationCount++,
			};
			var dispatcher = new QueuedDispatcher();
			var handler = CreateCommandHandlerWithDispatcher(mapper, commandMapper, dispatcher);
			var view = new AutomaticBatchingButton();

			handler.SetVirtualView(view);
			invalidationCount = 0;
			handler.Invoke(nameof(IView.InvalidateMeasure), null);
			((IViewHandler)handler).DisconnectHandler();

			Assert.Equal(1, invalidationCount);
			Assert.Equal(0, dispatcher.PendingCount);
		}

		[Fact]
		public void PlatformViewAccessFlushesAutomaticPropertyUpdates()
		{
			var mapCount = 0;
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["Value"] = (handler, view) => mapCount++,
			};
			var dispatcher = new QueuedDispatcher();
			var handler = CreateHandlerWithDispatcher(mapper, dispatcher);
			var view = new AutomaticBatchingButton();

			handler.SetVirtualView(view);
			mapCount = 0;

			handler.UpdateValue("Value");

			Assert.Equal(1, mapCount);
			Assert.NotNull(handler.PlatformView);
			Assert.Equal(1, mapCount);

			dispatcher.RunNext();

			Assert.Equal(1, mapCount);
		}

		[Fact]
		public void BaseElementHandlerTypedPlatformViewAccessFlushesPendingPropertyUpdates()
		{
			// ElementHandlerOfT/ViewHandler/ViewHandlerOfT all shadow PlatformView with `new`,
			// not `override` -- member lookup for a shadowed (non-virtual) property is resolved
			// by the STATIC type of the reference at compile time, not the runtime type. A
			// caller whose reference is statically typed as the base ElementHandler must still
			// observe the flush barrier; it must not silently bypass it by binding to a base
			// property that lacks the flush.
			var mapCount = 0;
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["Value"] = (handler, view) => mapCount++,
			};
			var dispatcher = new QueuedDispatcher();
			var handler = CreateHandlerWithDispatcher(mapper, dispatcher);
			var view = new AutomaticBatchingButton();

			handler.SetVirtualView(view);
			mapCount = 0;

			handler.UpdateValue("Value");
			handler.UpdateValue("Value");

			Assert.Equal(1, mapCount);

			// Access PlatformView through a reference whose static type is the base
			// ElementHandler, not the concrete HandlerStub/ViewHandler<,> type.
			ElementHandler baseTypedHandler = handler;
			Assert.NotNull(baseTypedHandler.PlatformView);

			Assert.Equal(2, mapCount);
		}

		[Fact]
		public void PendingDispatcherCallbackFlushesUpdatesQueuedAfterBarrier()
		{
			var mapCount = 0;
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["Value"] = (handler, view) => mapCount++,
			};
			var dispatcher = new QueuedDispatcher();
			var handler = CreateHandlerWithDispatcher(mapper, dispatcher);
			var view = new AutomaticBatchingButton();

			handler.SetVirtualView(view);
			mapCount = 0;

			handler.UpdateValue("Value");
			handler.UpdateValue("Value");
			Assert.NotNull(handler.PlatformView);

			handler.UpdateValue("Value");
			handler.UpdateValue("Value");

			Assert.Equal(3, mapCount);
			// The PlatformView barrier flush above resets the scheduling flag along with the
			// queue, so the third UpdateValue's leading update is free to schedule a genuine new
			// dispatcher callback: there are now two pending callbacks -- the stale one enqueued
			// before the barrier flush, and the new one. Without resetting the flag, the stale
			// flag would block the new schedule, leaving the fourth (queued) update stranded on
			// a callback that was already consumed by the barrier flush.
			Assert.Equal(2, dispatcher.PendingCount);

			dispatcher.RunNext();

			Assert.Equal(4, mapCount);
			Assert.Equal(1, dispatcher.PendingCount);

			// The remaining stale callback still runs safely against the now-empty queue: it
			// does not reschedule or re-map anything.
			dispatcher.RunNext();

			Assert.Equal(4, mapCount);
			Assert.Equal(0, dispatcher.PendingCount);
		}

		[Fact]
		public void ScheduledAutomaticFlushDoesNotInterruptExplicitBatch()
		{
			var mapped = new List<string>();
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["First"] = (handler, view) => mapped.Add("First"),
				["Second"] = (handler, view) => mapped.Add("Second"),
			};
			var dispatcher = new QueuedDispatcher();
			var handler = CreateHandlerWithDispatcher(mapper, dispatcher);
			var view = new AutomaticBatchingButton();

			handler.SetVirtualView(view);
			mapped.Clear();

			handler.UpdateValue("First");
			view.BatchBegin();
			handler.UpdateValue("Second");

			dispatcher.RunNext();

			Assert.Equal(new[] { "First" }, mapped);

			view.BatchCommit();

			Assert.Equal(new[] { "First", "Second" }, mapped);
		}

		[Fact]
		public void EmptyExplicitBatchClearsPreservedAutomaticLeadingState()
		{
			var mapCount = 0;
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["Value"] = (handler, view) => mapCount++,
			};
			var dispatcher = new QueuedDispatcher();
			var handler = CreateHandlerWithDispatcher(mapper, dispatcher);
			var view = new AutomaticBatchingButton();

			handler.SetVirtualView(view);
			mapCount = 0;

			handler.UpdateValue("Value");
			view.BatchBegin();
			dispatcher.RunNext();
			view.BatchCommit();
			handler.UpdateValue("Value");

			Assert.Equal(2, mapCount);
			Assert.Equal(1, dispatcher.PendingCount);
		}

		[Fact]
		public void DisconnectDiscardsTrailingAutomaticPropertyUpdates()
		{
			var mapCount = 0;
			var mapper = new PropertyMapper<IView, HandlerStub>
			{
				["Value"] = (handler, view) => mapCount++,
			};
			var dispatcher = new QueuedDispatcher();
			var handler = CreateHandlerWithDispatcher(mapper, dispatcher);
			var view = new AutomaticBatchingButton();

			handler.SetVirtualView(view);
			mapCount = 0;

			handler.UpdateValue("Value");
			handler.UpdateValue("Value");
			((IViewHandler)handler).DisconnectHandler();
			dispatcher.RunNext();

			Assert.Equal(1, mapCount);
		}

		static HandlerStub CreateHandlerWithDispatcher(
			PropertyMapper mapper,
			IDispatcher dispatcher)
		{
			var collection = new MauiHandlersCollection();
			collection.TryAddSingleton<IMauiHandlersFactory>(new MauiHandlersFactory(collection));
			collection.TryAddSingleton(dispatcher);

			var handler = new HandlerStub(mapper);
			handler.SetMauiContext(new HandlersContextStub(new MauiFactory(collection)));
			return handler;
		}

		static CommandHandlerStub CreateCommandHandlerWithDispatcher(
			PropertyMapper mapper,
			CommandMapper commandMapper,
			IDispatcher dispatcher)
		{
			var collection = new MauiHandlersCollection();
			collection.TryAddSingleton<IMauiHandlersFactory>(new MauiHandlersFactory(collection));
			collection.TryAddSingleton(dispatcher);

			var handler = new CommandHandlerStub(mapper, commandMapper);
			handler.SetMauiContext(new HandlersContextStub(new MauiFactory(collection)));
			return handler;
		}

		class CommandHandlerStub : ViewHandler<Maui.Controls.Button, object>
		{
			public CommandHandlerStub(PropertyMapper mapper, CommandMapper commandMapper)
				: base(mapper, commandMapper)
			{
			}

			protected override object CreatePlatformView() =>
				new();
		}

		class CustomNativeButton : object
		{

		}

		class CustomButton : Maui.Controls.Button
		{

		}

		class AlwaysBatchingButton : Maui.Controls.Button, IPropertyUpdateBatchingElement
		{
			bool IPropertyUpdateBatchingElement.IsPropertyUpdateBatchingEnabled => true;

			bool IPropertyUpdateBatchingElement.IsAutomaticPropertyUpdateBatchingEnabled => false;

			bool IPropertyUpdateBatchingElement.IsPropertyUpdateBatchingExplicitlyScoped => true;
		}

		class AutomaticBatchingButton : Maui.Controls.Button, IPropertyUpdateBatchingElement
		{
			bool IPropertyUpdateBatchingElement.IsPropertyUpdateBatchingEnabled => true;

			bool IPropertyUpdateBatchingElement.IsAutomaticPropertyUpdateBatchingEnabled => true;

			bool IPropertyUpdateBatchingElement.IsPropertyUpdateBatchingExplicitlyScoped => Batched;
		}

		class QueuedDispatcher : IDispatcher
		{
			readonly Queue<Action> _pending = new();

			public bool IsDispatchRequired => false;

			public int PendingCount => _pending.Count;

			public bool Dispatch(Action action)
			{
				_pending.Enqueue(action);
				return true;
			}

			public bool DispatchDelayed(TimeSpan delay, Action action) =>
				Dispatch(action);

			public IDispatcherTimer CreateTimer() =>
				throw new NotSupportedException();

			public void RunNext() =>
				_pending.Dequeue().Invoke();
		}

		[Fact]
		public void CanUseFactoryForAlternateType()
		{
			HandlerStub.PlatformViewFactory = (h) => { return new CustomNativeButton(); };

			HandlerStub handlerStub = new HandlerStub();
			handlerStub.SetVirtualView(new Maui.Controls.Button());

			Assert.True(handlerStub.PlatformView is CustomNativeButton);
		}

		[Fact]
		public void FactoryCanPuntAndUseOriginalType()
		{
			HandlerStub.PlatformViewFactory = (h) => { return null; };

			HandlerStub handlerStub = new HandlerStub();
			handlerStub.SetVirtualView(new Maui.Controls.Button());

			Assert.NotNull(handlerStub.PlatformView);
			Assert.False(handlerStub.PlatformView is CustomNativeButton);
			Assert.True(handlerStub.PlatformView is object);
		}

		[Fact]
		public void FactoryCanCustomizeBasedOnVirtualView()
		{
			HandlerStub.PlatformViewFactory = (h) =>
			{
				if (h.VirtualView is CustomButton)
				{
					return new CustomNativeButton();
				}

				return null;
			};

			HandlerStub handlerStub = new HandlerStub();
			handlerStub.SetVirtualView(new CustomButton());

			Assert.True(handlerStub.PlatformView is CustomNativeButton);

			HandlerStub handlerStub2 = new HandlerStub();
			handlerStub2.SetVirtualView(new Maui.Controls.Button());

			Assert.True(handlerStub2.PlatformView is object);
		}
	}
}