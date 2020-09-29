using System;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class MessagingCenterTests : BaseTestFixture
	{
		TestSubcriber _subscriber;

		[Test]
		public void SingleSubscriber()
		{
			string sentMessage = null;
			MessagingCenter.Subscribe<MessagingCenterTests, string>(this, "SimpleTest", (sender, args) => sentMessage = args);

			MessagingCenter.Send(this, "SimpleTest", "My Message");

			Assert.That(sentMessage, Is.EqualTo("My Message"));

			MessagingCenter.Unsubscribe<MessagingCenterTests, string>(this, "SimpleTest");
		}

		[Test]
		public void Filter()
		{
			string sentMessage = null;
			MessagingCenter.Subscribe<MessagingCenterTests, string>(this, "SimpleTest", (sender, args) => sentMessage = args, this);

			MessagingCenter.Send(new MessagingCenterTests(), "SimpleTest", "My Message");

			Assert.That(sentMessage, Is.Null);

			MessagingCenter.Send(this, "SimpleTest", "My Message");

			Assert.That(sentMessage, Is.EqualTo("My Message"));

			MessagingCenter.Unsubscribe<MessagingCenterTests, string>(this, "SimpleTest");
		}

		[Test]
		public void MultiSubscriber()
		{
			var sub1 = new object();
			var sub2 = new object();
			string sentMessage1 = null;
			string sentMessage2 = null;
			MessagingCenter.Subscribe<MessagingCenterTests, string>(sub1, "SimpleTest", (sender, args) => sentMessage1 = args);
			MessagingCenter.Subscribe<MessagingCenterTests, string>(sub2, "SimpleTest", (sender, args) => sentMessage2 = args);

			MessagingCenter.Send(this, "SimpleTest", "My Message");

			Assert.That(sentMessage1, Is.EqualTo("My Message"));
			Assert.That(sentMessage2, Is.EqualTo("My Message"));

			MessagingCenter.Unsubscribe<MessagingCenterTests, string>(sub1, "SimpleTest");
			MessagingCenter.Unsubscribe<MessagingCenterTests, string>(sub2, "SimpleTest");
		}

		[Test]
		public void Unsubscribe()
		{
			string sentMessage = null;
			MessagingCenter.Subscribe<MessagingCenterTests, string>(this, "SimpleTest", (sender, args) => sentMessage = args);
			MessagingCenter.Unsubscribe<MessagingCenterTests, string>(this, "SimpleTest");

			MessagingCenter.Send(this, "SimpleTest", "My Message");

			Assert.That(sentMessage, Is.EqualTo(null));
		}

		[Test]
		public void SendWithoutSubscribers()
		{
			Assert.DoesNotThrow(() => MessagingCenter.Send(this, "SimpleTest", "My Message"));
		}

		[Test]
		public void NoArgSingleSubscriber()
		{
			bool sentMessage = false;
			MessagingCenter.Subscribe<MessagingCenterTests>(this, "SimpleTest", sender => sentMessage = true);

			MessagingCenter.Send(this, "SimpleTest");

			Assert.That(sentMessage, Is.True);

			MessagingCenter.Unsubscribe<MessagingCenterTests>(this, "SimpleTest");
		}

		[Test]
		public void NoArgFilter()
		{
			bool sentMessage = false;
			MessagingCenter.Subscribe(this, "SimpleTest", (sender) => sentMessage = true, this);

			MessagingCenter.Send(new MessagingCenterTests(), "SimpleTest");

			Assert.That(sentMessage, Is.False);

			MessagingCenter.Send(this, "SimpleTest");

			Assert.That(sentMessage, Is.True);

			MessagingCenter.Unsubscribe<MessagingCenterTests>(this, "SimpleTest");
		}

		[Test]
		public void NoArgMultiSubscriber()
		{
			var sub1 = new object();
			var sub2 = new object();
			bool sentMessage1 = false;
			bool sentMessage2 = false;
			MessagingCenter.Subscribe<MessagingCenterTests>(sub1, "SimpleTest", (sender) => sentMessage1 = true);
			MessagingCenter.Subscribe<MessagingCenterTests>(sub2, "SimpleTest", (sender) => sentMessage2 = true);

			MessagingCenter.Send(this, "SimpleTest");

			Assert.That(sentMessage1, Is.True);
			Assert.That(sentMessage2, Is.True);

			MessagingCenter.Unsubscribe<MessagingCenterTests>(sub1, "SimpleTest");
			MessagingCenter.Unsubscribe<MessagingCenterTests>(sub2, "SimpleTest");
		}

		[Test]
		public void NoArgUnsubscribe()
		{
			bool sentMessage = false;
			MessagingCenter.Subscribe<MessagingCenterTests>(this, "SimpleTest", (sender) => sentMessage = true);
			MessagingCenter.Unsubscribe<MessagingCenterTests>(this, "SimpleTest");

			MessagingCenter.Send(this, "SimpleTest", "My Message");

			Assert.That(sentMessage, Is.False);
		}

		[Test]
		public void NoArgSendWithoutSubscribers()
		{
			Assert.DoesNotThrow(() => MessagingCenter.Send(this, "SimpleTest"));
		}

		[Test]
		public void ThrowOnNullArgs()
		{
			Assert.Throws<ArgumentNullException>(() => MessagingCenter.Subscribe<MessagingCenterTests, string>(null, "Foo", (sender, args) => { }));
			Assert.Throws<ArgumentNullException>(() => MessagingCenter.Subscribe<MessagingCenterTests, string>(this, null, (sender, args) => { }));
			Assert.Throws<ArgumentNullException>(() => MessagingCenter.Subscribe<MessagingCenterTests, string>(this, "Foo", null));

			Assert.Throws<ArgumentNullException>(() => MessagingCenter.Subscribe<MessagingCenterTests>(null, "Foo", (sender) => { }));
			Assert.Throws<ArgumentNullException>(() => MessagingCenter.Subscribe<MessagingCenterTests>(this, null, (sender) => { }));
			Assert.Throws<ArgumentNullException>(() => MessagingCenter.Subscribe<MessagingCenterTests>(this, "Foo", null));

			Assert.Throws<ArgumentNullException>(() => MessagingCenter.Send<MessagingCenterTests, string>(null, "Foo", "Bar"));
			Assert.Throws<ArgumentNullException>(() => MessagingCenter.Send<MessagingCenterTests, string>(this, null, "Bar"));

			Assert.Throws<ArgumentNullException>(() => MessagingCenter.Send<MessagingCenterTests>(null, "Foo"));
			Assert.Throws<ArgumentNullException>(() => MessagingCenter.Send<MessagingCenterTests>(this, null));

			Assert.Throws<ArgumentNullException>(() => MessagingCenter.Unsubscribe<MessagingCenterTests>(null, "Foo"));
			Assert.Throws<ArgumentNullException>(() => MessagingCenter.Unsubscribe<MessagingCenterTests>(this, null));

			Assert.Throws<ArgumentNullException>(() => MessagingCenter.Unsubscribe<MessagingCenterTests, string>(null, "Foo"));
			Assert.Throws<ArgumentNullException>(() => MessagingCenter.Unsubscribe<MessagingCenterTests, string>(this, null));
		}

		[Test]
		public void UnsubscribeInCallback()
		{
			int messageCount = 0;

			var subscriber1 = new object();
			var subscriber2 = new object();

			MessagingCenter.Subscribe<MessagingCenterTests>(subscriber1, "SimpleTest", (sender) =>
			{
				messageCount++;
				MessagingCenter.Unsubscribe<MessagingCenterTests>(subscriber2, "SimpleTest");
			});

			MessagingCenter.Subscribe<MessagingCenterTests>(subscriber2, "SimpleTest", (sender) =>
			{
				messageCount++;
				MessagingCenter.Unsubscribe<MessagingCenterTests>(subscriber1, "SimpleTest");
			});

			MessagingCenter.Send(this, "SimpleTest");

			Assert.AreEqual(1, messageCount);
		}

		[Test]
		public void SubscriberShouldBeCollected()
		{
			new Action(() =>
			{
				var subscriber = new TestSubcriber();
				MessagingCenter.Subscribe<TestPublisher>(subscriber, "test", p => Assert.Fail());
			})();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			var pub = new TestPublisher();
			pub.Test(); // Assert.Fail() shouldn't be called, because the TestSubcriber object should have ben GCed
		}

		[Test]
		public void ShouldBeCollectedIfCallbackTargetIsSubscriber()
		{
			WeakReference wr = null;

			new Action(() =>
			{
				var subscriber = new TestSubcriber();

				wr = new WeakReference(subscriber);

				subscriber.SubscribeToTestMessages();
			})();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			var pub = new TestPublisher();
			pub.Test();

			Assert.IsFalse(wr.IsAlive); // The Action target and subscriber were the same object, so both could be collected
		}

		[Test]
		public void NotCollectedIfSubscriberIsNotTheCallbackTarget()
		{
			WeakReference wr = null;

			new Action(() =>
			{
				var subscriber = new TestSubcriber();

				wr = new WeakReference(subscriber);

				// This creates a closure, so the callback target is not 'subscriber', but an instancce of a compiler generated class 
				// So MC has to keep a strong reference to it, and 'subscriber' won't be collectable
				MessagingCenter.Subscribe<TestPublisher>(subscriber, "test", p => subscriber.SetSuccess());
			})();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.IsTrue(wr.IsAlive); // The closure in Subscribe should be keeping the subscriber alive
			Assert.IsNotNull(wr.Target as TestSubcriber);

			Assert.IsFalse(((TestSubcriber)wr.Target).Successful);

			var pub = new TestPublisher();
			pub.Test();

			Assert.IsTrue(((TestSubcriber)wr.Target).Successful);  // Since it's still alive, the subscriber should still have received the message and updated the property
		}

		[Test]
		public void SubscriberCollectableAfterUnsubscribeEvenIfHeldByClosure()
		{
			WeakReference wr = null;

			new Action(() =>
			{
				var subscriber = new TestSubcriber();

				wr = new WeakReference(subscriber);

				MessagingCenter.Subscribe<TestPublisher>(subscriber, "test", p => subscriber.SetSuccess());
			})();

			Assert.IsNotNull(wr.Target as TestSubcriber);

			MessagingCenter.Unsubscribe<TestPublisher>(wr.Target, "test");

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.IsFalse(wr.IsAlive); // The Action target and subscriber were the same object, so both could be collected
		}

		[Test]
		public void StaticCallback()
		{
			int i = 4;

			_subscriber = new TestSubcriber(); // Using a class member so it doesn't get optimized away in Release build

			MessagingCenter.Subscribe<TestPublisher>(_subscriber, "test", p => MessagingCenterTestsCallbackSource.Increment(ref i));

			GC.Collect();
			GC.WaitForPendingFinalizers();

			var pub = new TestPublisher();
			pub.Test();

			Assert.IsTrue(i == 5, "The static method should have incremented 'i'");
		}

		[Test]
		public void NothingShouldBeCollected()
		{
			var success = false;

			_subscriber = new TestSubcriber(); // Using a class member so it doesn't get optimized away in Release build

			var source = new MessagingCenterTestsCallbackSource();
			MessagingCenter.Subscribe<TestPublisher>(_subscriber, "test", p => source.SuccessCallback(ref success));

			GC.Collect();
			GC.WaitForPendingFinalizers();

			var pub = new TestPublisher();
			pub.Test();

			Assert.True(success); // TestCallbackSource.SuccessCallback() should be invoked to make success == true
		}

		[Test]
		public void MultipleSubscribersOfTheSameClass()
		{
			var sub1 = new object();
			var sub2 = new object();

			string args2 = null;

			const string message = "message";

			MessagingCenter.Subscribe<MessagingCenterTests, string>(sub1, message, (sender, args) => { });
			MessagingCenter.Subscribe<MessagingCenterTests, string>(sub2, message, (sender, args) => args2 = args);
			MessagingCenter.Unsubscribe<MessagingCenterTests, string>(sub1, message);

			MessagingCenter.Send(this, message, "Testing");
			Assert.That(args2, Is.EqualTo("Testing"), "unsubscribing sub1 should not unsubscribe sub2");
		}

		class TestSubcriber
		{
			public void SetSuccess()
			{
				Successful = true;
			}

			public bool Successful { get; private set; }

			public void SubscribeToTestMessages()
			{
				MessagingCenter.Subscribe<TestPublisher>(this, "test", p => SetSuccess());
			}
		}

		class TestPublisher
		{
			public void Test()
			{
				MessagingCenter.Send(this, "test");
			}
		}

		public class MessagingCenterTestsCallbackSource
		{
			public void SuccessCallback(ref bool success)
			{
				success = true;
			}

			public static void Increment(ref int i)
			{
				i = i + 1;
			}
		}

		[Test(Description = "This is a demonstration of what a test with a fake/mock/substitute IMessagingCenter might look like")]
		public void TestMessagingCenterSubstitute()
		{
			var mc = new FakeMessagingCenter();

			// In the real world, you'd construct this with `new ComponentWithMessagingDependency(MessagingCenter.Instance);`
			var component = new ComponentWithMessagingDependency(mc);
			component.DoAThing();

			Assert.IsTrue(mc.WasSubscribeCalled, "ComponentWithMessagingDependency should have subscribed in its constructor");
			Assert.IsTrue(mc.WasSendCalled, "The DoAThing method should send a message");
		}

		class ComponentWithMessagingDependency
		{
			readonly IMessagingCenter _messagingCenter;

			public ComponentWithMessagingDependency(IMessagingCenter messagingCenter)
			{
				_messagingCenter = messagingCenter;
				_messagingCenter.Subscribe<ComponentWithMessagingDependency>(this, "test", dependency => Console.WriteLine("test"));
			}

			public void DoAThing()
			{
				_messagingCenter.Send(this, "test");
			}
		}

		internal class FakeMessagingCenter : IMessagingCenter
		{
			public bool WasSubscribeCalled { get; private set; }
			public bool WasSendCalled { get; private set; }

			public void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class
			{
				WasSendCalled = true;
			}

			public void Send<TSender>(TSender sender, string message) where TSender : class
			{
				WasSendCalled = true;
			}

			public void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, TSender source = default(TSender)) where TSender : class
			{
				WasSubscribeCalled = true;
			}

			public void Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, TSender source = default(TSender)) where TSender : class
			{
				WasSubscribeCalled = true;
			}

			public void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class
			{

			}

			public void Unsubscribe<TSender>(object subscriber, string message) where TSender : class
			{

			}
		}
	}
}