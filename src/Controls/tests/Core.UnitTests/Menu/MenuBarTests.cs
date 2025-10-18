#nullable disable

#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Menu
{
    [Category("MenuBar")]
    public class MenuBarTests :
        MenuTestBase<MenuBar, IMenuBarItem, MenuBarItem, MenuBarHandlerUpdate>
    {
        protected override int GetIndex(MenuBarHandlerUpdate handlerUpdate) =>
            handlerUpdate.Index;

        protected override IMenuBarItem GetItem(MenuBarHandlerUpdate handlerUpdate) =>
            handlerUpdate.MenuBarItem;

        protected override void SetHandler(
            Maui.IElement element, List<(string Name, MenuBarHandlerUpdate? Args)> events)
        {
            element.Handler = CreateMenuBarHandler((n, h, l, a) => events.Add((n, a)));
        }

        MenuBarHandler CreateMenuBarHandler(Action<string, IMenuBarHandler, IMenuBar, MenuBarHandlerUpdate?>? action)
        {
            var handler = new NonThrowingMenuBarHandler(
                MenuBarHandler.Mapper,
                new CommandMapper<IMenuBar, IMenuBarHandler>(MenuBarHandler.CommandMapper)
                {
                    [nameof(IMenuBarHandler.Add)] = (h, l, a) => action?.Invoke(nameof(IMenuBarHandler.Add), h, l, (MenuBarHandlerUpdate?)a),
                    [nameof(IMenuBarHandler.Remove)] = (h, l, a) => action?.Invoke(nameof(IMenuBarHandler.Remove), h, l, (MenuBarHandlerUpdate?)a),
                    [nameof(IMenuBarHandler.Clear)] = (h, l, a) => action?.Invoke(nameof(IMenuBarHandler.Clear), h, l, (MenuBarHandlerUpdate?)a),
                    [nameof(IMenuBarHandler.Insert)] = (h, l, a) => action?.Invoke(nameof(IMenuBarHandler.Insert), h, l, (MenuBarHandlerUpdate?)a),
                });

            return handler;
        }

        [Fact]
        public void UsingWindowDoesNotReAssignParents()
        {
            MenuFlyoutItem flyout;
            MenuBarItem menuItem;

            var page = new ContentPage
            {
                MenuBarItems =
                {
                    (menuItem = new MenuBarItem
                    {
                        (flyout = new MenuFlyoutItem { })
                    })
                }
            };

            Assert.Equal(menuItem, flyout.Parent);
            Assert.Equal(page, menuItem.Parent);

            var window = new Window(page);

            Assert.Equal(menuItem, flyout.Parent);
            Assert.Equal(page, menuItem.Parent);
            Assert.Equal(window, page.Parent);

            var menubar = (window as IMenuBarElement).MenuBar;
            Assert.NotNull(menubar);

            Assert.Equal(menuItem, flyout.Parent);
            Assert.Equal(page, menuItem.Parent);
            Assert.Equal(window, page.Parent);
        }

        [Fact]
        public void UsingWindowDoesNotReAssignBindingContext()
        {
            var bindingContext = new
            {
                Name = "Matthew"
            };

            MenuFlyoutItem flyout;
            MenuBarItem menuItem;

            var page = new ContentPage
            {
                BindingContext = bindingContext,
                MenuBarItems =
                {
                    (menuItem = new MenuBarItem
                    {
                        (flyout = new MenuFlyoutItem { })
                    })
                }
            };

            flyout.SetBinding(MenuFlyoutItem.TextProperty, new Binding(nameof(bindingContext.Name)));

            Assert.Equal(bindingContext.Name, flyout.Text);

            var window = new Window(page);

            Assert.Equal(bindingContext.Name, flyout.Text);

            var menubar = (window as IMenuBarElement).MenuBar;
            Assert.NotNull(menubar);

            Assert.Equal(bindingContext.Name, flyout.Text);
        }

        class NonThrowingMenuBarHandler : MenuBarHandler
        {
            public NonThrowingMenuBarHandler(IPropertyMapper mapper, CommandMapper commandMapper)
                : base(mapper, commandMapper)
            {
            }

            protected override object CreatePlatformElement() => new object();
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when searching for a null item.
        /// </summary>
        [Fact]
        public void IndexOf_NullItem_ReturnsNegativeOne()
        {
            // Arrange
            var menuBar = new MenuBar();
            var mockItem = Substitute.For<IMenuBarItem>();
            menuBar.Add(mockItem);

            // Act
            var result = menuBar.IndexOf(null);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when searching for an item that doesn't exist in an empty collection.
        /// </summary>
        [Fact]
        public void IndexOf_ItemNotInEmptyCollection_ReturnsNegativeOne()
        {
            // Arrange
            var menuBar = new MenuBar();
            var mockItem = Substitute.For<IMenuBarItem>();

            // Act
            var result = menuBar.IndexOf(mockItem);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns -1 when searching for an item that doesn't exist in a populated collection.
        /// </summary>
        [Fact]
        public void IndexOf_ItemNotInPopulatedCollection_ReturnsNegativeOne()
        {
            // Arrange
            var menuBar = new MenuBar();
            var existingItem = Substitute.For<IMenuBarItem>();
            var nonExistentItem = Substitute.For<IMenuBarItem>();
            menuBar.Add(existingItem);

            // Act
            var result = menuBar.IndexOf(nonExistentItem);

            // Assert
            Assert.Equal(-1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns the correct index for the first item in the collection.
        /// </summary>
        [Fact]
        public void IndexOf_FirstItem_ReturnsZero()
        {
            // Arrange
            var menuBar = new MenuBar();
            var firstItem = Substitute.For<IMenuBarItem>();
            var secondItem = Substitute.For<IMenuBarItem>();
            menuBar.Add(firstItem);
            menuBar.Add(secondItem);

            // Act
            var result = menuBar.IndexOf(firstItem);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that IndexOf returns the correct index for the last item in the collection.
        /// </summary>
        [Fact]
        public void IndexOf_LastItem_ReturnsCorrectIndex()
        {
            // Arrange
            var menuBar = new MenuBar();
            var firstItem = Substitute.For<IMenuBarItem>();
            var secondItem = Substitute.For<IMenuBarItem>();
            var thirdItem = Substitute.For<IMenuBarItem>();
            menuBar.Add(firstItem);
            menuBar.Add(secondItem);
            menuBar.Add(thirdItem);

            // Act
            var result = menuBar.IndexOf(thirdItem);

            // Assert
            Assert.Equal(2, result);
        }

        /// <summary>
        /// Tests that IndexOf returns the correct index for a middle item in the collection.
        /// </summary>
        [Fact]
        public void IndexOf_MiddleItem_ReturnsCorrectIndex()
        {
            // Arrange
            var menuBar = new MenuBar();
            var firstItem = Substitute.For<IMenuBarItem>();
            var middleItem = Substitute.For<IMenuBarItem>();
            var lastItem = Substitute.For<IMenuBarItem>();
            menuBar.Add(firstItem);
            menuBar.Add(middleItem);
            menuBar.Add(lastItem);

            // Act
            var result = menuBar.IndexOf(middleItem);

            // Assert
            Assert.Equal(1, result);
        }

        /// <summary>
        /// Tests that IndexOf returns the index of the first occurrence when the same item is added multiple times.
        /// </summary>
        [Fact]
        public void IndexOf_DuplicateItems_ReturnsFirstOccurrence()
        {
            // Arrange
            var menuBar = new MenuBar();
            var duplicateItem = Substitute.For<IMenuBarItem>();
            var otherItem = Substitute.For<IMenuBarItem>();
            menuBar.Add(duplicateItem);
            menuBar.Add(otherItem);
            menuBar.Add(duplicateItem);

            // Act
            var result = menuBar.IndexOf(duplicateItem);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that IndexOf returns the correct index when only one item exists in the collection.
        /// </summary>
        [Fact]
        public void IndexOf_SingleItemCollection_ReturnsZero()
        {
            // Arrange
            var menuBar = new MenuBar();
            var singleItem = Substitute.For<IMenuBarItem>();
            menuBar.Add(singleItem);

            // Act
            var result = menuBar.IndexOf(singleItem);

            // Assert
            Assert.Equal(0, result);
        }

        /// <summary>
        /// Tests that the IsReadOnly property always returns false, indicating that the MenuBar collection is mutable.
        /// </summary>
        [Fact]
        public void IsReadOnly_WhenAccessed_ReturnsFalse()
        {
            // Arrange
            var menuBar = new MenuBar();

            // Act
            var isReadOnly = menuBar.IsReadOnly;

            // Assert
            Assert.False(isReadOnly);
        }

        /// <summary>
        /// Tests that SyncMenuBarItemsFromPages throws ArgumentNullException when menuBarItems is null.
        /// </summary>
        [Fact]
        public void SyncMenuBarItemsFromPages_NullMenuBarItems_ThrowsArgumentNullException()
        {
            // Arrange
            var menuBar = new MenuBar();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => menuBar.SyncMenuBarItemsFromPages(null));
        }

        /// <summary>
        /// Tests that SyncMenuBarItemsFromPages clears the MenuBar when given an empty list.
        /// </summary>
        [Fact]
        public void SyncMenuBarItemsFromPages_EmptyList_ClearsMenuBar()
        {
            // Arrange
            var menuBar = new MenuBar();
            var existingItem1 = new MenuBarItem { Text = "Item1" };
            var existingItem2 = new MenuBarItem { Text = "Item2" };
            menuBar.Add(existingItem1);
            menuBar.Add(existingItem2);
            var emptyList = new List<MenuBarItem>();

            // Act
            menuBar.SyncMenuBarItemsFromPages(emptyList);

            // Assert
            Assert.Equal(0, menuBar.Count);
        }

        /// <summary>
        /// Tests that SyncMenuBarItemsFromPages makes no changes when items are already in the correct order.
        /// </summary>
        [Fact]
        public void SyncMenuBarItemsFromPages_SameItemsSameOrder_NoChanges()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item1 = new MenuBarItem { Text = "Item1" };
            var item2 = new MenuBarItem { Text = "Item2" };
            menuBar.Add(item1);
            menuBar.Add(item2);
            var menuBarItems = new List<MenuBarItem> { item1, item2 };

            // Act
            menuBar.SyncMenuBarItemsFromPages(menuBarItems);

            // Assert
            Assert.Equal(2, menuBar.Count);
            Assert.Same(item1, menuBar[0]);
            Assert.Same(item2, menuBar[1]);
        }

        /// <summary>
        /// Tests that SyncMenuBarItemsFromPages reorders items when they exist but are in different positions.
        /// This test specifically exercises the Contains and Remove code path to cover uncovered line 49.
        /// </summary>
        [Fact]
        public void SyncMenuBarItemsFromPages_ItemsInDifferentOrder_ReordersItems()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item1 = new MenuBarItem { Text = "Item1" };
            var item2 = new MenuBarItem { Text = "Item2" };
            var item3 = new MenuBarItem { Text = "Item3" };
            menuBar.Add(item1);
            menuBar.Add(item2);
            menuBar.Add(item3);

            // Reorder: item3, item1, item2
            var menuBarItems = new List<MenuBarItem> { item3, item1, item2 };

            // Act
            menuBar.SyncMenuBarItemsFromPages(menuBarItems);

            // Assert
            Assert.Equal(3, menuBar.Count);
            Assert.Same(item3, menuBar[0]);
            Assert.Same(item1, menuBar[1]);
            Assert.Same(item2, menuBar[2]);
        }

        /// <summary>
        /// Tests that SyncMenuBarItemsFromPages adds new items that don't exist in the MenuBar.
        /// </summary>
        [Fact]
        public void SyncMenuBarItemsFromPages_NewItems_AddsItems()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item1 = new MenuBarItem { Text = "Item1" };
            var item2 = new MenuBarItem { Text = "Item2" };
            var menuBarItems = new List<MenuBarItem> { item1, item2 };

            // Act
            menuBar.SyncMenuBarItemsFromPages(menuBarItems);

            // Assert
            Assert.Equal(2, menuBar.Count);
            Assert.Same(item1, menuBar[0]);
            Assert.Same(item2, menuBar[1]);
        }

        /// <summary>
        /// Tests that SyncMenuBarItemsFromPages removes excess items when the input list is shorter.
        /// </summary>
        [Fact]
        public void SyncMenuBarItemsFromPages_FewerItems_RemovesExcessItems()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item1 = new MenuBarItem { Text = "Item1" };
            var item2 = new MenuBarItem { Text = "Item2" };
            var item3 = new MenuBarItem { Text = "Item3" };
            menuBar.Add(item1);
            menuBar.Add(item2);
            menuBar.Add(item3);
            var menuBarItems = new List<MenuBarItem> { item1 };

            // Act
            menuBar.SyncMenuBarItemsFromPages(menuBarItems);

            // Assert
            Assert.Equal(1, menuBar.Count);
            Assert.Same(item1, menuBar[0]);
        }

        /// <summary>
        /// Tests that SyncMenuBarItemsFromPages handles complex scenarios with mixed adding, removing, and reordering.
        /// </summary>
        [Fact]
        public void SyncMenuBarItemsFromPages_MixedScenario_HandlesComplexChanges()
        {
            // Arrange
            var menuBar = new MenuBar();
            var existingItem1 = new MenuBarItem { Text = "Existing1" };
            var existingItem2 = new MenuBarItem { Text = "Existing2" };
            var existingItem3 = new MenuBarItem { Text = "Existing3" };
            menuBar.Add(existingItem1);
            menuBar.Add(existingItem2);
            menuBar.Add(existingItem3);

            var newItem1 = new MenuBarItem { Text = "New1" };
            var newItem2 = new MenuBarItem { Text = "New2" };

            // New arrangement: newItem1, existingItem3, newItem2, existingItem1
            // This removes existingItem2, adds new items, and reorders
            var menuBarItems = new List<MenuBarItem> { newItem1, existingItem3, newItem2, existingItem1 };

            // Act
            menuBar.SyncMenuBarItemsFromPages(menuBarItems);

            // Assert
            Assert.Equal(4, menuBar.Count);
            Assert.Same(newItem1, menuBar[0]);
            Assert.Same(existingItem3, menuBar[1]);
            Assert.Same(newItem2, menuBar[2]);
            Assert.Same(existingItem1, menuBar[3]);
        }

        /// <summary>
        /// Tests that SyncMenuBarItemsFromPages works correctly when starting with an empty MenuBar.
        /// </summary>
        [Fact]
        public void SyncMenuBarItemsFromPages_EmptyMenuBarWithItems_AddsAllItems()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item1 = new MenuBarItem { Text = "Item1" };
            var item2 = new MenuBarItem { Text = "Item2" };
            var item3 = new MenuBarItem { Text = "Item3" };
            var menuBarItems = new List<MenuBarItem> { item1, item2, item3 };

            // Act
            menuBar.SyncMenuBarItemsFromPages(menuBarItems);

            // Assert
            Assert.Equal(3, menuBar.Count);
            Assert.Same(item1, menuBar[0]);
            Assert.Same(item2, menuBar[1]);
            Assert.Same(item3, menuBar[2]);
        }

        /// <summary>
        /// Tests that SyncMenuBarItemsFromPages handles duplicate items in the input list correctly.
        /// </summary>
        [Fact]
        public void SyncMenuBarItemsFromPages_DuplicateItemsInList_HandlesCorrectly()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item1 = new MenuBarItem { Text = "Item1" };
            var item2 = new MenuBarItem { Text = "Item2" };

            // List with duplicate item1
            var menuBarItems = new List<MenuBarItem> { item1, item2, item1 };

            // Act
            menuBar.SyncMenuBarItemsFromPages(menuBarItems);

            // Assert
            Assert.Equal(3, menuBar.Count);
            Assert.Same(item1, menuBar[0]);
            Assert.Same(item2, menuBar[1]);
            Assert.Same(item1, menuBar[2]);
        }

        /// <summary>
        /// Tests that SyncMenuBarItemsFromPages handles the scenario where an item needs to be moved to the beginning.
        /// This specifically tests the Contains/Remove path for repositioning items.
        /// </summary>
        [Fact]
        public void SyncMenuBarItemsFromPages_MoveItemToBeginning_RepositionsCorrectly()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item1 = new MenuBarItem { Text = "Item1" };
            var item2 = new MenuBarItem { Text = "Item2" };
            var item3 = new MenuBarItem { Text = "Item3" };
            menuBar.Add(item1);
            menuBar.Add(item2);
            menuBar.Add(item3);

            // Move item3 to the beginning
            var menuBarItems = new List<MenuBarItem> { item3, item1, item2 };

            // Act
            menuBar.SyncMenuBarItemsFromPages(menuBarItems);

            // Assert
            Assert.Equal(3, menuBar.Count);
            Assert.Same(item3, menuBar[0]);
            Assert.Same(item1, menuBar[1]);
            Assert.Same(item2, menuBar[2]);
        }

        /// <summary>
        /// Tests that Clear method works correctly on an empty MenuBar collection.
        /// Verifies that no exceptions are thrown and the collection remains empty.
        /// </summary>
        [Fact]
        public void Clear_EmptyCollection_DoesNotThrow()
        {
            // Arrange
            var menuBar = new MenuBar();

            // Act & Assert
            var exception = Record.Exception(() => menuBar.Clear());

            Assert.Null(exception);
            Assert.Equal(0, menuBar.Count);
        }

        /// <summary>
        /// Tests that Clear method removes a single item from the MenuBar and notifies the handler.
        /// Verifies that the item is removed, handler is notified, and collection becomes empty.
        /// </summary>
        [Fact]
        public void Clear_SingleItem_RemovesItemAndNotifiesHandler()
        {
            // Arrange
            var menuBar = new MenuBar();
            var mockItem = Substitute.For<IMenuBarItem>();
            var handlerInvocations = new List<(string action, MenuBarHandlerUpdate update)>();

            // Set up handler to capture notifications
            menuBar.Handler = Substitute.For<IMenuBarHandler>();
            menuBar.Handler.When(h => h.Invoke(Arg.Any<string>(), Arg.Any<MenuBarHandlerUpdate>()))
                .Do(callInfo => handlerInvocations.Add((callInfo.ArgAt<string>(0), callInfo.ArgAt<MenuBarHandlerUpdate>(1))));

            menuBar.Add(mockItem);
            Assert.Equal(1, menuBar.Count);

            // Act
            menuBar.Clear();

            // Assert
            Assert.Equal(0, menuBar.Count);
            Assert.Single(handlerInvocations);
            Assert.Equal(nameof(IMenuBarHandler.Remove), handlerInvocations[0].action);
            Assert.Equal(0, handlerInvocations[0].update.Index);
            Assert.Equal(mockItem, handlerInvocations[0].update.MenuBarItem);
        }

        /// <summary>
        /// Tests that Clear method removes multiple items from the MenuBar in reverse order.
        /// Verifies that all items are removed, handlers are notified for each removal, and collection becomes empty.
        /// </summary>
        [Fact]
        public void Clear_MultipleItems_RemovesAllItemsInReverseOrder()
        {
            // Arrange
            var menuBar = new MenuBar();
            var mockItem1 = Substitute.For<IMenuBarItem>();
            var mockItem2 = Substitute.For<IMenuBarItem>();
            var mockItem3 = Substitute.For<IMenuBarItem>();
            var handlerInvocations = new List<(string action, MenuBarHandlerUpdate update)>();

            // Set up handler to capture notifications
            menuBar.Handler = Substitute.For<IMenuBarHandler>();
            menuBar.Handler.When(h => h.Invoke(Arg.Any<string>(), Arg.Any<MenuBarHandlerUpdate>()))
                .Do(callInfo => handlerInvocations.Add((callInfo.ArgAt<string>(0), callInfo.ArgAt<MenuBarHandlerUpdate>(1))));

            menuBar.Add(mockItem1);
            menuBar.Add(mockItem2);
            menuBar.Add(mockItem3);
            Assert.Equal(3, menuBar.Count);

            // Act
            menuBar.Clear();

            // Assert
            Assert.Equal(0, menuBar.Count);
            Assert.Equal(3, handlerInvocations.Count);

            // Verify items are removed in reverse order (index 2, 1, 0)
            Assert.Equal(nameof(IMenuBarHandler.Remove), handlerInvocations[0].action);
            Assert.Equal(2, handlerInvocations[0].update.Index);
            Assert.Equal(mockItem3, handlerInvocations[0].update.MenuBarItem);

            Assert.Equal(nameof(IMenuBarHandler.Remove), handlerInvocations[1].action);
            Assert.Equal(1, handlerInvocations[1].update.Index);
            Assert.Equal(mockItem2, handlerInvocations[1].update.MenuBarItem);

            Assert.Equal(nameof(IMenuBarHandler.Remove), handlerInvocations[2].action);
            Assert.Equal(0, handlerInvocations[2].update.Index);
            Assert.Equal(mockItem1, handlerInvocations[2].update.MenuBarItem);
        }

        /// <summary>
        /// Tests that Clear method sets parent to null for MenuBarItem elements that have the MenuBar as parent.
        /// Verifies that parent relationships are properly cleaned up during removal.
        /// </summary>
        [Fact]
        public void Clear_ItemsWithElementParent_SetsParentToNull()
        {
            // Arrange
            var menuBar = new MenuBar();
            var menuBarItem1 = new MenuBarItem { Text = "Item1" };
            var menuBarItem2 = new MenuBarItem { Text = "Item2" };

            menuBar.Add(menuBarItem1);
            menuBar.Add(menuBarItem2);

            // Verify items have the menu bar as parent
            Assert.Equal(menuBar, menuBarItem1.Parent);
            Assert.Equal(menuBar, menuBarItem2.Parent);
            Assert.Equal(2, menuBar.Count);

            // Act
            menuBar.Clear();

            // Assert
            Assert.Equal(0, menuBar.Count);
            Assert.Null(menuBarItem1.Parent);
            Assert.Null(menuBarItem2.Parent);
        }

        /// <summary>
        /// Tests that Clear method results in an empty collection with correct state.
        /// Verifies that Count is 0 and collection enumeration yields no items.
        /// </summary>
        [Fact]
        public void Clear_AfterCompletion_CollectionIsEmpty()
        {
            // Arrange
            var menuBar = new MenuBar();
            var mockItem1 = Substitute.For<IMenuBarItem>();
            var mockItem2 = Substitute.For<IMenuBarItem>();

            menuBar.Add(mockItem1);
            menuBar.Add(mockItem2);
            Assert.Equal(2, menuBar.Count);

            // Act
            menuBar.Clear();

            // Assert
            Assert.Equal(0, menuBar.Count);
            Assert.Empty(menuBar);

            // Verify indexer throws for out of range
            Assert.Throws<ArgumentOutOfRangeException>(() => menuBar[0]);
        }

        /// <summary>
        /// Tests that Clear method works correctly when no handler is set.
        /// Verifies that the method doesn't fail when Handler is null.
        /// </summary>
        [Fact]
        public void Clear_NoHandler_RemovesItemsWithoutException()
        {
            // Arrange
            var menuBar = new MenuBar();
            var mockItem = Substitute.For<IMenuBarItem>();

            menuBar.Add(mockItem);
            Assert.Equal(1, menuBar.Count);
            Assert.Null(menuBar.Handler);

            // Act & Assert
            var exception = Record.Exception(() => menuBar.Clear());

            Assert.Null(exception);
            Assert.Equal(0, menuBar.Count);
        }

        /// <summary>
        /// Tests that CopyTo copies all items from the MenuBar to the target array starting at the specified index.
        /// Input: MenuBar with multiple items, valid array, and valid arrayIndex.
        /// Expected: All items are copied to the array at the correct positions.
        /// </summary>
        [Fact]
        public void CopyTo_WithValidParameters_CopiesAllItemsToArray()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item1 = Substitute.For<IMenuBarItem>();
            var item2 = Substitute.For<IMenuBarItem>();
            var item3 = Substitute.For<IMenuBarItem>();

            menuBar.Add(item1);
            menuBar.Add(item2);
            menuBar.Add(item3);

            var targetArray = new IMenuBarItem[5];
            int arrayIndex = 1;

            // Act
            menuBar.CopyTo(targetArray, arrayIndex);

            // Assert
            Assert.Null(targetArray[0]);
            Assert.Equal(item1, targetArray[1]);
            Assert.Equal(item2, targetArray[2]);
            Assert.Equal(item3, targetArray[3]);
            Assert.Null(targetArray[4]);
        }

        /// <summary>
        /// Tests that CopyTo works correctly with an empty MenuBar.
        /// Input: Empty MenuBar, valid array, and valid arrayIndex.
        /// Expected: Array remains unchanged.
        /// </summary>
        [Fact]
        public void CopyTo_WithEmptyMenuBar_DoesNotModifyArray()
        {
            // Arrange
            var menuBar = new MenuBar();
            var targetArray = new IMenuBarItem[3];
            int arrayIndex = 0;

            // Act
            menuBar.CopyTo(targetArray, arrayIndex);

            // Assert
            Assert.All(targetArray, item => Assert.Null(item));
        }

        /// <summary>
        /// Tests that CopyTo copies a single item correctly.
        /// Input: MenuBar with one item, valid array, and valid arrayIndex.
        /// Expected: Single item is copied to the array at the specified index.
        /// </summary>
        [Fact]
        public void CopyTo_WithSingleItem_CopiesItemToArray()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item = Substitute.For<IMenuBarItem>();
            menuBar.Add(item);

            var targetArray = new IMenuBarItem[2];
            int arrayIndex = 0;

            // Act
            menuBar.CopyTo(targetArray, arrayIndex);

            // Assert
            Assert.Equal(item, targetArray[0]);
            Assert.Null(targetArray[1]);
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentNullException when array parameter is null.
        /// Input: MenuBar with items, null array, valid arrayIndex.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_WithNullArray_ThrowsArgumentNullException()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item = Substitute.For<IMenuBarItem>();
            menuBar.Add(item);

            IMenuBarItem[] targetArray = null;
            int arrayIndex = 0;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => menuBar.CopyTo(targetArray, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentOutOfRangeException when arrayIndex is negative.
        /// Input: MenuBar with items, valid array, negative arrayIndex.
        /// Expected: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_WithNegativeArrayIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item = Substitute.For<IMenuBarItem>();
            menuBar.Add(item);

            var targetArray = new IMenuBarItem[5];
            int arrayIndex = -1;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => menuBar.CopyTo(targetArray, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when arrayIndex equals array length.
        /// Input: MenuBar with items, valid array, arrayIndex equal to array length.
        /// Expected: ArgumentException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_WithArrayIndexEqualToArrayLength_ThrowsArgumentException()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item = Substitute.For<IMenuBarItem>();
            menuBar.Add(item);

            var targetArray = new IMenuBarItem[3];
            int arrayIndex = 3;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => menuBar.CopyTo(targetArray, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException when array is too small to accommodate all items.
        /// Input: MenuBar with multiple items, array too small for all items starting at arrayIndex.
        /// Expected: ArgumentException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_WithArrayTooSmall_ThrowsArgumentException()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item1 = Substitute.For<IMenuBarItem>();
            var item2 = Substitute.For<IMenuBarItem>();
            var item3 = Substitute.For<IMenuBarItem>();

            menuBar.Add(item1);
            menuBar.Add(item2);
            menuBar.Add(item3);

            var targetArray = new IMenuBarItem[4];
            int arrayIndex = 2; // Only 2 positions available, but need 3

            // Act & Assert
            Assert.Throws<ArgumentException>(() => menuBar.CopyTo(targetArray, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo works correctly when arrayIndex is at maximum valid value.
        /// Input: MenuBar with items, valid array, arrayIndex at boundary (array.Length - collection.Count).
        /// Expected: Items are copied to the end of the array.
        /// </summary>
        [Fact]
        public void CopyTo_WithArrayIndexAtBoundary_CopiesItemsCorrectly()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item1 = Substitute.For<IMenuBarItem>();
            var item2 = Substitute.For<IMenuBarItem>();

            menuBar.Add(item1);
            menuBar.Add(item2);

            var targetArray = new IMenuBarItem[5];
            int arrayIndex = 3; // Exactly enough space for 2 items

            // Act
            menuBar.CopyTo(targetArray, arrayIndex);

            // Assert
            Assert.Null(targetArray[0]);
            Assert.Null(targetArray[1]);
            Assert.Null(targetArray[2]);
            Assert.Equal(item1, targetArray[3]);
            Assert.Equal(item2, targetArray[4]);
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentOutOfRangeException with int.MinValue arrayIndex.
        /// Input: MenuBar with items, valid array, int.MinValue arrayIndex.
        /// Expected: ArgumentOutOfRangeException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_WithMinValueArrayIndex_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item = Substitute.For<IMenuBarItem>();
            menuBar.Add(item);

            var targetArray = new IMenuBarItem[5];
            int arrayIndex = int.MinValue;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => menuBar.CopyTo(targetArray, arrayIndex));
        }

        /// <summary>
        /// Tests that CopyTo throws ArgumentException with int.MaxValue arrayIndex.
        /// Input: MenuBar with items, valid array, int.MaxValue arrayIndex.
        /// Expected: ArgumentException is thrown.
        /// </summary>
        [Fact]
        public void CopyTo_WithMaxValueArrayIndex_ThrowsArgumentException()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item = Substitute.For<IMenuBarItem>();
            menuBar.Add(item);

            var targetArray = new IMenuBarItem[5];
            int arrayIndex = int.MaxValue;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => menuBar.CopyTo(targetArray, arrayIndex));
        }

        /// <summary>
        /// Tests that RemoveAt returns early without throwing when given a negative index.
        /// This test exercises the early return condition for negative indices.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        [InlineData(int.MinValue)]
        public void RemoveAt_NegativeIndex_ReturnsEarlyWithoutException(int negativeIndex)
        {
            // Arrange
            var menuBar = new MenuBar();
            var menuItem = Substitute.For<IMenuBarItem>();
            menuBar.Add(menuItem);

            // Act & Assert - Should not throw
            menuBar.RemoveAt(negativeIndex);

            // Verify the item is still in the collection (not removed)
            Assert.Equal(1, menuBar.Count);
            Assert.Contains(menuItem, menuBar);
        }

        /// <summary>
        /// Tests RemoveAt with a valid index where the item is not an Element.
        /// This verifies normal removal behavior without Parent manipulation.
        /// </summary>
        [Fact]
        public void RemoveAt_ValidIndex_NonElementItem_RemovesItem()
        {
            // Arrange
            var menuBar = new MenuBar();
            var menuItem = Substitute.For<IMenuBarItem>();
            menuBar.Add(menuItem);

            // Act
            menuBar.RemoveAt(0);

            // Assert
            Assert.Equal(0, menuBar.Count);
            Assert.DoesNotContain(menuItem, menuBar);
        }

        /// <summary>
        /// Tests RemoveAt with a valid index where the item is an Element but Parent is not this MenuBar.
        /// This verifies that Parent is not modified when the Element's Parent is different.
        /// </summary>
        [Fact]
        public void RemoveAt_ValidIndex_ElementWithDifferentParent_DoesNotModifyParent()
        {
            // Arrange
            var menuBar = new MenuBar();
            var elementItem = new TestMenuBarElement();
            var differentParent = new MenuBar();
            elementItem.Parent = differentParent;
            menuBar.Add(elementItem);

            // Act
            menuBar.RemoveAt(0);

            // Assert
            Assert.Equal(0, menuBar.Count);
            Assert.Equal(differentParent, elementItem.Parent); // Parent should remain unchanged
        }

        /// <summary>
        /// Tests RemoveAt with a valid index where the item is an Element and Parent is this MenuBar.
        /// This verifies that Parent is set to null when removing an Element whose Parent is the current MenuBar.
        /// </summary>
        [Fact]
        public void RemoveAt_ValidIndex_ElementWithSameParent_SetsParentToNull()
        {
            // Arrange
            var menuBar = new MenuBar();
            var elementItem = new TestMenuBarElement();
            menuBar.Add(elementItem);
            elementItem.Parent = menuBar; // Set parent to this MenuBar

            // Act
            menuBar.RemoveAt(0);

            // Assert
            Assert.Equal(0, menuBar.Count);
            Assert.Null(elementItem.Parent); // Parent should be set to null
        }

        /// <summary>
        /// Tests RemoveAt with an out-of-range positive index.
        /// This verifies that appropriate exceptions are thrown for invalid indices.
        /// </summary>
        [Theory]
        [InlineData(0)] // Empty collection
        [InlineData(1)] // Beyond single item
        [InlineData(5)] // Way beyond
        [InlineData(int.MaxValue)]
        public void RemoveAt_OutOfRangeIndex_ThrowsException(int index)
        {
            // Arrange
            var menuBar = new MenuBar();
            // Add one item for some test cases
            if (index > 0)
            {
                menuBar.Add(Substitute.For<IMenuBarItem>());
            }

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => menuBar.RemoveAt(index));
        }

        /// <summary>
        /// Tests RemoveAt with boundary valid indices.
        /// This verifies correct behavior at boundary conditions.
        /// </summary>
        [Fact]
        public void RemoveAt_BoundaryIndices_RemovesCorrectItems()
        {
            // Arrange
            var menuBar = new MenuBar();
            var item1 = Substitute.For<IMenuBarItem>();
            var item2 = Substitute.For<IMenuBarItem>();
            var item3 = Substitute.For<IMenuBarItem>();
            menuBar.Add(item1);
            menuBar.Add(item2);
            menuBar.Add(item3);

            // Act - Remove last item (index 2)
            menuBar.RemoveAt(2);

            // Assert
            Assert.Equal(2, menuBar.Count);
            Assert.DoesNotContain(item3, menuBar);
            Assert.Contains(item1, menuBar);
            Assert.Contains(item2, menuBar);

            // Act - Remove first item (index 0)
            menuBar.RemoveAt(0);

            // Assert
            Assert.Equal(1, menuBar.Count);
            Assert.DoesNotContain(item1, menuBar);
            Assert.Contains(item2, menuBar);
        }

        /// <summary>
        /// Tests RemoveAt with zero index on a single-item collection.
        /// This verifies correct behavior when removing the only item.
        /// </summary>
        [Fact]
        public void RemoveAt_ZeroIndex_SingleItem_RemovesItem()
        {
            // Arrange
            var menuBar = new MenuBar();
            var menuItem = Substitute.For<IMenuBarItem>();
            menuBar.Add(menuItem);

            // Act
            menuBar.RemoveAt(0);

            // Assert
            Assert.Equal(0, menuBar.Count);
            Assert.DoesNotContain(menuItem, menuBar);
        }

        /// <summary>
        /// Tests that the IsEnabled property returns the default value of true when not explicitly set.
        /// This verifies the getter implementation and the default value specified in the BindableProperty creation.
        /// </summary>
        [Fact]
        public void IsEnabled_DefaultValue_ReturnsTrue()
        {
            // Arrange
            var menuBar = new MenuBar();

            // Act
            var result = menuBar.IsEnabled;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the IsEnabled property correctly stores and retrieves the value when set to true.
        /// This verifies both the setter and getter implementations for the true case.
        /// </summary>
        [Fact]
        public void IsEnabled_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var menuBar = new MenuBar();

            // Act
            menuBar.IsEnabled = true;
            var result = menuBar.IsEnabled;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the IsEnabled property correctly stores and retrieves the value when set to false.
        /// This verifies both the setter and getter implementations for the false case.
        /// </summary>
        [Fact]
        public void IsEnabled_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var menuBar = new MenuBar();

            // Act
            menuBar.IsEnabled = false;
            var result = menuBar.IsEnabled;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the IsEnabled property can be toggled between true and false values multiple times.
        /// This verifies the getter and setter work correctly for value changes.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsEnabled_SetValue_ReturnsCorrectValue(bool expectedValue)
        {
            // Arrange
            var menuBar = new MenuBar();

            // Act
            menuBar.IsEnabled = expectedValue;
            var result = menuBar.IsEnabled;

            // Assert
            Assert.Equal(expectedValue, result);
        }
    }
}