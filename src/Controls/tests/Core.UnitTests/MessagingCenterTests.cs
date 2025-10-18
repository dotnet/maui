#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class MessagingCenterTests : BaseTestFixture
    {
        TestSubcriber _subscriber;

        [Fact]
        public void SingleSubscriber()
        {
            string sentMessage = null;
            MessagingCenter.Subscribe<MessagingCenterTests, string>(this, "SimpleTest", (sender, args) => sentMessage = args);

            MessagingCenter.Send(this, "SimpleTest", "My Message");

            Assert.Equal("My Message", sentMessage);

            MessagingCenter.Unsubscribe<MessagingCenterTests, string>(this, "SimpleTest");
        }

        [Fact]
        public void Filter()
        {
            string sentMessage = null;
            MessagingCenter.Subscribe<MessagingCenterTests, string>(this, "SimpleTest", (sender, args) => sentMessage = args, this);

            MessagingCenter.Send(new MessagingCenterTests(), "SimpleTest", "My Message");

            Assert.Null(sentMessage);

            MessagingCenter.Send(this, "SimpleTest", "My Message");

            Assert.Equal("My Message", sentMessage);

            MessagingCenter.Unsubscribe<MessagingCenterTests, string>(this, "SimpleTest");
        }

        [Fact]
        public void MultiSubscriber()
        {
            var sub1 = new object();
            var sub2 = new object();
            string sentMessage1 = null;
            string sentMessage2 = null;
            MessagingCenter.Subscribe<MessagingCenterTests, string>(sub1, "SimpleTest", (sender, args) => sentMessage1 = args);
            MessagingCenter.Subscribe<MessagingCenterTests, string>(sub2, "SimpleTest", (sender, args) => sentMessage2 = args);

            MessagingCenter.Send(this, "SimpleTest", "My Message");

            Assert.Equal("My Message", sentMessage1);
            Assert.Equal("My Message", sentMessage2);

            MessagingCenter.Unsubscribe<MessagingCenterTests, string>(sub1, "SimpleTest");
            MessagingCenter.Unsubscribe<MessagingCenterTests, string>(sub2, "SimpleTest");
        }

        [Fact]
        public void Unsubscribe()
        {
            string sentMessage = null;
            MessagingCenter.Subscribe<MessagingCenterTests, string>(this, "SimpleTest", (sender, args) => sentMessage = args);
            MessagingCenter.Unsubscribe<MessagingCenterTests, string>(this, "SimpleTest");

            MessagingCenter.Send(this, "SimpleTest", "My Message");

            Assert.Null(sentMessage);
        }

        [Fact]
        public void SendWithoutSubscribers()
        {
            MessagingCenter.Send(this, "SimpleTest", "My Message");
        }

        [Fact]
        public void NoArgSingleSubscriber()
        {
            bool sentMessage = false;
            MessagingCenter.Subscribe<MessagingCenterTests>(this, "SimpleTest", sender => sentMessage = true);

            MessagingCenter.Send(this, "SimpleTest");

            Assert.True(sentMessage);

            MessagingCenter.Unsubscribe<MessagingCenterTests>(this, "SimpleTest");
        }

        [Fact]
        public void NoArgFilter()
        {
            bool sentMessage = false;
            MessagingCenter.Subscribe(this, "SimpleTest", (sender) => sentMessage = true, this);

            MessagingCenter.Send(new MessagingCenterTests(), "SimpleTest");

            Assert.False(sentMessage);

            MessagingCenter.Send(this, "SimpleTest");

            Assert.True(sentMessage);

            MessagingCenter.Unsubscribe<MessagingCenterTests>(this, "SimpleTest");
        }

        [Fact]
        public void NoArgMultiSubscriber()
        {
            var sub1 = new object();
            var sub2 = new object();
            bool sentMessage1 = false;
            bool sentMessage2 = false;
            MessagingCenter.Subscribe<MessagingCenterTests>(sub1, "SimpleTest", (sender) => sentMessage1 = true);
            MessagingCenter.Subscribe<MessagingCenterTests>(sub2, "SimpleTest", (sender) => sentMessage2 = true);

            MessagingCenter.Send(this, "SimpleTest");

            Assert.True(sentMessage1);
            Assert.True(sentMessage2);

            MessagingCenter.Unsubscribe<MessagingCenterTests>(sub1, "SimpleTest");
            MessagingCenter.Unsubscribe<MessagingCenterTests>(sub2, "SimpleTest");
        }

        [Fact]
        public void NoArgUnsubscribe()
        {
            bool sentMessage = false;
            MessagingCenter.Subscribe<MessagingCenterTests>(this, "SimpleTest", (sender) => sentMessage = true);
            MessagingCenter.Unsubscribe<MessagingCenterTests>(this, "SimpleTest");

            MessagingCenter.Send(this, "SimpleTest", "My Message");

            Assert.False(sentMessage);
        }

        [Fact]
        public void NoArgSendWithoutSubscribers()
        {
            MessagingCenter.Send(this, "SimpleTest");
        }

        [Fact]
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

        [Fact]
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

            Assert.Equal(1, messageCount);
        }

        [Fact]
        public void SubscriberShouldBeCollected()
        {
            new Action(() =>
            {
                var subscriber = new TestSubcriber();
                MessagingCenter.Subscribe<TestPublisher>(subscriber, "test", p => throw new XunitException("The subscriber should have been collected."));
            })();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            var pub = new TestPublisher();
            pub.Test(); // Assert.Fail() shouldn't be called, because the TestSubcriber object should have ben GCed
        }

        [Fact]
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

            Assert.False(wr.IsAlive); // The Action target and subscriber were the same object, so both could be collected
        }

        [Fact]
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

            Assert.True(wr.IsAlive); // The closure in Subscribe should be keeping the subscriber alive
            Assert.NotNull(wr.Target as TestSubcriber);

            Assert.False(((TestSubcriber)wr.Target).Successful);

            var pub = new TestPublisher();
            pub.Test();

            Assert.True(((TestSubcriber)wr.Target).Successful);  // Since it's still alive, the subscriber should still have received the message and updated the property
        }

        [Fact, Category(TestCategory.Memory)]
        public async Task SubscriberCollectableAfterUnsubscribeEvenIfHeldByClosure()
        {
            WeakReference CreateReference()
            {
                WeakReference wr = null;

                new Action(() =>
                {
                    var subscriber = new TestSubcriber();

                    wr = new WeakReference(subscriber);

                    MessagingCenter.Subscribe<TestPublisher>(subscriber, "test", p => subscriber.SetSuccess());
                })();

                Assert.NotNull(wr.Target as TestSubcriber);

                MessagingCenter.Unsubscribe<TestPublisher>(wr.Target, "test");

                return wr;
            }

            var wr = CreateReference();

            await TestHelpers.Collect();

            Assert.False(wr.IsAlive); // The Action target and subscriber were the same object, so both could be collected
        }

        [Fact]
        public void StaticCallback()
        {
            int i = 4;

            _subscriber = new TestSubcriber(); // Using a class member so it doesn't get optimized away in Release build

            MessagingCenter.Subscribe<TestPublisher>(_subscriber, "test", p => MessagingCenterTestsCallbackSource.Increment(ref i));

            GC.Collect();
            GC.WaitForPendingFinalizers();

            var pub = new TestPublisher();
            pub.Test();

            Assert.True(i == 5, "The static method should have incremented 'i'");
        }

        [Fact]
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

        [Fact]
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
            Assert.True(args2 == "Testing", "unsubscribing sub1 should not unsubscribe sub2");
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

        [Fact("This is a demonstration of what a test with a fake/mock/substitute IMessagingCenter might look like")]
        public void TestMessagingCenterSubstitute()
        {
            var mc = new FakeMessagingCenter();

            // In the real world, you'd construct this with `new ComponentWithMessagingDependency(MessagingCenter.Instance);`
            var component = new ComponentWithMessagingDependency(mc);
            component.DoAThing();

            Assert.True(mc.WasSubscribeCalled, "ComponentWithMessagingDependency should have subscribed in its constructor");
            Assert.True(mc.WasSendCalled, "The DoAThing method should send a message");
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

        /// <summary>
        /// Tests that ClearSubscribers successfully removes all existing subscriptions when subscriptions exist.
        /// Verifies that after clearing, no callbacks are invoked when messages are sent.
        /// </summary>
        [Fact]
        public void ClearSubscribers_WithExistingSubscriptions_RemovesAllSubscriptions()
        {
            // Arrange
            bool callback1Invoked = false;
            bool callback2Invoked = false;
            bool callback3Invoked = false;

            var subscriber1 = new object();
            var subscriber2 = new object();
            var subscriber3 = new object();

            // Subscribe to different message types
            MessagingCenter.Subscribe<MessagingCenterTests, string>(subscriber1, "TestMessage1", (sender, args) => callback1Invoked = true);
            MessagingCenter.Subscribe<MessagingCenterTests>(subscriber2, "TestMessage2", sender => callback2Invoked = true);
            MessagingCenter.Subscribe<MessagingCenterTests, int>(subscriber3, "TestMessage3", (sender, args) => callback3Invoked = true);

            // Act
            MessagingCenter.ClearSubscribers();

            // Send messages to verify subscriptions were cleared
            MessagingCenter.Send(this, "TestMessage1", "test");
            MessagingCenter.Send(this, "TestMessage2");
            MessagingCenter.Send(this, "TestMessage3", 42);

            // Assert
            Assert.False(callback1Invoked);
            Assert.False(callback2Invoked);
            Assert.False(callback3Invoked);
        }

        /// <summary>
        /// Tests that ClearSubscribers does not throw an exception when called with no existing subscriptions.
        /// Verifies the method handles empty subscription state gracefully.
        /// </summary>
        [Fact]
        public void ClearSubscribers_WithNoSubscriptions_DoesNotThrow()
        {
            // Arrange - ensure no subscriptions exist
            MessagingCenter.ClearSubscribers();

            // Act & Assert - should not throw
            MessagingCenter.ClearSubscribers();
        }

        /// <summary>
        /// Tests that ClearSubscribers can be called multiple times consecutively without issues.
        /// Verifies the method is idempotent and handles repeated clearing operations.
        /// </summary>
        [Fact]
        public void ClearSubscribers_CalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            var subscriber = new object();
            MessagingCenter.Subscribe<MessagingCenterTests, string>(subscriber, "TestMessage", (sender, args) => { });

            // Act & Assert - multiple calls should not throw
            MessagingCenter.ClearSubscribers();
            MessagingCenter.ClearSubscribers();
            MessagingCenter.ClearSubscribers();
        }

        /// <summary>
        /// Tests that ClearSubscribers removes subscriptions with different generic type parameters.
        /// Verifies that all subscription types (with and without arguments) are properly cleared.
        /// </summary>
        [Fact]
        public void ClearSubscribers_WithMixedSubscriptionTypes_ClearsAllTypes()
        {
            // Arrange
            bool stringArgCallback = false;
            bool intArgCallback = false;
            bool noArgCallback = false;
            bool customObjectCallback = false;

            var subscriber = new object();
            var customObject = new { Value = "test" };

            MessagingCenter.Subscribe<MessagingCenterTests, string>(subscriber, "StringMessage", (sender, args) => stringArgCallback = true);
            MessagingCenter.Subscribe<MessagingCenterTests, int>(subscriber, "IntMessage", (sender, args) => intArgCallback = true);
            MessagingCenter.Subscribe<MessagingCenterTests>(subscriber, "NoArgMessage", sender => noArgCallback = true);
            MessagingCenter.Subscribe<MessagingCenterTests, object>(subscriber, "ObjectMessage", (sender, args) => customObjectCallback = true);

            // Act
            MessagingCenter.ClearSubscribers();

            // Send all message types to verify they were cleared
            MessagingCenter.Send(this, "StringMessage", "test");
            MessagingCenter.Send(this, "IntMessage", 123);
            MessagingCenter.Send(this, "NoArgMessage");
            MessagingCenter.Send(this, "ObjectMessage", customObject);

            // Assert
            Assert.False(stringArgCallback);
            Assert.False(intArgCallback);
            Assert.False(noArgCallback);
            Assert.False(customObjectCallback);
        }

        /// <summary>
        /// Tests that after ClearSubscribers, new subscriptions can be added normally.
        /// Verifies that clearing does not break the messaging system for future use.
        /// </summary>
        [Fact]
        public void ClearSubscribers_AfterClearing_AllowsNewSubscriptions()
        {
            // Arrange
            var subscriber = new object();
            MessagingCenter.Subscribe<MessagingCenterTests, string>(subscriber, "InitialMessage", (sender, args) => { });

            // Act - clear existing subscriptions
            MessagingCenter.ClearSubscribers();

            // Add new subscription after clearing
            bool newCallbackInvoked = false;
            MessagingCenter.Subscribe<MessagingCenterTests, string>(subscriber, "NewMessage", (sender, args) => newCallbackInvoked = true);

            // Send message to new subscription
            MessagingCenter.Send(this, "NewMessage", "test");

            // Assert
            Assert.True(newCallbackInvoked);

            // Cleanup
            MessagingCenter.Unsubscribe<MessagingCenterTests, string>(subscriber, "NewMessage");
        }
    }

    public partial class SubscriptionTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that InvokeCallback returns early when Filter returns false.
        /// Input: Filter that returns false for any sender.
        /// Expected: Method should return without invoking MethodInfo.Invoke.
        /// </summary>
        [Fact]
        public void InvokeCallback_FilterReturnsFalse_ReturnsWithoutInvokingMethod()
        {
            // Arrange
            var mockSubscriber = new object();
            var mockDelegateSource = new object();
            var mockMethodInfo = Substitute.For<MethodInfo>();
            var mockFilter = Substitute.For<MessagingCenter.Filter>();

            mockFilter.Invoke(Arg.Any<object>()).Returns(false);

            var subscription = new MessagingCenter.Subscription(mockSubscriber, mockDelegateSource, mockMethodInfo, mockFilter);
            var sender = new object();
            var args = new object();

            // Act
            subscription.InvokeCallback(sender, args);

            // Assert
            mockFilter.Received(1).Invoke(sender);
            mockMethodInfo.DidNotReceive().Invoke(Arg.Any<object>(), Arg.Any<object[]>());
        }

        /// <summary>
        /// Tests that InvokeCallback correctly invokes static method with single parameter.
        /// Input: Static MethodInfo with single parameter, filter returns true.
        /// Expected: MethodInfo.Invoke called with null target and sender only.
        /// </summary>
        [Fact]
        public void InvokeCallback_StaticMethodSingleParameter_InvokesWithNullTargetAndSenderOnly()
        {
            // Arrange
            var mockSubscriber = new object();
            var mockDelegateSource = new object();
            var mockMethodInfo = Substitute.For<MethodInfo>();
            var mockParameters = new[] { Substitute.For<ParameterInfo>() };
            var mockFilter = Substitute.For<MessagingCenter.Filter>();

            mockFilter.Invoke(Arg.Any<object>()).Returns(true);
            mockMethodInfo.IsStatic.Returns(true);
            mockMethodInfo.GetParameters().Returns(mockParameters);

            var subscription = new MessagingCenter.Subscription(mockSubscriber, mockDelegateSource, mockMethodInfo, mockFilter);
            var sender = new object();
            var args = new object();

            // Act
            subscription.InvokeCallback(sender, args);

            // Assert
            mockFilter.Received(1).Invoke(sender);
            mockMethodInfo.Received(1).Invoke(null, Arg.Is<object[]>(arr => arr.Length == 1 && arr[0] == sender));
        }

        /// <summary>
        /// Tests that InvokeCallback correctly invokes static method with two parameters.
        /// Input: Static MethodInfo with two parameters, filter returns true.
        /// Expected: MethodInfo.Invoke called with null target, sender and args.
        /// </summary>
        [Fact]
        public void InvokeCallback_StaticMethodTwoParameters_InvokesWithNullTargetSenderAndArgs()
        {
            // Arrange
            var mockSubscriber = new object();
            var mockDelegateSource = new object();
            var mockMethodInfo = Substitute.For<MethodInfo>();
            var mockParameters = new[] { Substitute.For<ParameterInfo>(), Substitute.For<ParameterInfo>() };
            var mockFilter = Substitute.For<MessagingCenter.Filter>();

            mockFilter.Invoke(Arg.Any<object>()).Returns(true);
            mockMethodInfo.IsStatic.Returns(true);
            mockMethodInfo.GetParameters().Returns(mockParameters);

            var subscription = new MessagingCenter.Subscription(mockSubscriber, mockDelegateSource, mockMethodInfo, mockFilter);
            var sender = new object();
            var args = new object();

            // Act
            subscription.InvokeCallback(sender, args);

            // Assert
            mockFilter.Received(1).Invoke(sender);
            mockMethodInfo.Received(1).Invoke(null, Arg.Is<object[]>(arr => arr.Length == 2 && arr[0] == sender && arr[1] == args));
        }

        /// <summary>
        /// Tests that InvokeCallback returns early when target is null (garbage collected).
        /// Input: Non-static method with null target from DelegateSource.
        /// Expected: Method should return without invoking MethodInfo.Invoke.
        /// </summary>
        [Fact]
        public void InvokeCallback_TargetIsNull_ReturnsWithoutInvokingMethod()
        {
            // Arrange
            var mockSubscriber = new object();
            var mockMethodInfo = Substitute.For<MethodInfo>();
            var mockFilter = Substitute.For<MessagingCenter.Filter>();

            mockFilter.Invoke(Arg.Any<object>()).Returns(true);
            mockMethodInfo.IsStatic.Returns(false);

            // Create a MaybeWeakReference that returns null target (simulating garbage collection)
            var mockMaybeWeakRef = new TestMaybeWeakReference(null);
            var subscription = new TestSubscription(mockSubscriber, mockMaybeWeakRef, mockMethodInfo, mockFilter);

            var sender = new object();
            var args = new object();

            // Act
            subscription.InvokeCallback(sender, args);

            // Assert
            mockFilter.Received(1).Invoke(sender);
            mockMethodInfo.DidNotReceive().Invoke(Arg.Any<object>(), Arg.Any<object[]>());
        }

        /// <summary>
        /// Tests that InvokeCallback correctly invokes instance method with single parameter.
        /// Input: Non-static MethodInfo with single parameter, valid target.
        /// Expected: MethodInfo.Invoke called with target and sender only.
        /// </summary>
        [Fact]
        public void InvokeCallback_InstanceMethodSingleParameter_InvokesWithTargetAndSenderOnly()
        {
            // Arrange
            var mockSubscriber = new object();
            var mockTarget = new object();
            var mockMethodInfo = Substitute.For<MethodInfo>();
            var mockParameters = new[] { Substitute.For<ParameterInfo>() };
            var mockFilter = Substitute.For<MessagingCenter.Filter>();

            mockFilter.Invoke(Arg.Any<object>()).Returns(true);
            mockMethodInfo.IsStatic.Returns(false);
            mockMethodInfo.GetParameters().Returns(mockParameters);

            var mockMaybeWeakRef = new TestMaybeWeakReference(mockTarget);
            var subscription = new TestSubscription(mockSubscriber, mockMaybeWeakRef, mockMethodInfo, mockFilter);

            var sender = new object();
            var args = new object();

            // Act
            subscription.InvokeCallback(sender, args);

            // Assert
            mockFilter.Received(1).Invoke(sender);
            mockMethodInfo.Received(1).Invoke(mockTarget, Arg.Is<object[]>(arr => arr.Length == 1 && arr[0] == sender));
        }

        /// <summary>
        /// Tests that InvokeCallback correctly invokes instance method with two parameters.
        /// Input: Non-static MethodInfo with two parameters, valid target.
        /// Expected: MethodInfo.Invoke called with target, sender and args.
        /// </summary>
        [Fact]
        public void InvokeCallback_InstanceMethodTwoParameters_InvokesWithTargetSenderAndArgs()
        {
            // Arrange
            var mockSubscriber = new object();
            var mockTarget = new object();
            var mockMethodInfo = Substitute.For<MethodInfo>();
            var mockParameters = new[] { Substitute.For<ParameterInfo>(), Substitute.For<ParameterInfo>() };
            var mockFilter = Substitute.For<MessagingCenter.Filter>();

            mockFilter.Invoke(Arg.Any<object>()).Returns(true);
            mockMethodInfo.IsStatic.Returns(false);
            mockMethodInfo.GetParameters().Returns(mockParameters);

            var mockMaybeWeakRef = new TestMaybeWeakReference(mockTarget);
            var subscription = new TestSubscription(mockSubscriber, mockMaybeWeakRef, mockMethodInfo, mockFilter);

            var sender = new object();
            var args = new object();

            // Act
            subscription.InvokeCallback(sender, args);

            // Assert
            mockFilter.Received(1).Invoke(sender);
            mockMethodInfo.Received(1).Invoke(mockTarget, Arg.Is<object[]>(arr => arr.Length == 2 && arr[0] == sender && arr[1] == args));
        }

        /// <summary>
        /// Tests InvokeCallback with null sender parameter.
        /// Input: Null sender, valid args.
        /// Expected: Filter receives null, method invoked with null sender if filter passes.
        /// </summary>
        [Fact]
        public void InvokeCallback_NullSender_PassesNullToFilterAndInvoke()
        {
            // Arrange
            var mockSubscriber = new object();
            var mockTarget = new object();
            var mockMethodInfo = Substitute.For<MethodInfo>();
            var mockParameters = new[] { Substitute.For<ParameterInfo>(), Substitute.For<ParameterInfo>() };
            var mockFilter = Substitute.For<MessagingCenter.Filter>();

            mockFilter.Invoke(Arg.Any<object>()).Returns(true);
            mockMethodInfo.IsStatic.Returns(false);
            mockMethodInfo.GetParameters().Returns(mockParameters);

            var mockMaybeWeakRef = new TestMaybeWeakReference(mockTarget);
            var subscription = new TestSubscription(mockSubscriber, mockMaybeWeakRef, mockMethodInfo, mockFilter);

            object sender = null;
            var args = new object();

            // Act
            subscription.InvokeCallback(sender, args);

            // Assert
            mockFilter.Received(1).Invoke(null);
            mockMethodInfo.Received(1).Invoke(mockTarget, Arg.Is<object[]>(arr => arr.Length == 2 && arr[0] == null && arr[1] == args));
        }

        /// <summary>
        /// Tests InvokeCallback with null args parameter.
        /// Input: Valid sender, null args.
        /// Expected: Method invoked with null args if two parameters expected.
        /// </summary>
        [Fact]
        public void InvokeCallback_NullArgs_PassesNullArgsToInvoke()
        {
            // Arrange
            var mockSubscriber = new object();
            var mockTarget = new object();
            var mockMethodInfo = Substitute.For<MethodInfo>();
            var mockParameters = new[] { Substitute.For<ParameterInfo>(), Substitute.For<ParameterInfo>() };
            var mockFilter = Substitute.For<MessagingCenter.Filter>();

            mockFilter.Invoke(Arg.Any<object>()).Returns(true);
            mockMethodInfo.IsStatic.Returns(false);
            mockMethodInfo.GetParameters().Returns(mockParameters);

            var mockMaybeWeakRef = new TestMaybeWeakReference(mockTarget);
            var subscription = new TestSubscription(mockSubscriber, mockMaybeWeakRef, mockMethodInfo, mockFilter);

            var sender = new object();
            object args = null;

            // Act
            subscription.InvokeCallback(sender, args);

            // Assert
            mockFilter.Received(1).Invoke(sender);
            mockMethodInfo.Received(1).Invoke(mockTarget, Arg.Is<object[]>(arr => arr.Length == 2 && arr[0] == sender && arr[1] == null));
        }

        // Helper classes to enable testing of protected/internal functionality
        private class TestMaybeWeakReference
        {
            private readonly object _target;

            public TestMaybeWeakReference(object target)
            {
                _target = target;
            }

            public object Target => _target;
            public bool IsAlive => _target != null;
        }

    }
}