using System;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class WeakEventManagerTests
	{
		static int s_count;

		static void Handler(object sender, EventArgs eventArgs)
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


			void EventSource_TestEvent(object sender, EventArgs e)
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
		}

		internal class TestSubscriber
		{
			public void Subscribe(TestEventSource source)
			{
				source.TestEvent += SourceOnTestEvent;
			}

			void SourceOnTestEvent(object sender, EventArgs eventArgs)
			{
				Assert.Fail();
			}
		}

		[Test]
		public void AddHandlerWithEmptyEventNameThrowsException()
		{
			var wem = new WeakEventManager();
			Assert.Throws<ArgumentNullException>(() => wem.AddEventHandler((sender, args) => { }, ""));
		}

		[Test]
		public void AddHandlerWithNullEventHandlerThrowsException()
		{
			var wem = new WeakEventManager();
			Assert.Throws<ArgumentNullException>(() => wem.AddEventHandler(null, "test"));
		}

		[Test]
		public void AddHandlerWithNullEventNameThrowsException()
		{
			var wem = new WeakEventManager();
			Assert.Throws<ArgumentNullException>(() => wem.AddEventHandler((sender, args) => { }, null));
		}

		[Test]
		public void CanRemoveEventHandler()
		{
			var source = new TestSource();
			int beforeRun = source.Count;
			source.Fire();

			Assert.IsTrue(source.Count == 1);
			source.Clean();
			source.Fire();
			Assert.IsTrue(source.Count == 1);
		}

		[Test]
		public void CanRemoveStaticEventHandler()
		{
			int beforeRun = s_count;

			var source = new TestEventSource();
			source.TestEvent += Handler;
			source.TestEvent -= Handler;

			source.FireTestEvent();

			Assert.IsTrue(s_count == beforeRun);
		}

		[Test]
		public void EventHandlerCalled()
		{
			var called = false;

			var source = new TestEventSource();
			source.TestEvent += (sender, args) => { called = true; };

			source.FireTestEvent();

			Assert.IsTrue(called);
		}

		[Test]
		public void FiringEventWithoutHandlerShouldNotThrow()
		{
			var source = new TestEventSource();
			source.FireTestEvent();
		}

		[Test]
		public void MultipleHandlersCalled()
		{
			var called1 = false;
			var called2 = false;

			var source = new TestEventSource();
			source.TestEvent += (sender, args) => { called1 = true; };
			source.TestEvent += (sender, args) => { called2 = true; };
			source.FireTestEvent();

			Assert.IsTrue(called1 && called2);
		}

		[Test]
		public void RemoveHandlerWithEmptyEventNameThrowsException()
		{
			var wem = new WeakEventManager();
			Assert.Throws<ArgumentNullException>(() => wem.RemoveEventHandler((sender, args) => { }, ""));
		}

		[Test]
		public void RemoveHandlerWithNullEventHandlerThrowsException()
		{
			var wem = new WeakEventManager();
			Assert.Throws<ArgumentNullException>(() => wem.RemoveEventHandler(null, "test"));
		}

		[Test]
		public void RemoveHandlerWithNullEventNameThrowsException()
		{
			var wem = new WeakEventManager();
			Assert.Throws<ArgumentNullException>(() => wem.RemoveEventHandler((sender, args) => { }, null));
		}

		[Test]
		public void RemovingNonExistentHandlersShouldNotThrow()
		{
			var wem = new WeakEventManager();
			wem.RemoveEventHandler((sender, args) => { }, "fake");
			wem.RemoveEventHandler(Handler, "alsofake");
		}

		[Test]
		public void RemoveHandlerWithMultipleSubscriptionsRemovesOne()
		{
			int beforeRun = s_count;

			var source = new TestEventSource();
			source.TestEvent += Handler;
			source.TestEvent += Handler;
			source.TestEvent -= Handler;

			source.FireTestEvent();

			Assert.AreEqual(beforeRun + 1, s_count);
		}

		[Test]
		public void StaticHandlerShouldRun()
		{
			int beforeRun = s_count;

			var source = new TestEventSource();
			source.TestEvent += Handler;

			source.FireTestEvent();

			Assert.IsTrue(s_count > beforeRun);
		}

		[Test]
		public void VerifySubscriberCanBeCollected()
		{
			WeakReference wr = null;
			var source = new TestEventSource();
			new Action(() =>
			{
				var ts = new TestSubscriber();
				wr = new WeakReference(ts);
				ts.Subscribe(source);
			})();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.IsNotNull(wr);
			Assert.IsFalse(wr.IsAlive);

			// The handler for this calls Assert.Fail, so if the subscriber has not been collected
			// the handler will be called and the test will fail
			source.FireTestEvent();
		}
	}
}