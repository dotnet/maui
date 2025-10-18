#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class TableSectionTests : BaseTestFixture
    {
        [Fact]
        public void Constructor()
        {
            var section = new TableSection("Title");
            Assert.Equal("Title", section.Title);
            Assert.Empty(section);
        }

        [Fact]
        public void IsReadOnly()
        {
            var section = new TableSection() as ICollection<Cell>;
            Assert.False(section.IsReadOnly);
        }

        [Fact]
        public void Add()
        {
            var section = new TableSection();
            TextCell first, second;
            section.Add(first = new TextCell { Text = "Text" });
            section.Add(second = new TextCell { Text = "Text" });

            Assert.Contains(first, section);
            Assert.Contains(second, section);
        }

        [Fact]
        public void Remove()
        {
            var section = new TableSection();
            TextCell first;
            section.Add(first = new TextCell { Text = "Text" });
            section.Add(new TextCell { Text = "Text" });

            var result = section.Remove(first);
            Assert.True(result);
            Assert.DoesNotContain(first, section);
        }

        [Fact]
        public void Clear()
        {
            var section = new TableSection { new TextCell { Text = "Text" }, new TextCell { Text = "Text" } };
            section.Clear();
            Assert.Empty(section);
        }

        [Fact]
        public void Contains()
        {
            var section = new TableSection();
            TextCell first, second;
            section.Add(first = new TextCell { Text = "Text" });
            section.Add(second = new TextCell { Text = "Text" });

            Assert.Contains(first, section);
            Assert.Contains(second, section);
        }

        [Fact]
        public void IndexOf()
        {
            var section = new TableSection();
            TextCell first, second;
            section.Add(first = new TextCell { Text = "Text" });
            section.Add(second = new TextCell { Text = "Text" });

            Assert.Equal(0, section.IndexOf(first));
            Assert.Equal(1, section.IndexOf(second));
        }

        [Fact]
        public void Insert()
        {
            var section = new TableSection();
            section.Add(new TextCell { Text = "Text" });
            section.Add(new TextCell { Text = "Text" });

            var third = new TextCell { Text = "Text" };
            section.Insert(1, third);
            Assert.Equal(third, section[1]);
        }

        [Fact]
        public void RemoveAt()
        {
            var section = new TableSection();
            TextCell first, second;
            section.Add(first = new TextCell { Text = "Text" });
            section.Add(second = new TextCell { Text = "Text" });

            section.RemoveAt(0);
            Assert.DoesNotContain(first, section);
        }

        [Fact]
        public void Overwrite()
        {
            var section = new TableSection();
            TextCell second;
            section.Add(new TextCell { Text = "Text" });
            section.Add(second = new TextCell { Text = "Text" });

            var third = new TextCell { Text = "Text" };
            section[1] = third;

            Assert.Equal(third, section[1]);
            Assert.DoesNotContain(second, section);
        }

        [Fact]
        public void CopyTo()
        {
            var section = new TableSection();
            TextCell first, second;
            section.Add(first = new TextCell { Text = "Text" });
            section.Add(second = new TextCell { Text = "Text" });

            Cell[] cells = new Cell[2];
            section.CopyTo(cells, 0);

            Assert.Equal(first, cells[0]);
            Assert.Equal(second, cells[1]);
        }

        [Fact]
        public void ChainsBindingContextOnSet()
        {
            var section = new TableSection();
            TextCell first, second;
            section.Add(first = new TextCell { Text = "Text" });
            section.Add(second = new TextCell { Text = "Text" });

            var bindingContext = "bindingContext";

            section.BindingContext = bindingContext;

            Assert.Equal(bindingContext, first.BindingContext);
            Assert.Equal(bindingContext, second.BindingContext);
        }

        [Fact]
        public void ChainsBindingContextWithExistingContext()
        {
            var section = new TableSection();
            TextCell first, second;
            section.Add(first = new TextCell { Text = "Text" });
            section.Add(second = new TextCell { Text = "Text" });

            var bindingContext = "bindingContext";
            section.BindingContext = bindingContext;

            bindingContext = "newContext";
            section.BindingContext = bindingContext;

            Assert.Equal(bindingContext, first.BindingContext);
            Assert.Equal(bindingContext, second.BindingContext);
        }

        [Fact]
        public void ChainsBindingContextToNewlyAdded()
        {
            var section = new TableSection();
            var bindingContext = "bindingContext";
            section.BindingContext = bindingContext;

            TextCell first, second;
            section.Add(first = new TextCell { Text = "Text" });
            section.Add(second = new TextCell { Text = "Text" });

            Assert.Equal(bindingContext, first.BindingContext);
            Assert.Equal(bindingContext, second.BindingContext);
        }

        [Fact]
        public void TestBindingTitleSectionChange()
        {
            var vm = new MockViewModel { Text = "FooBar" };
            var section = new TableSection();

            section.BindingContext = vm;
            section.SetBinding(TableSectionBase.TitleProperty, "Text");

            Assert.Equal("FooBar", section.Title);

            vm.Text = "Baz";

            Assert.Equal("Baz", section.Title);
        }

        [Fact]
        public void TestBindingTitle()
        {
            var section = new TableSection();
            var mock = new MockViewModel();
            section.BindingContext = mock;
            section.SetBinding(TableSection.TitleProperty, new Binding("Text"));

            Assert.Equal(mock.Text, section.Title);
        }

        /// <summary>
        /// Tests that Contains returns true when the specified item exists in the collection.
        /// </summary>
        [Fact]
        public void Contains_ItemExists_ReturnsTrue()
        {
            // Arrange
            var section = new TableSection();
            var cell = new TextCell { Text = "Test" };
            section.Add(cell);

            // Act
            var result = section.Contains(cell);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Contains returns false when the specified item does not exist in the collection.
        /// </summary>
        [Fact]
        public void Contains_ItemDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var section = new TableSection();
            var existingCell = new TextCell { Text = "Existing" };
            var nonExistingCell = new TextCell { Text = "NonExisting" };
            section.Add(existingCell);

            // Act
            var result = section.Contains(nonExistingCell);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains returns false when called on an empty collection.
        /// </summary>
        [Fact]
        public void Contains_EmptyCollection_ReturnsFalse()
        {
            // Arrange
            var section = new TableSection();
            var cell = new TextCell { Text = "Test" };

            // Act
            var result = section.Contains(cell);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains handles null items appropriately.
        /// </summary>
        [Fact]
        public void Contains_NullItem_ReturnsFalse()
        {
            // Arrange
            var section = new TableSection();
            section.Add(new TextCell { Text = "Test" });

            // Act
            var result = section.Contains(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains works correctly with multiple items in the collection.
        /// </summary>
        [Fact]
        public void Contains_MultipleItems_ReturnsCorrectResult()
        {
            // Arrange
            var section = new TableSection();
            var firstCell = new TextCell { Text = "First" };
            var secondCell = new TextCell { Text = "Second" };
            var thirdCell = new TextCell { Text = "Third" };
            var nonExistingCell = new TextCell { Text = "NonExisting" };

            section.Add(firstCell);
            section.Add(secondCell);
            section.Add(thirdCell);

            // Act & Assert
            Assert.True(section.Contains(firstCell));
            Assert.True(section.Contains(secondCell));
            Assert.True(section.Contains(thirdCell));
            Assert.False(section.Contains(nonExistingCell));
        }

        /// <summary>
        /// Tests that Contains returns false after an item has been removed from the collection.
        /// </summary>
        [Fact]
        public void Contains_RemovedItem_ReturnsFalse()
        {
            // Arrange
            var section = new TableSection();
            var cell = new TextCell { Text = "Test" };
            section.Add(cell);
            section.Remove(cell);

            // Act
            var result = section.Contains(cell);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Contains uses reference equality, not value equality.
        /// </summary>
        [Fact]
        public void Contains_DifferentObjectsSameContent_ReturnsFalse()
        {
            // Arrange
            var section = new TableSection();
            var cell1 = new TextCell { Text = "Same Content" };
            var cell2 = new TextCell { Text = "Same Content" };
            section.Add(cell1);

            // Act
            var result = section.Contains(cell2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the CollectionChanged event is raised when adding an item to the TableSection.
        /// Verifies that the event args contain the correct action, new item, and index.
        /// </summary>
        [Fact]
        public void CollectionChanged_Add_RaisesEventWithCorrectArgs()
        {
            // Arrange
            var section = new TableSection();
            var cell = new TextCell { Text = "Test" };
            NotifyCollectionChangedEventArgs receivedArgs = null;
            int eventCallCount = 0;

            section.CollectionChanged += (sender, args) =>
            {
                receivedArgs = args;
                eventCallCount++;
            };

            // Act
            section.Add(cell);

            // Assert
            Assert.Equal(1, eventCallCount);
            Assert.NotNull(receivedArgs);
            Assert.Equal(NotifyCollectionChangedAction.Add, receivedArgs.Action);
            Assert.Single(receivedArgs.NewItems);
            Assert.Equal(cell, receivedArgs.NewItems[0]);
            Assert.Equal(0, receivedArgs.NewStartingIndex);
            Assert.Null(receivedArgs.OldItems);
        }

        /// <summary>
        /// Tests that the CollectionChanged event is raised when removing an item from the TableSection.
        /// Verifies that the event args contain the correct action, old item, and index.
        /// </summary>
        [Fact]
        public void CollectionChanged_Remove_RaisesEventWithCorrectArgs()
        {
            // Arrange
            var section = new TableSection();
            var cell = new TextCell { Text = "Test" };
            section.Add(cell);

            NotifyCollectionChangedEventArgs receivedArgs = null;
            int eventCallCount = 0;

            section.CollectionChanged += (sender, args) =>
            {
                receivedArgs = args;
                eventCallCount++;
            };

            // Act
            bool result = section.Remove(cell);

            // Assert
            Assert.True(result);
            Assert.Equal(1, eventCallCount);
            Assert.NotNull(receivedArgs);
            Assert.Equal(NotifyCollectionChangedAction.Remove, receivedArgs.Action);
            Assert.Single(receivedArgs.OldItems);
            Assert.Equal(cell, receivedArgs.OldItems[0]);
            Assert.Equal(0, receivedArgs.OldStartingIndex);
            Assert.Null(receivedArgs.NewItems);
        }

        /// <summary>
        /// Tests that the CollectionChanged event is raised when clearing the TableSection.
        /// Verifies that the event args contain the Reset action.
        /// </summary>
        [Fact]
        public void CollectionChanged_Clear_RaisesEventWithCorrectArgs()
        {
            // Arrange
            var section = new TableSection();
            section.Add(new TextCell { Text = "Test1" });
            section.Add(new TextCell { Text = "Test2" });

            NotifyCollectionChangedEventArgs receivedArgs = null;
            int eventCallCount = 0;

            section.CollectionChanged += (sender, args) =>
            {
                receivedArgs = args;
                eventCallCount++;
            };

            // Act
            section.Clear();

            // Assert
            Assert.Equal(1, eventCallCount);
            Assert.NotNull(receivedArgs);
            Assert.Equal(NotifyCollectionChangedAction.Reset, receivedArgs.Action);
            Assert.Null(receivedArgs.NewItems);
            Assert.Null(receivedArgs.OldItems);
        }

        /// <summary>
        /// Tests that the CollectionChanged event is raised when inserting an item at a specific index.
        /// Verifies that the event args contain the correct action, new item, and index.
        /// </summary>
        [Fact]
        public void CollectionChanged_Insert_RaisesEventWithCorrectArgs()
        {
            // Arrange
            var section = new TableSection();
            section.Add(new TextCell { Text = "Existing" });

            var newCell = new TextCell { Text = "Inserted" };
            NotifyCollectionChangedEventArgs receivedArgs = null;
            int eventCallCount = 0;

            section.CollectionChanged += (sender, args) =>
            {
                receivedArgs = args;
                eventCallCount++;
            };

            // Act
            section.Insert(0, newCell);

            // Assert
            Assert.Equal(1, eventCallCount);
            Assert.NotNull(receivedArgs);
            Assert.Equal(NotifyCollectionChangedAction.Add, receivedArgs.Action);
            Assert.Single(receivedArgs.NewItems);
            Assert.Equal(newCell, receivedArgs.NewItems[0]);
            Assert.Equal(0, receivedArgs.NewStartingIndex);
            Assert.Null(receivedArgs.OldItems);
        }

        /// <summary>
        /// Tests that the CollectionChanged event is raised when removing an item at a specific index.
        /// Verifies that the event args contain the correct action, old item, and index.
        /// </summary>
        [Fact]
        public void CollectionChanged_RemoveAt_RaisesEventWithCorrectArgs()
        {
            // Arrange
            var section = new TableSection();
            var cell = new TextCell { Text = "Test" };
            section.Add(cell);

            NotifyCollectionChangedEventArgs receivedArgs = null;
            int eventCallCount = 0;

            section.CollectionChanged += (sender, args) =>
            {
                receivedArgs = args;
                eventCallCount++;
            };

            // Act
            section.RemoveAt(0);

            // Assert
            Assert.Equal(1, eventCallCount);
            Assert.NotNull(receivedArgs);
            Assert.Equal(NotifyCollectionChangedAction.Remove, receivedArgs.Action);
            Assert.Single(receivedArgs.OldItems);
            Assert.Equal(cell, receivedArgs.OldItems[0]);
            Assert.Equal(0, receivedArgs.OldStartingIndex);
            Assert.Null(receivedArgs.NewItems);
        }

        /// <summary>
        /// Tests that the CollectionChanged event is raised when setting an item via the indexer.
        /// Verifies that the event args contain the correct action for replacement.
        /// </summary>
        [Fact]
        public void CollectionChanged_IndexerSet_RaisesEventWithCorrectArgs()
        {
            // Arrange
            var section = new TableSection();
            var oldCell = new TextCell { Text = "Old" };
            var newCell = new TextCell { Text = "New" };
            section.Add(oldCell);

            NotifyCollectionChangedEventArgs receivedArgs = null;
            int eventCallCount = 0;

            section.CollectionChanged += (sender, args) =>
            {
                receivedArgs = args;
                eventCallCount++;
            };

            // Act
            section[0] = newCell;

            // Assert
            Assert.Equal(1, eventCallCount);
            Assert.NotNull(receivedArgs);
            Assert.Equal(NotifyCollectionChangedAction.Replace, receivedArgs.Action);
            Assert.Single(receivedArgs.NewItems);
            Assert.Single(receivedArgs.OldItems);
            Assert.Equal(newCell, receivedArgs.NewItems[0]);
            Assert.Equal(oldCell, receivedArgs.OldItems[0]);
            Assert.Equal(0, receivedArgs.NewStartingIndex);
            Assert.Equal(0, receivedArgs.OldStartingIndex);
        }

        /// <summary>
        /// Tests that multiple subscribers to CollectionChanged all receive the event.
        /// Verifies that the event is properly forwarded to all registered handlers.
        /// </summary>
        [Fact]
        public void CollectionChanged_MultipleSubscribers_AllReceiveEvent()
        {
            // Arrange
            var section = new TableSection();
            var cell = new TextCell { Text = "Test" };

            int handler1CallCount = 0;
            int handler2CallCount = 0;
            NotifyCollectionChangedEventArgs handler1Args = null;
            NotifyCollectionChangedEventArgs handler2Args = null;

            section.CollectionChanged += (sender, args) =>
            {
                handler1CallCount++;
                handler1Args = args;
            };

            section.CollectionChanged += (sender, args) =>
            {
                handler2CallCount++;
                handler2Args = args;
            };

            // Act
            section.Add(cell);

            // Assert
            Assert.Equal(1, handler1CallCount);
            Assert.Equal(1, handler2CallCount);
            Assert.NotNull(handler1Args);
            Assert.NotNull(handler2Args);
            Assert.Equal(NotifyCollectionChangedAction.Add, handler1Args.Action);
            Assert.Equal(NotifyCollectionChangedAction.Add, handler2Args.Action);
            Assert.Equal(cell, handler1Args.NewItems[0]);
            Assert.Equal(cell, handler2Args.NewItems[0]);
        }

        /// <summary>
        /// Tests that CollectionChanged event passes the correct sender (the TableSection instance).
        /// Verifies that the sender parameter in the event handler is the TableSection that raised the event.
        /// </summary>
        [Fact]
        public void CollectionChanged_EventSender_IsTableSectionInstance()
        {
            // Arrange
            var section = new TableSection();
            var cell = new TextCell { Text = "Test" };
            object receivedSender = null;

            section.CollectionChanged += (sender, args) =>
            {
                receivedSender = sender;
            };

            // Act
            section.Add(cell);

            // Assert
            Assert.Equal(section, receivedSender);
        }

        /// <summary>
        /// Tests that no CollectionChanged event is raised when no items are added to an empty collection.
        /// Verifies that the event is not unnecessarily triggered for operations that don't change the collection.
        /// </summary>
        [Fact]
        public void CollectionChanged_NoChange_NoEventRaised()
        {
            // Arrange
            var section = new TableSection();
            var nonExistentCell = new TextCell { Text = "Test" };
            int eventCallCount = 0;

            section.CollectionChanged += (sender, args) =>
            {
                eventCallCount++;
            };

            // Act - try to remove item that doesn't exist
            bool result = section.Remove(nonExistentCell);

            // Assert
            Assert.False(result);
            Assert.Equal(0, eventCallCount);
        }
    }

    public partial class TableSectionBaseTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that the parameterless TableSectionBase constructor completes successfully without throwing exceptions.
        /// Verifies basic initialization and that the object is in a valid state after construction.
        /// Expected result: Constructor completes successfully and object is properly initialized.
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstructor_InitializesSuccessfully()
        {
            // Arrange & Act
            var section = new TableSection();

            // Assert
            Assert.NotNull(section);
            Assert.Empty(section);
            Assert.Equal(0, section.Count);
        }

        /// <summary>
        /// Tests that the parameterless TableSectionBase constructor properly subscribes to the CollectionChanged event.
        /// Verifies that the OnChildrenChanged event handler is attached by testing binding context propagation.
        /// Expected result: Event handler is properly subscribed and binding context is propagated to new items.
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstructor_SubscribesToCollectionChangedEvent()
        {
            // Arrange
            var section = new TableSection();
            var bindingContext = new object();
            section.BindingContext = bindingContext;
            var cell = new TextCell();

            // Act
            section.Add(cell);

            // Assert
            Assert.Equal(bindingContext, cell.BindingContext);
        }

        /// <summary>
        /// Tests that the parameterless TableSectionBase constructor allows the collection to function properly.
        /// Verifies that basic collection operations work correctly after construction.
        /// Expected result: Collection operations work as expected, indicating proper initialization.
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstructor_AllowsCollectionOperations()
        {
            // Arrange
            var section = new TableSection();
            var cell1 = new TextCell { Text = "Cell 1" };
            var cell2 = new TextCell { Text = "Cell 2" };

            // Act
            section.Add(cell1);
            section.Add(cell2);

            // Assert
            Assert.Equal(2, section.Count);
            Assert.Contains(cell1, section);
            Assert.Contains(cell2, section);
            Assert.Equal(cell1, section[0]);
            Assert.Equal(cell2, section[1]);
        }

        /// <summary>
        /// Tests that the parameterless TableSectionBase constructor properly handles CollectionChanged event firing.
        /// Verifies that the internal event mechanism works correctly.
        /// Expected result: CollectionChanged event is fired when items are added to the collection.
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstructor_CollectionChangedEventFires()
        {
            // Arrange
            var section = new TableSection();
            var eventFired = false;
            NotifyCollectionChangedEventArgs capturedEventArgs = null;

            section.CollectionChanged += (sender, e) =>
            {
                eventFired = true;
                capturedEventArgs = e;
            };

            var cell = new TextCell();

            // Act
            section.Add(cell);

            // Assert
            Assert.True(eventFired);
            Assert.NotNull(capturedEventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Add, capturedEventArgs.Action);
            Assert.Contains(cell, capturedEventArgs.NewItems);
        }

        /// <summary>
        /// Tests that multiple instances created with the parameterless TableSectionBase constructor are independent.
        /// Verifies that each instance has its own collection and event handlers.
        /// Expected result: Each instance operates independently without affecting others.
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstructor_CreatesIndependentInstances()
        {
            // Arrange
            var section1 = new TableSection();
            var section2 = new TableSection();
            var cell1 = new TextCell { Text = "Cell 1" };
            var cell2 = new TextCell { Text = "Cell 2" };

            // Act
            section1.Add(cell1);
            section2.Add(cell2);

            // Assert
            Assert.Equal(1, section1.Count);
            Assert.Equal(1, section2.Count);
            Assert.Contains(cell1, section1);
            Assert.DoesNotContain(cell1, section2);
            Assert.Contains(cell2, section2);
            Assert.DoesNotContain(cell2, section1);
        }

        /// <summary>
        /// Tests that Add(IEnumerable) successfully adds multiple items from an array to the section.
        /// Verifies that all items are contained in the section after adding.
        /// </summary>
        [Fact]
        public void AddIEnumerable_WithArray_AddsAllItems()
        {
            // Arrange
            var section = new TableSection();
            var cells = new TextCell[]
            {
                new TextCell { Text = "First" },
                new TextCell { Text = "Second" },
                new TextCell { Text = "Third" }
            };

            // Act
            section.Add(cells);

            // Assert
            Assert.Equal(3, section.Count);
            Assert.Contains(cells[0], section);
            Assert.Contains(cells[1], section);
            Assert.Contains(cells[2], section);
        }

        /// <summary>
        /// Tests that Add(IEnumerable) successfully adds multiple items from a List to the section.
        /// Verifies that items maintain their order after adding.
        /// </summary>
        [Fact]
        public void AddIEnumerable_WithList_AddsAllItemsInOrder()
        {
            // Arrange
            var section = new TableSection();
            var cells = new List<TextCell>
            {
                new TextCell { Text = "First" },
                new TextCell { Text = "Second" },
                new TextCell { Text = "Third" }
            };

            // Act
            section.Add(cells);

            // Assert
            Assert.Equal(3, section.Count);
            Assert.Equal(cells[0], section[0]);
            Assert.Equal(cells[1], section[1]);
            Assert.Equal(cells[2], section[2]);
        }

        /// <summary>
        /// Tests that Add(IEnumerable) with an empty collection does not add any items.
        /// Verifies that the section remains empty.
        /// </summary>
        [Fact]
        public void AddIEnumerable_WithEmptyCollection_AddsNoItems()
        {
            // Arrange
            var section = new TableSection();
            var emptyCells = new TextCell[0];

            // Act
            section.Add(emptyCells);

            // Assert
            Assert.Empty(section);
        }

        /// <summary>
        /// Tests that Add(IEnumerable) with a null collection throws ArgumentNullException.
        /// Verifies proper null input validation.
        /// </summary>
        [Fact]
        public void AddIEnumerable_WithNullCollection_ThrowsArgumentNullException()
        {
            // Arrange
            var section = new TableSection();
            IEnumerable<TextCell> nullCells = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => section.Add(nullCells));
        }

        /// <summary>
        /// Tests that Add(IEnumerable) adds items to existing collection content.
        /// Verifies that new items are appended to already existing items.
        /// </summary>
        [Fact]
        public void AddIEnumerable_WithExistingItems_AppendsNewItems()
        {
            // Arrange
            var section = new TableSection();
            var existingCell = new TextCell { Text = "Existing" };
            section.Add(existingCell);

            var newCells = new TextCell[]
            {
                new TextCell { Text = "New1" },
                new TextCell { Text = "New2" }
            };

            // Act
            section.Add(newCells);

            // Assert
            Assert.Equal(3, section.Count);
            Assert.Equal(existingCell, section[0]);
            Assert.Equal(newCells[0], section[1]);
            Assert.Equal(newCells[1], section[2]);
        }

        /// <summary>
        /// Tests that Add(IEnumerable) triggers CollectionChanged event for each added item.
        /// Verifies that the ObservableCollection properly notifies observers of changes.
        /// </summary>
        [Fact]
        public void AddIEnumerable_WithMultipleItems_TriggersCollectionChangedEvents()
        {
            // Arrange
            var section = new TableSection();
            var collectionChangedCount = 0;
            section.CollectionChanged += (sender, e) => collectionChangedCount++;

            var cells = new TextCell[]
            {
                new TextCell { Text = "First" },
                new TextCell { Text = "Second" }
            };

            // Act
            section.Add(cells);

            // Assert
            Assert.Equal(2, collectionChangedCount);
            Assert.Equal(2, section.Count);
        }

        /// <summary>
        /// Tests that Add(IEnumerable) works with LINQ-generated IEnumerables.
        /// Verifies compatibility with different IEnumerable implementations.
        /// </summary>
        [Fact]
        public void AddIEnumerable_WithLinqEnumerable_AddsAllItems()
        {
            // Arrange
            var section = new TableSection();
            var sourceTexts = new[] { "Item1", "Item2", "Item3" };
            var cells = sourceTexts.Select(text => new TextCell { Text = text });

            // Act
            section.Add(cells);

            // Assert
            Assert.Equal(3, section.Count);
            Assert.Equal("Item1", ((TextCell)section[0]).Text);
            Assert.Equal("Item2", ((TextCell)section[1]).Text);
            Assert.Equal("Item3", ((TextCell)section[2]).Text);
        }

        /// <summary>
        /// Tests the TableSectionBase constructor with a title parameter to verify proper initialization.
        /// Input: Valid string title.
        /// Expected: Constructor completes successfully, title is set, collection is empty, and event subscription is established.
        /// </summary>
        [Theory]
        [InlineData("Test Title")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Title with special chars: !@#$%^&*()")]
        [InlineData("Very long title that contains many characters to test boundary conditions and ensure the constructor handles long strings properly without issues")]
        public void Constructor_WithValidTitle_InitializesCorrectly(string title)
        {
            // Arrange & Act
            var section = new TableSection(title);

            // Assert
            Assert.Equal(title, section.Title);
            Assert.Empty(section);
            Assert.Equal(0, section.Count);
        }

        /// <summary>
        /// Tests the TableSectionBase constructor with null title parameter.
        /// Input: null title value.
        /// Expected: Constructor completes successfully and handles null title appropriately.
        /// </summary>
        [Fact]
        public void Constructor_WithNullTitle_InitializesCorrectly()
        {
            // Arrange & Act
            var section = new TableSection(null);

            // Assert
            Assert.Null(section.Title);
            Assert.Empty(section);
            Assert.Equal(0, section.Count);
        }

        /// <summary>
        /// Tests that the TableSectionBase constructor properly sets up event subscription for collection changes.
        /// Input: Valid title and subsequent item addition.
        /// Expected: Event subscription works and binding context is propagated to added items.
        /// </summary>
        [Fact]
        public void Constructor_WithTitle_EstablishesEventSubscription()
        {
            // Arrange
            var section = new TableSection("Test");
            var bindingContext = new object();
            section.BindingContext = bindingContext;

            // Act
            var cell = new TextCell { Text = "Test Cell" };
            section.Add(cell);

            // Assert
            Assert.Equal(bindingContext, cell.BindingContext);
        }

        /// <summary>
        /// Tests that the TableSectionBase constructor allows multiple items to be added and maintains proper event handling.
        /// Input: Valid title and multiple item additions.
        /// Expected: All items are properly added and binding context is propagated to all items.
        /// </summary>
        [Fact]
        public void Constructor_WithTitle_HandlesMultipleItemAdditions()
        {
            // Arrange
            var section = new TableSection("Multi-Item Test");
            var bindingContext = new object();
            section.BindingContext = bindingContext;

            // Act
            var cell1 = new TextCell { Text = "Cell 1" };
            var cell2 = new TextCell { Text = "Cell 2" };
            section.Add(cell1);
            section.Add(cell2);

            // Assert
            Assert.Equal(2, section.Count);
            Assert.Equal(bindingContext, cell1.BindingContext);
            Assert.Equal(bindingContext, cell2.BindingContext);
        }

        /// <summary>
        /// Tests that the TableSectionBase constructor works correctly when binding context is set after items are added.
        /// Input: Valid title, items added first, then binding context set.
        /// Expected: Existing items do not automatically get the new binding context (event only handles new additions).
        /// </summary>
        [Fact]
        public void Constructor_WithTitle_BindingContextSetAfterItemsAdded()
        {
            // Arrange
            var section = new TableSection("Test");
            var cell = new TextCell { Text = "Test Cell" };
            section.Add(cell);

            // Act
            var bindingContext = new object();
            section.BindingContext = bindingContext;

            // Assert - existing items don't automatically get new binding context
            Assert.NotEqual(bindingContext, cell.BindingContext);

            // But new items should get it
            var newCell = new TextCell { Text = "New Cell" };
            section.Add(newCell);
            Assert.Equal(bindingContext, newCell.BindingContext);
        }
    }
}