#nullable enable
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
			public TestEventSource EventSource { get; set; }
			public TestSource()
			{
				EventSource = new TestEventSource();
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

		internal class TestEventSource
		{
			readonly WeakEventManager _weakEventManager;

			public TestEventSource()
			{
				_weakEventManager = new WeakEventManager();
			}

			public void FireTestEvent()
			{
				OnTestEvent();
			}

			internal event EventHandler TestEvent
			{
				add { _weakEventManager.AddEventHandler(value); }
				remove { _weakEventManager.RemoveEventHandler(value); }
			}

			void OnTestEvent()
			{
				_weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(TestEvent));
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

		internal class TestSubscriber
		{
			public void Subscribe(TestEventSource source)
			{
				source.TestEvent += SourceOnTestEvent;
			}

			public void Unsubscribe(TestEventSource source)
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

		[Fact]
		public void CanRemoveEventHandler()
		{
			var source = new TestSource();
			int beforeRun = source.Count;
			source.Fire();

			Assert.True(source.Count == 1);
			source.Clean();
			source.Fire();
			Assert.True(source.Count == 1);
		}

		[Fact]
		public void CanRemoveStaticEventHandler()
		{
			int beforeRun = s_count;

			var source = new TestEventSource();
			source.TestEvent += Handler;
			source.TestEvent -= Handler;

			source.FireTestEvent();

			Assert.True(s_count == beforeRun);
		}

		[Fact]
		public void EventHandlerCalled()
		{
			var called = false;

			var source = new TestEventSource();
			source.TestEvent += (sender, args) => { called = true; };

			source.FireTestEvent();

			Assert.True(called);
		}

		[Fact]
		public void FiringEventWithoutHandlerShouldNotThrow()
		{
			var source = new TestEventSource();
			source.FireTestEvent();
		}

		[Fact]
		public void MultipleHandlersCalled()
		{
			var called1 = false;
			var called2 = false;

			var source = new TestEventSource();
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

		[Fact]
		public void RemoveHandlerWithMultipleSubscriptionsRemovesOne()
		{
			int beforeRun = s_count;

			var source = new TestEventSource();
			source.TestEvent += Handler;
			source.TestEvent += Handler;
			source.TestEvent -= Handler;

			source.FireTestEvent();

			Assert.Equal(beforeRun + 1, s_count);
		}

		[Fact]
		public void StaticHandlerShouldRun()
		{
			int beforeRun = s_count;

			var source = new TestEventSource();
			source.TestEvent += Handler;

			source.FireTestEvent();

			Assert.True(s_count > beforeRun);
		}

		[Fact]
		public void VerifySubscriberCanBeCollected_FireEvent()
		{
			WeakReference? wr = null;
			var source = new TestEventSource();
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

		[Fact]
		public void VerifySubscriberCanBeCollected_Unsubscribe()
		{
			WeakReference? wr = null;
			var source = new TestEventSource();
			new Action(() =>
			{
				var ts = new TestSubscriber();
				wr = new WeakReference(ts);
				ts.Subscribe(source);
			})();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			new TestSubscriber().Unsubscribe(source);

			Assert.NotNull(wr);
			Assert.False(wr?.IsAlive);
			Assert.Equal(0, source.EventHandlerCount);
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