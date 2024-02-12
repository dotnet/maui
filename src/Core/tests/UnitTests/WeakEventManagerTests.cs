#nullable enable
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	public class WeakEventManagerTests : INotifyPropertyChanged
	{
		static int s_count;
		readonly WeakEventManager _propertyChangedWeakEventManager = new WeakEventManager();

		static void Handler(object? sender, EventArgs eventArgs)
		{
			s_count++;
		}

		internal class TestSource
		{
			public int Count = 0;
			public ITestEventSource EventSource { get; private set; }
			public TestSource(Type sourceType)
			{
				var source = Activator.CreateInstance(sourceType) as ITestEventSource;
				Assert.NotNull(source);
				EventSource = source;
				EventSource.TestEvent += EventSource_TestEvent;
			}
			public void Clean()
			{
				EventSource.TestEvent -= EventSource_TestEvent;
			}

			public void Fire()
			{
				EventSource.FireTestEvent();
			}


			void EventSource_TestEvent(object? sender, EventArgs e)
			{
				Count++;
			}
		}

		internal interface ITestEventSource
		{
			public void FireTestEvent();

			public event EventHandler<EventArgs> TestEvent;

			public int EventHandlerCount { get; }
		}

		/// <summary>
		/// Tests WeakEventManager
		/// </summary>
		internal class TestEventManagerSource : ITestEventSource
		{
			readonly WeakEventManager _weakEventManager = new();

			public void FireTestEvent()
			{
				_weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(TestEvent));
			}

			public event EventHandler<EventArgs> TestEvent
			{
				add { _weakEventManager.AddEventHandler(value); }
				remove { _weakEventManager.RemoveEventHandler(value); }
			}

			public int EventHandlerCount
			{
				get
				{
					// Access private members:
					// Dictionary<string, List<Subscription>> eventHandlers = WeakEventManager._eventHandlers;
					var flags = BindingFlags.NonPublic | BindingFlags.Instance;
					var eventHandlers = _weakEventManager.GetType().GetField("_eventHandlers", flags)?.GetValue(_weakEventManager) as IDictionary;
					Assert.NotNull(eventHandlers);
					return eventHandlers.Values.Cast<IList>().Sum(l => l.Count);
				}
			}
		}

		/// <summary>
		/// Tests WeakEventHandler
		/// </summary>
		internal class TestEventHandlerSource : ITestEventSource
		{
			readonly WeakEventHandler<EventArgs> _weakEventManager = new();

			public void FireTestEvent()
			{
				_weakEventManager.HandleEvent(this, EventArgs.Empty);
			}

			public event EventHandler<EventArgs> TestEvent
			{
				add { _weakEventManager.AddEventHandler(value); }
				remove { _weakEventManager.RemoveEventHandler(value); }
			}

			public int EventHandlerCount
			{
				get
				{
					// Access private members:
					// HashSet<Subscriptions> _subscriptions;
					var flags = BindingFlags.NonPublic | BindingFlags.Instance;
					var subscriptions = _weakEventManager.GetType().GetField("_subscriptions", flags)?.GetValue(_weakEventManager) as IEnumerable;
					Assert.NotNull(subscriptions);
					return subscriptions.Cast<object>().Count();
				}
			}
		}

		internal class TestSubscriber
		{
			public void Subscribe(ITestEventSource source)
			{
				source.TestEvent += SourceOnTestEvent;
			}

			public void Unsubscribe(ITestEventSource source)
			{
				source.TestEvent -= SourceOnTestEvent;
			}

			void SourceOnTestEvent(object? sender, EventArgs eventArgs)
			{
				throw new Exception("Fail");
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged
		{
			add => _propertyChangedWeakEventManager.AddEventHandler(value);
			remove => _propertyChangedWeakEventManager.RemoveEventHandler(value);
		}

		[Fact]
		public void AddHandlerWithEmptyEventNameThrowsException()
		{
			var wem = new WeakEventManager();
			Assert.Throws<ArgumentNullException>(() => wem.AddEventHandler((EventHandler)((sender, args) => { }), ""));
		}

		[Fact]
		public void AddHandlerWithNullEventHandlerThrowsException()
		{
			var wem = new WeakEventManager();
			Assert.Throws<ArgumentNullException>(() => wem.AddEventHandler(null, "test"));
		}

		[Fact]
		public void AddHandlerWithNullEventNameThrowsException()
		{
			var wem = new WeakEventManager();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
			Assert.Throws<ArgumentNullException>(() => wem.AddEventHandler((EventHandler)((sender, args) => { }), null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
		}

		[Theory]
		[InlineData(typeof(TestEventHandlerSource))]
		[InlineData(typeof(TestEventManagerSource))]
		public void CanRemoveEventHandler(Type sourceType)
		{
			var source = new TestSource(sourceType);
			source.Fire();

			Assert.True(source.Count == 1);
			source.Clean();
			source.Fire();
			Assert.True(source.Count == 1);
		}

		[Theory]
		[InlineData(typeof(TestEventHandlerSource))]
		[InlineData(typeof(TestEventManagerSource))]
		public void CanRemoveStaticEventHandler(Type sourceType)
		{
			int beforeRun = s_count;

			var source = Activator.CreateInstance(sourceType) as ITestEventSource;
			Assert.NotNull(source);
			source.TestEvent += Handler;
			source.TestEvent -= Handler;

			source.FireTestEvent();

			Assert.True(s_count == beforeRun);
		}

		[Theory]
		[InlineData(typeof(TestEventHandlerSource))]
		[InlineData(typeof(TestEventManagerSource))]
		public void EventHandlerCalled(Type sourceType)
		{
			var called = false;

			var source = Activator.CreateInstance(sourceType) as ITestEventSource;
			Assert.NotNull(source);
			source.TestEvent += (sender, args) => { called = true; };

			source.FireTestEvent();

			Assert.True(called);
		}

		[Theory]
		[InlineData(typeof(TestEventHandlerSource))]
		[InlineData(typeof(TestEventManagerSource))]
		public void FiringEventWithoutHandlerShouldNotThrow(Type sourceType)
		{
			var source = Activator.CreateInstance(sourceType) as ITestEventSource;
			Assert.NotNull(source);
			source.FireTestEvent();
		}

		[Theory]
		[InlineData(typeof(TestEventHandlerSource))]
		[InlineData(typeof(TestEventManagerSource))]
		public void MultipleHandlersCalled(Type sourceType)
		{
			var called1 = false;
			var called2 = false;

			var source = Activator.CreateInstance(sourceType) as ITestEventSource;
			Assert.NotNull(source);
			source.TestEvent += (sender, args) => { called1 = true; };
			source.TestEvent += (sender, args) => { called2 = true; };
			source.FireTestEvent();

			Assert.True(called1 && called2);
		}

		[Fact]
		public void RemoveHandlerWithEmptyEventNameThrowsException()
		{
			var wem = new WeakEventManager();
			Assert.Throws<ArgumentNullException>(() => wem.RemoveEventHandler((EventHandler)((sender, args) => { }), ""));
		}

		[Fact]
		public void RemoveHandlerWithNullEventHandlerThrowsException()
		{
			var wem = new WeakEventManager();
			Assert.Throws<ArgumentNullException>(() => wem.RemoveEventHandler(null, "test"));
		}

		[Fact]
		public void RemoveHandlerWithNullEventNameThrowsException()
		{
			var wem = new WeakEventManager();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
			Assert.Throws<ArgumentNullException>(() => wem.RemoveEventHandler((EventHandler)((sender, args) => { }), null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
		}

		[Fact]
		public void RemovingNonExistentHandlersShouldNotThrow()
		{
			var wem = new WeakEventManager();
			wem.RemoveEventHandler((EventHandler)((sender, args) => { }), "fake");
			wem.RemoveEventHandler((Action<object?, EventArgs>)Handler, "alsofake");
		}

		[Theory]
		[InlineData(typeof(TestEventHandlerSource))]
		[InlineData(typeof(TestEventManagerSource))]
		public void RemoveHandlerWithMultipleSubscriptionsRemovesOne(Type sourceType)
		{
			int beforeRun = s_count;

			var source = Activator.CreateInstance(sourceType) as ITestEventSource;
			Assert.NotNull(source);
			source.TestEvent += Handler;
			source.TestEvent += Handler;
			source.TestEvent -= Handler;

			source.FireTestEvent();

			Assert.Equal(beforeRun + 1, s_count);
		}

		[Theory]
		[InlineData(typeof(TestEventHandlerSource))]
		[InlineData(typeof(TestEventManagerSource))]
		public void StaticHandlerShouldRun(Type sourceType)
		{
			int beforeRun = s_count;

			var source = Activator.CreateInstance(sourceType) as ITestEventSource;
			Assert.NotNull(source);
			source.TestEvent += Handler;

			source.FireTestEvent();

			Assert.True(s_count > beforeRun);
		}

		[Theory]
		[InlineData(typeof(TestEventHandlerSource))]
		[InlineData(typeof(TestEventManagerSource))]
		public void VerifySubscriberCanBeCollected_FireEvent(Type sourceType)
		{
			WeakReference? wr = null;
			var source = Activator.CreateInstance(sourceType) as ITestEventSource;
			Assert.NotNull(source);
			new Action(() =>
			{
				var ts = new TestSubscriber();
				wr = new WeakReference(ts);
				ts.Subscribe(source);
			})();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.NotNull(wr);
			Assert.False(wr?.IsAlive);

			// The handler for this calls Assert.Fail, so if the subscriber has not been collected
			// the handler will be called and the test will fail
			source.FireTestEvent();
			Assert.Equal(0, source.EventHandlerCount);
		}

		[Theory]
		[InlineData(typeof(TestEventHandlerSource))]
		[InlineData(typeof(TestEventManagerSource))]
		public async Task VerifySubscriberCanBeCollected_Unsubscribe(Type sourceType)
		{
			WeakReference wr;
			var source = Activator.CreateInstance(sourceType) as ITestEventSource;
			Assert.NotNull(source);

			{
				var ts = new TestSubscriber();
				wr = new WeakReference(ts);
				ts.Subscribe(source);
			}

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			Assert.False(wr.IsAlive);

			new TestSubscriber().Unsubscribe(source);
			Assert.Equal(0, source.EventHandlerCount);
		}

		[Theory]
		[InlineData(typeof(TestEventHandlerSource))]
		[InlineData(typeof(TestEventManagerSource))]
		public async Task EventFiresAfterCollection(Type sourceType)
		{
			bool fired = false;

			var source = Activator.CreateInstance(sourceType) as ITestEventSource;
			Assert.NotNull(source);

			// Use a scope so nothing holds onto handler
			{
				EventHandler<EventArgs> handler = (sender, args) => fired = true;
				source.TestEvent += handler;
			}

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			source.FireTestEvent();
			Assert.True(fired);
		}

		[Fact]
		public void VerifyPropertyChanged()
		{
			//Arrange
			PropertyChanged += HandleDelegateTest;
			bool didEventFire = false;

			void HandleDelegateTest(object? sender, PropertyChangedEventArgs e)
			{
				Assert.NotNull(sender);
				Assert.Equal(this.GetType(), sender?.GetType());

				Assert.NotNull(e);

				didEventFire = true;
				PropertyChanged -= HandleDelegateTest;
			}

			//Act
			_propertyChangedWeakEventManager.HandleEvent(this, new PropertyChangedEventArgs("Test"), nameof(PropertyChanged));

			//Assert
			Assert.True(didEventFire);
		}
	}
}