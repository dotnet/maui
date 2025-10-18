#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;


using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class NotifyCollectionChangedEventArgsExtensionsTests : BaseTestFixture
    {
        [Fact]
        public void Add()
        {
            List<string> applied = new List<string> { "foo", "bar", "baz" };

            Action reset = () => throw new XunitException("Reset should not be called");
            Action<object, int, bool> insert = (o, i, create) =>
            {
                Assert.True(create);
                applied.Insert(i, (string)o);
            };

            Action<object, int> removeAt = (o, i) => applied.RemoveAt(i);

            var items = new ObservableCollection<string>(applied);
            items.CollectionChanged += (s, e) => e.Apply(insert, removeAt, reset);

            items.Add("monkey");

            Assert.Equal(items, applied);
        }

        [Fact]
        public void Insert()
        {
            List<string> applied = new List<string> { "foo", "bar", "baz" };

            Action reset = () => throw new XunitException("Reset should not be called");
            Action<object, int, bool> insert = (o, i, create) =>
            {
                Assert.True(create);
                applied.Insert(i, (string)o);
            };
            Action<object, int> removeAt = (o, i) => applied.RemoveAt(i);

            var items = new ObservableCollection<string>(applied);
            items.CollectionChanged += (s, e) => e.Apply(insert, removeAt, reset);

            items.Insert(1, "monkey");

            Assert.Equal(items, applied);
        }

        [Fact]
        public void Move()
        {
            List<string> applied = new List<string> { "foo", "bar", "baz" };

            Action reset = () => throw new XunitException("Reset should not be called");
            Action<object, int, bool> insert = (o, i, create) =>
            {
                Assert.False(create);
                applied.Insert(i, (string)o);
            };

            Action<object, int> removeAt = (o, i) => applied.RemoveAt(i);

            var items = new ObservableCollection<string>(applied);
            items.CollectionChanged += (s, e) => e.Apply(insert, removeAt, reset);

            items.Move(0, 2);

            Assert.Equal(items, applied);
        }

        [Fact]
        public void Replace()
        {
            List<string> applied = new List<string> { "foo", "bar", "baz" };

            Action reset = () => throw new XunitException("Reset should not be called");
            Action<object, int, bool> insert = (o, i, create) =>
            {
                Assert.True(create);
                applied.Insert(i, (string)o);
            };

            Action<object, int> removeAt = (o, i) => applied.RemoveAt(i);

            var items = new ObservableCollection<string>(applied);
            items.CollectionChanged += (s, e) => e.Apply(insert, removeAt, reset);

            items[1] = "monkey";

            Assert.Equal(items, applied);
        }

        /// <summary>
        /// Tests that Apply throws ArgumentNullException when self parameter is null.
        /// Validates null parameter checking for the extension method target.
        /// Expected: ArgumentNullException with parameter name "self".
        /// </summary>
        [Fact]
        public void Apply_NullSelf_ThrowsArgumentNullException()
        {
            // Arrange
            NotifyCollectionChangedEventArgs self = null;
            Action<object, int, bool> insert = (o, i, create) => { };
            Action<object, int> removeAt = (o, i) => { };
            Action reset = () => { };

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                self.Apply(insert, removeAt, reset));
            Assert.Equal("self", exception.ParamName);
        }

        /// <summary>
        /// Tests that Apply throws ArgumentNullException when reset parameter is null.
        /// Validates null parameter checking for the reset action delegate.
        /// Expected: ArgumentNullException with parameter name "reset".
        /// </summary>
        [Fact]
        public void Apply_NullReset_ThrowsArgumentNullException()
        {
            // Arrange
            var self = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            Action<object, int, bool> insert = (o, i, create) => { };
            Action<object, int> removeAt = (o, i) => { };
            Action reset = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                self.Apply(insert, removeAt, reset));
            Assert.Equal("reset", exception.ParamName);
        }

        /// <summary>
        /// Tests that Apply throws ArgumentNullException when insert parameter is null.
        /// Validates null parameter checking for the insert action delegate.
        /// Expected: ArgumentNullException with parameter name "insert".
        /// </summary>
        [Fact]
        public void Apply_NullInsert_ThrowsArgumentNullException()
        {
            // Arrange
            var self = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            Action<object, int, bool> insert = null;
            Action<object, int> removeAt = (o, i) => { };
            Action reset = () => { };

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                self.Apply(insert, removeAt, reset));
            Assert.Equal("insert", exception.ParamName);
        }

        /// <summary>
        /// Tests that Apply throws ArgumentNullException when removeAt parameter is null.
        /// Validates null parameter checking for the removeAt action delegate.
        /// Expected: ArgumentNullException with parameter name "removeAt".
        /// </summary>
        [Fact]
        public void Apply_NullRemoveAt_ThrowsArgumentNullException()
        {
            // Arrange
            var self = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            Action<object, int, bool> insert = (o, i, create) => { };
            Action<object, int> removeAt = null;
            Action reset = () => { };

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                self.Apply(insert, removeAt, reset));
            Assert.Equal("removeAt", exception.ParamName);
        }

        /// <summary>
        /// Tests that Apply calls reset action when Add action has invalid NewStartingIndex.
        /// Validates fallback behavior when NewStartingIndex is negative for Add operation.
        /// Expected: Reset action is called and Reset action is returned.
        /// </summary>
        [Fact]
        public void Apply_AddWithInvalidNewStartingIndex_CallsReset()
        {
            // Arrange
            var items = new object[] { "test" };
            var self = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, -1);
            bool resetCalled = false;
            bool insertCalled = false;

            Action<object, int, bool> insert = (o, i, create) => insertCalled = true;
            Action<object, int> removeAt = (o, i) => { };
            Action reset = () => resetCalled = true;

            // Act
            var result = self.Apply(insert, removeAt, reset);

            // Assert
            Assert.True(resetCalled);
            Assert.False(insertCalled);
            Assert.Equal(NotifyCollectionChangedAction.Reset, result);
        }

        /// <summary>
        /// Tests that Apply calls reset action when Move action has invalid indices.
        /// Validates fallback behavior when NewStartingIndex or OldStartingIndex is negative for Move operation.
        /// Expected: Reset action is called and Reset action is returned.
        /// </summary>
        [Theory]
        [InlineData(-1, 0)] // Invalid new index
        [InlineData(0, -1)] // Invalid old index
        [InlineData(-1, -1)] // Both invalid
        public void Apply_MoveWithInvalidIndices_CallsReset(int oldStartingIndex, int newStartingIndex)
        {
            // Arrange
            var items = new object[] { "test" };
            var self = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, items, newStartingIndex, oldStartingIndex);
            bool resetCalled = false;
            bool insertCalled = false;
            bool removeAtCalled = false;

            Action<object, int, bool> insert = (o, i, create) => insertCalled = true;
            Action<object, int> removeAt = (o, i) => removeAtCalled = true;
            Action reset = () => resetCalled = true;

            // Act
            var result = self.Apply(insert, removeAt, reset);

            // Assert
            Assert.True(resetCalled);
            Assert.False(insertCalled);
            Assert.False(removeAtCalled);
            Assert.Equal(NotifyCollectionChangedAction.Reset, result);
        }

        /// <summary>
        /// Tests that Apply calls reset action when Remove action has invalid OldStartingIndex.
        /// Validates fallback behavior when OldStartingIndex is negative for Remove operation.
        /// Expected: Reset action is called and Reset action is returned.
        /// </summary>
        [Fact]
        public void Apply_RemoveWithInvalidOldStartingIndex_CallsReset()
        {
            // Arrange
            var items = new object[] { "test" };
            var self = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, -1);
            bool resetCalled = false;
            bool removeAtCalled = false;

            Action<object, int, bool> insert = (o, i, create) => { };
            Action<object, int> removeAt = (o, i) => removeAtCalled = true;
            Action reset = () => resetCalled = true;

            // Act
            var result = self.Apply(insert, removeAt, reset);

            // Assert
            Assert.True(resetCalled);
            Assert.False(removeAtCalled);
            Assert.Equal(NotifyCollectionChangedAction.Reset, result);
        }

        /// <summary>
        /// Tests that Apply calls reset action when Replace action has invalid conditions.
        /// Validates fallback behavior when OldStartingIndex is negative or item counts don't match for Replace operation.
        /// Expected: Reset action is called and Reset action is returned.
        /// </summary>
        [Theory]
        [InlineData(-1, 1, 1)] // Invalid old index
        [InlineData(0, 1, 2)] // Mismatched counts
        public void Apply_ReplaceWithInvalidConditions_CallsReset(int oldStartingIndex, int oldCount, int newCount)
        {
            // Arrange
            var oldItems = new object[oldCount];
            var newItems = new object[newCount];
            for (int i = 0; i < oldCount; i++) oldItems[i] = $"old{i}";
            for (int i = 0; i < newCount; i++) newItems[i] = $"new{i}";

            var self = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, oldStartingIndex);
            bool resetCalled = false;
            bool insertCalled = false;
            bool removeAtCalled = false;

            Action<object, int, bool> insert = (o, i, create) => insertCalled = true;
            Action<object, int> removeAt = (o, i) => removeAtCalled = true;
            Action reset = () => resetCalled = true;

            // Act
            var result = self.Apply(insert, removeAt, reset);

            // Assert
            Assert.True(resetCalled);
            Assert.False(insertCalled);
            Assert.False(removeAtCalled);
            Assert.Equal(NotifyCollectionChangedAction.Reset, result);
        }

        /// <summary>
        /// Tests that Apply correctly adjusts insert index for Move operation when old index is before new index.
        /// Validates the index adjustment logic: insertIndex -= self.OldItems.Count - 1.
        /// Expected: Insert is called with adjusted index.
        /// </summary>
        [Fact]
        public void Apply_MoveFromLowerToHigherIndex_AdjutsInsertIndex()
        {
            // Arrange
            var items = new object[] { "item1", "item2" };
            var self = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, items, 5, 2); // Move from index 2 to 5
            var insertedItems = new List<(object item, int index, bool create)>();
            var removedItems = new List<(object item, int index)>();

            Action<object, int, bool> insert = (o, i, create) => insertedItems.Add((o, i, create));
            Action<object, int> removeAt = (o, i) => removedItems.Add((o, i));
            Action reset = () => { };

            // Act
            var result = self.Apply(insert, removeAt, reset);

            // Assert
            Assert.Equal(NotifyCollectionChangedAction.Move, result);

            // Verify remove operations
            Assert.Equal(2, removedItems.Count);
            Assert.Equal(("item1", 2), (removedItems[0].item, removedItems[0].index));
            Assert.Equal(("item2", 2), (removedItems[1].item, removedItems[1].index));

            // Verify insert operations with adjusted index (5 - 2 + 1 = 4)
            Assert.Equal(2, insertedItems.Count);
            Assert.Equal(("item1", 4, false), insertedItems[0]);
            Assert.Equal(("item2", 5, false), insertedItems[1]);
        }

        /// <summary>
        /// Tests the generic Apply method with valid parameters and Reset action.
        /// Verifies that the target collection is cleared and items are copied from source.
        /// </summary>
        [Fact]
        public void Apply_WithResetAction_ClearsTargetAndCopiesFromSource()
        {
            // Arrange
            var source = new List<string> { "item1", "item2", "item3" };
            var target = new List<object> { "oldItem1", "oldItem2" };
            var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            // Act
            eventArgs.Apply(source, target);

            // Assert
            Assert.Equal(3, target.Count);
            Assert.Equal("item1", target[0]);
            Assert.Equal("item2", target[1]);
            Assert.Equal("item3", target[2]);
        }

        /// <summary>
        /// Tests the generic Apply method with Add action and valid indices.
        /// Verifies that items are properly inserted into the target collection.
        /// </summary>
        [Fact]
        public void Apply_WithAddAction_InsertsItemsIntoTarget()
        {
            // Arrange
            var source = new List<int> { 1, 2, 3 };
            var target = new List<object> { "existing" };
            var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new object[] { "newItem" }, 1);

            // Act
            eventArgs.Apply(source, target);

            // Assert
            Assert.Equal(2, target.Count);
            Assert.Equal("existing", target[0]);
            Assert.Equal("newItem", target[1]);
        }

        /// <summary>
        /// Tests the generic Apply method with Add action but invalid index.
        /// Verifies that reset behavior is triggered when NewStartingIndex is negative.
        /// </summary>
        [Fact]
        public void Apply_WithAddActionInvalidIndex_TriggersResetBehavior()
        {
            // Arrange
            var source = new List<string> { "source1", "source2" };
            var target = new List<object> { "target1" };
            var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new object[] { "newItem" }, -1);

            // Act
            eventArgs.Apply(source, target);

            // Assert
            Assert.Equal(2, target.Count);
            Assert.Equal("source1", target[0]);
            Assert.Equal("source2", target[1]);
        }

        /// <summary>
        /// Tests the generic Apply method with null NotifyCollectionChangedEventArgs.
        /// Verifies that ArgumentNullException is thrown for null self parameter.
        /// </summary>
        [Fact]
        public void Apply_WithNullEventArgs_ThrowsArgumentNullException()
        {
            // Arrange
            NotifyCollectionChangedEventArgs nullEventArgs = null;
            var source = new List<string>();
            var target = new List<object>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => nullEventArgs.Apply(source, target));
        }

        /// <summary>
        /// Tests the generic Apply method with null source collection.
        /// Verifies that the method handles null source during reset operation.
        /// </summary>
        [Fact]
        public void Apply_WithNullSource_ThrowsNullReferenceExceptionDuringReset()
        {
            // Arrange
            List<string> nullSource = null;
            var target = new List<object> { "existing" };
            var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => eventArgs.Apply(nullSource, target));
        }

        /// <summary>
        /// Tests the generic Apply method with null target collection.
        /// Verifies that NullReferenceException is thrown when target is null.
        /// </summary>
        [Fact]
        public void Apply_WithNullTarget_ThrowsNullReferenceException()
        {
            // Arrange
            var source = new List<string> { "item1" };
            List<object> nullTarget = null;
            var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => eventArgs.Apply(source, nullTarget));
        }

        /// <summary>
        /// Tests the generic Apply method with empty source collection.
        /// Verifies that target is cleared but no items are copied when source is empty.
        /// </summary>
        [Fact]
        public void Apply_WithEmptySource_ClearsTargetOnly()
        {
            // Arrange
            var source = new List<string>();
            var target = new List<object> { "item1", "item2" };
            var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            // Act
            eventArgs.Apply(source, target);

            // Assert
            Assert.Empty(target);
        }

        /// <summary>
        /// Tests the generic Apply method with empty target collection.
        /// Verifies that items from source are properly copied to empty target.
        /// </summary>
        [Fact]
        public void Apply_WithEmptyTarget_CopiesAllItemsFromSource()
        {
            // Arrange
            var source = new List<string> { "item1", "item2" };
            var target = new List<object>();
            var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            // Act
            eventArgs.Apply(source, target);

            // Assert
            Assert.Equal(2, target.Count);
            Assert.Equal("item1", target[0]);
            Assert.Equal("item2", target[1]);
        }

        /// <summary>
        /// Tests the generic Apply method with custom object types.
        /// Verifies that complex types are properly converted from TFrom to object.
        /// </summary>
        [Fact]
        public void Apply_WithCustomObjectTypes_HandlesTypeConversion()
        {
            // Arrange
            var customObjects = new List<CustomTestObject>
            {
                new CustomTestObject { Value = "test1" },
                new CustomTestObject { Value = "test2" }
            };
            var target = new List<object>();
            var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            // Act
            eventArgs.Apply(customObjects, target);

            // Assert
            Assert.Equal(2, target.Count);
            Assert.IsType<CustomTestObject>(target[0]);
            Assert.IsType<CustomTestObject>(target[1]);
            Assert.Equal("test1", ((CustomTestObject)target[0]).Value);
            Assert.Equal("test2", ((CustomTestObject)target[1]).Value);
        }

        /// <summary>
        /// Tests the generic Apply method with Remove action.
        /// Verifies that items are properly removed from target collection.
        /// </summary>
        [Fact]
        public void Apply_WithRemoveAction_RemovesItemsFromTarget()
        {
            // Arrange
            var source = new List<string> { "source1" };
            var target = new List<object> { "item1", "item2", "item3" };
            var eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new object[] { "item2" }, 1);

            // Act
            eventArgs.Apply(source, target);

            // Assert
            Assert.Equal(2, target.Count);
            Assert.Equal("item1", target[0]);
            Assert.Equal("item3", target[1]);
        }

        private class CustomTestObject
        {
            public string Value { get; set; }
        }
    }
}