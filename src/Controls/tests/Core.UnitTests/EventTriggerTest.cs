#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    internal class MockTriggerAction : TriggerAction<BindableObject>
    {
        public bool Invoked { get; set; }

        protected override void Invoke(BindableObject sender)
        {
            Invoked = true;
        }
    }


    internal class MockBindableWithEvent : VisualElement
    {
        public void FireEvent()
        {
            if (MockEvent != null)
                MockEvent(this, EventArgs.Empty);
        }

        public void FireEvent2()
        {
            if (MockEvent2 != null)
                MockEvent2(this, EventArgs.Empty);
        }

        public event EventHandler MockEvent;
        public event EventHandler MockEvent2;
    }



    public class EventTriggerTest : BaseTestFixture
    {
        [Fact]
        public void TestTriggerActionInvoked()
        {
            var bindable = new MockBindableWithEvent();
            var triggeraction = new MockTriggerAction();
            var eventtrigger = new EventTrigger() { Event = "MockEvent", Actions = { triggeraction } };
            var collection = bindable.Triggers;
            collection.Add(eventtrigger);

            Assert.False(triggeraction.Invoked);
            bindable.FireEvent();
            Assert.True(triggeraction.Invoked);
        }

        [Fact]
        public void TestChangeEventOnEventTrigger()
        {
            var bindable = new MockBindableWithEvent();
            var triggeraction = new MockTriggerAction();
            var eventtrigger = new EventTrigger { Event = "MockEvent", Actions = { triggeraction } };
            var collection = bindable.Triggers;
            collection.Add(eventtrigger);

            triggeraction.Invoked = false;
            Assert.False(triggeraction.Invoked);
            bindable.FireEvent();
            Assert.True(triggeraction.Invoked);

            triggeraction.Invoked = false;
            Assert.False(triggeraction.Invoked);
            bindable.FireEvent2();
            Assert.False(triggeraction.Invoked);

            Assert.Throws<InvalidOperationException>(() => eventtrigger.Event = "MockEvent2");
        }
    }

    public partial class EventTriggerTests
    {
        /// <summary>
        /// Tests that setting the Event property to null when it is already null triggers the early return path.
        /// This test specifically targets the uncovered line where _eventname == value comparison is made.
        /// Expected result: The property remains null and no exception is thrown.
        /// </summary>
        [Fact]
        public void Event_SetNullWhenAlreadyNull_EarlyReturnPath()
        {
            // Arrange
            var eventTrigger = new EventTrigger();

            // Act & Assert - Setting null when already null should work (early return)
            eventTrigger.Event = null;
            Assert.Null(eventTrigger.Event);

            // Setting null again should still work (early return path)
            eventTrigger.Event = null;
            Assert.Null(eventTrigger.Event);
        }

        /// <summary>
        /// Tests that setting the Event property to empty string when it is already empty triggers the early return path.
        /// This test specifically targets the uncovered line where _eventname == value comparison is made.
        /// Expected result: The property remains empty and no exception is thrown.
        /// </summary>
        [Fact]
        public void Event_SetEmptyWhenAlreadyEmpty_EarlyReturnPath()
        {
            // Arrange
            var eventTrigger = new EventTrigger();
            eventTrigger.Event = "";

            // Act & Assert - Setting empty when already empty should work (early return)
            eventTrigger.Event = "";
            Assert.Equal("", eventTrigger.Event);
        }

        /// <summary>
        /// Tests that setting the Event property to the same string value when it already has that value triggers the early return path.
        /// This test specifically targets the uncovered line where _eventname == value comparison is made.
        /// Expected result: The property remains unchanged and no exception is thrown.
        /// </summary>
        [Fact]
        public void Event_SetSameValueWhenAlreadySet_EarlyReturnPath()
        {
            // Arrange
            var eventTrigger = new EventTrigger();
            const string eventName = "TestEvent";
            eventTrigger.Event = eventName;

            // Act & Assert - Setting same value should work (early return)
            eventTrigger.Event = eventName;
            Assert.Equal(eventName, eventTrigger.Event);
        }

        /// <summary>
        /// Tests that setting the Event property to whitespace-only string when it already has that value triggers the early return path.
        /// This test covers edge case with whitespace strings and targets the uncovered comparison line.
        /// Expected result: The property remains unchanged and no exception is thrown.
        /// </summary>
        [Fact]
        public void Event_SetWhitespaceWhenAlreadyWhitespace_EarlyReturnPath()
        {
            // Arrange
            var eventTrigger = new EventTrigger();
            const string whitespace = "   ";
            eventTrigger.Event = whitespace;

            // Act & Assert - Setting same whitespace value should work (early return)
            eventTrigger.Event = whitespace;
            Assert.Equal(whitespace, eventTrigger.Event);
        }

        /// <summary>
        /// Tests that setting the Event property to a very long string when it already has that value triggers the early return path.
        /// This test covers edge case with long strings and targets the uncovered comparison line.
        /// Expected result: The property remains unchanged and no exception is thrown.
        /// </summary>
        [Fact]
        public void Event_SetLongStringWhenAlreadyLongString_EarlyReturnPath()
        {
            // Arrange
            var eventTrigger = new EventTrigger();
            var longString = new string('A', 10000);
            eventTrigger.Event = longString;

            // Act & Assert - Setting same long string should work (early return)
            eventTrigger.Event = longString;
            Assert.Equal(longString, eventTrigger.Event);
        }

        /// <summary>
        /// Tests that setting the Event property to different values works correctly and does not trigger early return.
        /// This verifies the normal setter path when values are different.
        /// Expected result: The property value changes as expected.
        /// </summary>
        [Fact]
        public void Event_SetDifferentValues_PropertyChanges()
        {
            // Arrange
            var eventTrigger = new EventTrigger();

            // Act & Assert - Setting different values should work
            eventTrigger.Event = "FirstEvent";
            Assert.Equal("FirstEvent", eventTrigger.Event);

            eventTrigger.Event = "SecondEvent";
            Assert.Equal("SecondEvent", eventTrigger.Event);

            eventTrigger.Event = null;
            Assert.Null(eventTrigger.Event);

            eventTrigger.Event = "";
            Assert.Equal("", eventTrigger.Event);
        }

        /// <summary>
        /// Tests that setting the Event property with special characters when it already has that value triggers the early return path.
        /// This test covers edge case with special characters and targets the uncovered comparison line.
        /// Expected result: The property remains unchanged and no exception is thrown.
        /// </summary>
        [Fact]
        public void Event_SetSpecialCharactersWhenAlreadySet_EarlyReturnPath()
        {
            // Arrange
            var eventTrigger = new EventTrigger();
            const string specialChars = "Event@#$%^&*(){}[]|\\:;\"'<>,.?/~`";
            eventTrigger.Event = specialChars;

            // Act & Assert - Setting same special characters should work (early return)
            eventTrigger.Event = specialChars;
            Assert.Equal(specialChars, eventTrigger.Event);
        }

        /// <summary>
        /// Tests that the Event property getter returns the correct value after setting.
        /// This verifies the basic getter functionality.
        /// Expected result: The getter returns the value that was set.
        /// </summary>
        [Fact]
        public void Event_GetAfterSet_ReturnsCorrectValue()
        {
            // Arrange
            var eventTrigger = new EventTrigger();
            const string eventName = "TestEvent";

            // Act
            eventTrigger.Event = eventName;

            // Assert
            Assert.Equal(eventName, eventTrigger.Event);
        }

        /// <summary>
        /// Tests that the Event property handles null to non-null transitions correctly.
        /// This verifies the setter behavior when changing from null to a value.
        /// Expected result: The property changes from null to the specified value.
        /// </summary>
        [Fact]
        public void Event_SetNullToNonNull_PropertyChanges()
        {
            // Arrange
            var eventTrigger = new EventTrigger();
            Assert.Null(eventTrigger.Event);

            // Act
            eventTrigger.Event = "NewEvent";

            // Assert
            Assert.Equal("NewEvent", eventTrigger.Event);
        }

        /// <summary>
        /// Tests that the Event property handles non-null to null transitions correctly.
        /// This verifies the setter behavior when changing from a value to null.
        /// Expected result: The property changes from the value to null.
        /// </summary>
        [Fact]
        public void Event_SetNonNullToNull_PropertyChanges()
        {
            // Arrange
            var eventTrigger = new EventTrigger();
            eventTrigger.Event = "ExistingEvent";

            // Act
            eventTrigger.Event = null;

            // Assert
            Assert.Null(eventTrigger.Event);
        }

        /// <summary>
        /// Tests that OnDetachingFrom handles null bindable parameter without throwing exceptions.
        /// Input: null bindable parameter
        /// Expected: Method completes without exception and calls base implementation
        /// </summary>
        [Fact]
        public void OnDetachingFrom_WithNullBindable_DoesNotThrowAndCallsBase()
        {
            // Arrange
            var trigger = new TestableEventTrigger();

            // Act & Assert - Should not throw
            trigger.CallOnDetachingFrom(null);
        }

        /// <summary>
        /// Tests that OnDetachingFrom with bindable not in associated objects only calls base method.
        /// Input: Valid bindable object not present in _associatedObjects
        /// Expected: No removal from _associatedObjects, base method called
        /// </summary>
        [Fact]
        public void OnDetachingFrom_WithBindableNotInAssociatedObjects_CallsBaseOnly()
        {
            // Arrange
            var trigger = new TestableEventTrigger();
            var bindable = Substitute.For<BindableObject>();
            var otherBindable = Substitute.For<BindableObject>();

            trigger.AddAssociatedObject(otherBindable);
            var initialCount = trigger.GetAssociatedObjects().Count;

            // Act
            trigger.CallOnDetachingFrom(bindable);

            // Assert
            Assert.Equal(initialCount, trigger.GetAssociatedObjects().Count);
        }

        /// <summary>
        /// Tests that OnDetachingFrom removes matching bindable from associated objects.
        /// Input: Valid bindable object present in _associatedObjects
        /// Expected: Matching weak reference removed from _associatedObjects
        /// </summary>
        [Fact]
        public void OnDetachingFrom_WithBindableInAssociatedObjects_RemovesMatchingReference()
        {
            // Arrange
            var trigger = new TestableEventTrigger();
            var bindable = Substitute.For<BindableObject>();

            trigger.AddAssociatedObject(bindable);
            var initialCount = trigger.GetAssociatedObjects().Count;
            Assert.Equal(1, initialCount);

            // Act
            trigger.CallOnDetachingFrom(bindable);

            // Assert
            Assert.Equal(0, trigger.GetAssociatedObjects().Count);
        }

        /// <summary>
        /// Tests that OnDetachingFrom removes only matching references when multiple objects exist.
        /// Input: Multiple objects in _associatedObjects, detaching from one specific object
        /// Expected: Only the matching object is removed, others remain
        /// </summary>
        [Fact]
        public void OnDetachingFrom_WithMultipleAssociatedObjects_RemovesOnlyMatchingOnes()
        {
            // Arrange
            var trigger = new TestableEventTrigger();
            var bindable1 = Substitute.For<BindableObject>();
            var bindable2 = Substitute.For<BindableObject>();
            var bindable3 = Substitute.For<BindableObject>();

            trigger.AddAssociatedObject(bindable1);
            trigger.AddAssociatedObject(bindable2);
            trigger.AddAssociatedObject(bindable3);

            Assert.Equal(3, trigger.GetAssociatedObjects().Count);

            // Act
            trigger.CallOnDetachingFrom(bindable2);

            // Assert
            Assert.Equal(2, trigger.GetAssociatedObjects().Count);

            // Verify the correct objects remain
            var remainingObjects = trigger.GetAssociatedObjects();
            var remainingTargets = remainingObjects
                .Where(wr => wr.TryGetTarget(out var target))
                .Select(wr => { wr.TryGetTarget(out var target); return target; })
                .ToList();

            Assert.Contains(bindable1, remainingTargets);
            Assert.Contains(bindable3, remainingTargets);
            Assert.DoesNotContain(bindable2, remainingTargets);
        }

        /// <summary>
        /// Tests that OnDetachingFrom handles dead weak references gracefully.
        /// Input: _associatedObjects containing weak references to garbage collected objects
        /// Expected: Dead references are handled without exceptions
        /// </summary>
        [Fact]
        public void OnDetachingFrom_WithDeadWeakReferences_HandlesGracefully()
        {
            // Arrange
            var trigger = new TestableEventTrigger();
            var bindable = Substitute.For<BindableObject>();

            // Create a weak reference to an object that will be collected
            var associatedObjects = trigger.GetAssociatedObjects();
            var tempObject = new Button();
            associatedObjects.Add(new WeakReference<BindableObject>(tempObject));

            // Force garbage collection to make the weak reference dead
            tempObject = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Act & Assert - Should not throw
            trigger.CallOnDetachingFrom(bindable);
        }

        /// <summary>
        /// Tests that OnDetachingFrom removes multiple matching references of the same object.
        /// Input: Multiple weak references to the same bindable object
        /// Expected: All matching references are removed
        /// </summary>
        [Fact]
        public void OnDetachingFrom_WithDuplicateReferences_RemovesAllMatchingReferences()
        {
            // Arrange
            var trigger = new TestableEventTrigger();
            var bindable = Substitute.For<BindableObject>();
            var otherBindable = Substitute.For<BindableObject>();

            // Add the same bindable multiple times
            trigger.AddAssociatedObject(bindable);
            trigger.AddAssociatedObject(otherBindable);
            trigger.AddAssociatedObject(bindable);

            Assert.Equal(3, trigger.GetAssociatedObjects().Count);

            // Act
            trigger.CallOnDetachingFrom(bindable);

            // Assert
            Assert.Equal(1, trigger.GetAssociatedObjects().Count);

            // Verify only the other bindable remains
            var remainingObjects = trigger.GetAssociatedObjects();
            Assert.True(remainingObjects[0].TryGetTarget(out var remainingTarget));
            Assert.Equal(otherBindable, remainingTarget);
        }

        /// <summary>
        /// Tests that OnDetachingFrom works correctly with empty associated objects list.
        /// Input: Empty _associatedObjects list
        /// Expected: Method completes without exception and calls base implementation
        /// </summary>
        [Fact]
        public void OnDetachingFrom_WithEmptyAssociatedObjects_CompletesSuccessfully()
        {
            // Arrange
            var trigger = new TestableEventTrigger();
            var bindable = Substitute.For<BindableObject>();

            Assert.Empty(trigger.GetAssociatedObjects());

            // Act & Assert - Should not throw
            trigger.CallOnDetachingFrom(bindable);

            // Verify still empty
            Assert.Empty(trigger.GetAssociatedObjects());
        }

        /// <summary>
        /// Tests that the EventTrigger constructor successfully creates an instance
        /// and properly initializes the object without throwing any exceptions.
        /// </summary>
        [Fact]
        public void Constructor_CreatesInstance_Successfully()
        {
            // Arrange & Act
            var eventTrigger = new EventTrigger();

            // Assert
            Assert.NotNull(eventTrigger);
        }

        /// <summary>
        /// Tests that the EventTrigger constructor properly initializes the Actions property
        /// to a non-null collection that implements IList&lt;TriggerAction&gt;.
        /// </summary>
        [Fact]
        public void Constructor_InitializesActions_PropertyNotNull()
        {
            // Arrange & Act
            var eventTrigger = new EventTrigger();

            // Assert
            Assert.NotNull(eventTrigger.Actions);
            Assert.IsAssignableFrom<IList<TriggerAction>>(eventTrigger.Actions);
        }

        /// <summary>
        /// Tests that the EventTrigger constructor initializes the Actions collection
        /// to be empty with zero items.
        /// </summary>
        [Fact]
        public void Constructor_InitializesActions_CollectionEmpty()
        {
            // Arrange & Act
            var eventTrigger = new EventTrigger();

            // Assert
            Assert.Equal(0, eventTrigger.Actions.Count);
            Assert.Empty(eventTrigger.Actions);
        }

        /// <summary>
        /// Tests that the EventTrigger constructor initializes the Actions collection
        /// to be writable (not read-only) so that trigger actions can be added.
        /// </summary>
        [Fact]
        public void Constructor_InitializesActions_CollectionNotReadOnly()
        {
            // Arrange & Act
            var eventTrigger = new EventTrigger();

            // Assert
            Assert.False(eventTrigger.Actions.IsReadOnly);
        }

        /// <summary>
        /// Tests that the EventTrigger constructor allows immediate access to the Actions
        /// property and that items can be added to the collection without exceptions.
        /// </summary>
        [Fact]
        public void Constructor_InitializesActions_CanAddItems()
        {
            // Arrange
            var eventTrigger = new EventTrigger();
            var mockAction = new MockTriggerAction();

            // Act & Assert - Should not throw
            eventTrigger.Actions.Add(mockAction);
            Assert.Single(eventTrigger.Actions);
            Assert.Contains(mockAction, eventTrigger.Actions);
        }
    }
}