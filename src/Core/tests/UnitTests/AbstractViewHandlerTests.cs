using System;
using System.Collections.Generic;
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

			Assert.Equal(new[] { "Value", "InvalidateMeasure", "InvalidateMeasure" }, mapped);
			Assert.Equal(1, dispatcher.PendingCount);

			dispatcher.RunNext();

			Assert.Equal(new[] { "Value", "InvalidateMeasure", "InvalidateMeasure", "Value" }, mapped);
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

			Assert.Equal(new[] { "InvalidateMeasure", "InvalidateMeasure" }, mapped);

			view.BatchCommit();

			Assert.Equal(new[] { "InvalidateMeasure", "InvalidateMeasure", "Value" }, mapped);
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
			Assert.Equal(1, dispatcher.PendingCount);

			dispatcher.RunNext();

			Assert.Equal(4, mapCount);
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