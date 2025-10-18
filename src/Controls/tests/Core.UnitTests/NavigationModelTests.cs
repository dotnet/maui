#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class NavigationModelTests : BaseTestFixture
    {
        [Fact]
        public async Task ParentSetsWhenPushingAndUnsetsWhenPopping()
        {
            var mainPage = new ContentPage() { Title = "Main Page" };
            var modal1 = new ContentPage() { Title = "Modal 1" };
            TestWindow testWindow = new TestWindow(mainPage);

            await mainPage.Navigation.PushModalAsync(modal1);

            Assert.Equal(testWindow, modal1.Parent);
            await mainPage.Navigation.PopModalAsync();
            Assert.Null(modal1.Parent);
        }

        [Fact]
        public async Task ModalsWireUpInCorrectOrderWhenPushedBeforeWindowHasBeenCreated()
        {
            var mainPage = new ContentPage() { Title = "Main Page" };
            var modal1 = new ContentPage() { Title = "Modal 1" };
            var modal2 = new ContentPage() { Title = "Modal 2" };

            await mainPage.Navigation.PushModalAsync(modal1);
            await modal1.Navigation.PushModalAsync(modal2);

            TestWindow testWindow = new TestWindow(mainPage);

            Assert.Equal(modal1, testWindow.Navigation.ModalStack[0]);
            Assert.Equal(modal2, testWindow.Navigation.ModalStack[1]);
        }

        [Fact]
        public async Task ModalsWireUpInCorrectOrderWhenShellIsBuiltAfterModalPush()
        {
            var shell = new Shell();
            var mainPage = new ContentPage() { Title = "Main Page 1" };
            var modal1 = new ContentPage() { Title = "Modal 1" };
            var modal2 = new ContentPage() { Title = "Modal 2" };

            await mainPage.Navigation.PushModalAsync(modal1);
            await modal1.Navigation.PushModalAsync(modal2);

            shell.Items.Add(new ShellContent { Content = mainPage });
            TestWindow testWindow = new TestWindow(shell);

            Assert.Equal(modal1, testWindow.Navigation.ModalStack[0]);
            Assert.Equal(modal2, testWindow.Navigation.ModalStack[1]);
            Assert.Equal(2, testWindow.Navigation.ModalStack.Count);
        }

        [Fact]
        public void CurrentNullWhenEmpty()
        {
            var navModel = new NavigationModel();
            Assert.Null(navModel.CurrentPage);
        }

        [Fact]
        public void CurrentGivesLastViewWithoutModal()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();
            var page2 = new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);

            Assert.Equal(page2, navModel.CurrentPage);
        }

        [Fact]
        public void CurrentGivesLastViewWithModal()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();
            var page2 = new ContentPage();

            var modal1 = new ContentPage();
            var modal2 = new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);

            navModel.PushModal(modal1);
            navModel.Push(modal2, modal1);

            Assert.Equal(modal2, navModel.CurrentPage);
        }

        [Fact]
        public void Roots()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();
            var page2 = new ContentPage();

            var modal1 = new ContentPage();
            var modal2 = new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);

            navModel.PushModal(modal1);
            navModel.Push(modal2, modal1);

            Assert.True(navModel.Roots.SequenceEqual(new[] { page1, modal1 }));
        }

        [Fact]
        public void PushFirstItem()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();
            navModel.Push(page1, null);

            Assert.Equal(page1, navModel.CurrentPage);
            Assert.Equal(page1, navModel.Roots.First());
        }

        [Fact]
        public void ThrowsWhenPushingWithoutAncestor()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();
            var page2 = new ContentPage();

            navModel.Push(page1, null);
            Assert.Throws<InvalidNavigationException>(() => navModel.Push(page2, null));
        }

        [Fact]
        public void PushFromNonRootAncestor()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();
            var page2 = new ContentPage();
            var page3 = new ContentPage();

            page2.Parent = page1;
            page3.Parent = page2;

            navModel.Push(page1, null);
            navModel.Push(page2, page1);
            navModel.Push(page3, page2);

            Assert.Equal(page3, navModel.CurrentPage);
        }

        [Fact]
        public void ThrowsWhenPushFromInvalidAncestor()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();
            var page2 = new ContentPage();

            Assert.Throws<InvalidNavigationException>(() => navModel.Push(page2, page1));
        }

        [Fact]
        public void Pop()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();
            var page2 = new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);

            navModel.Pop(page1);

            Assert.Equal(page1, navModel.CurrentPage);
        }

        [Fact]
        public void ThrowsPoppingRootItem()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();

            navModel.Push(page1, null);

            Assert.Throws<InvalidNavigationException>(() => navModel.Pop(page1));
        }

        [Fact]
        public void ThrowsPoppingRootOfModal()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();
            var page2 = new ContentPage();

            var modal1 = new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);

            navModel.PushModal(modal1);
            Assert.Throws<InvalidNavigationException>(() => navModel.Pop(modal1));
        }

        [Fact]
        public void ThrowsPoppingWithInvalidAncestor()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();

            navModel.Push(page1, null);

            Assert.Throws<InvalidNavigationException>(() => navModel.Pop(new ContentPage()));
        }

        [Fact]
        public void PopToRoot()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();
            var page2 = new ContentPage();
            var page3 = new ContentPage();

            page2.Parent = page1;
            page3.Parent = page2;

            navModel.Push(page1, null);
            navModel.Push(page2, page1);
            navModel.Push(page3, page2);

            navModel.PopToRoot(page2);

            Assert.Equal(page1, navModel.CurrentPage);
        }

        [Fact]
        public void ThrowsWhenPopToRootOnRoot()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();

            navModel.Push(page1, null);
            Assert.Throws<InvalidNavigationException>(() => navModel.PopToRoot(page1));
        }

        [Fact]
        public void ThrowsWhenPopToRootWithInvalidAncestor()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();
            var page2 = new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);

            Assert.Throws<InvalidNavigationException>(() => navModel.PopToRoot(new ContentPage()));
        }

        [Fact]
        public void PopModal()
        {
            var navModel = new NavigationModel();

            var child1 = new ContentPage();
            var modal1 = new ContentPage();

            navModel.Push(child1, null);
            navModel.PushModal(modal1);

            navModel.PopModal();

            Assert.Equal(child1, navModel.CurrentPage);
            Assert.Single(navModel.Roots);
        }

        [Fact]
        public void ReturnsCorrectModal()
        {
            var navModel = new NavigationModel();

            var child1 = new ContentPage();
            var modal1 = new ContentPage();
            var modal2 = new ContentPage();

            navModel.Push(child1, null);
            navModel.PushModal(modal1);
            navModel.PushModal(modal2);

            Assert.Equal(modal2, navModel.PopModal());
        }

        [Fact]
        public void PopTopPageWithoutModals()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();
            var page2 = new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);

            Assert.Equal(page2, navModel.PopTopPage());
        }

        [Fact]
        public void PopTopPageWithSinglePage()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();

            navModel.Push(page1, null);

            Assert.Null(navModel.PopTopPage());
        }

        [Fact]
        public void PopTopPageWithModal()
        {
            var navModel = new NavigationModel();

            var page1 = new ContentPage();
            var modal1 = new ContentPage();

            navModel.Push(page1, null);
            navModel.PushModal(modal1);

            Assert.Equal(modal1, navModel.PopTopPage());
        }

        /// <summary>
        /// Tests that the Modals property returns an empty read-only list when the NavigationModel is newly created.
        /// </summary>
        [Fact]
        public void Modals_WhenEmpty_ReturnsEmptyReadOnlyList()
        {
            // Arrange
            var navModel = new NavigationModel();

            // Act
            var modals = navModel.Modals;

            // Assert
            Assert.NotNull(modals);
            Assert.Empty(modals);
            Assert.IsAssignableFrom<IReadOnlyList<Page>>(modals);
        }

        /// <summary>
        /// Tests that the Modals property returns the correct modal page after pushing a single modal.
        /// </summary>
        [Fact]
        public void Modals_AfterPushingSingleModal_ContainsModal()
        {
            // Arrange
            var navModel = new NavigationModel();
            var rootPage = new ContentPage { Title = "Root" };
            var modal = new ContentPage { Title = "Modal" };

            // Act
            navModel.Push(rootPage, null);
            navModel.PushModal(modal);
            var modals = navModel.Modals;

            // Assert
            Assert.Single(modals);
            Assert.Equal(modal, modals[0]);
        }

        /// <summary>
        /// Tests that the Modals property returns all modal pages in the correct order after pushing multiple modals.
        /// </summary>
        [Fact]
        public void Modals_AfterPushingMultipleModals_ContainsAllModalsInCorrectOrder()
        {
            // Arrange
            var navModel = new NavigationModel();
            var rootPage = new ContentPage { Title = "Root" };
            var modal1 = new ContentPage { Title = "Modal 1" };
            var modal2 = new ContentPage { Title = "Modal 2" };
            var modal3 = new ContentPage { Title = "Modal 3" };

            // Act
            navModel.Push(rootPage, null);
            navModel.PushModal(modal1);
            navModel.PushModal(modal2);
            navModel.PushModal(modal3);
            var modals = navModel.Modals;

            // Assert
            Assert.Equal(3, modals.Count);
            Assert.Equal(modal1, modals[0]);
            Assert.Equal(modal2, modals[1]);
            Assert.Equal(modal3, modals[2]);
        }

        /// <summary>
        /// Tests that the Modals property reflects the updated state after popping a modal.
        /// </summary>
        [Fact]
        public void Modals_AfterPoppingModal_ReflectsUpdatedState()
        {
            // Arrange
            var navModel = new NavigationModel();
            var rootPage = new ContentPage { Title = "Root" };
            var modal1 = new ContentPage { Title = "Modal 1" };
            var modal2 = new ContentPage { Title = "Modal 2" };

            navModel.Push(rootPage, null);
            navModel.PushModal(modal1);
            navModel.PushModal(modal2);

            // Act
            navModel.PopModal();
            var modals = navModel.Modals;

            // Assert
            Assert.Single(modals);
            Assert.Equal(modal1, modals[0]);
        }

        /// <summary>
        /// Tests that the Modals property returns an empty list after popping all modals.
        /// </summary>
        [Fact]
        public void Modals_AfterPoppingAllModals_ReturnsEmptyList()
        {
            // Arrange
            var navModel = new NavigationModel();
            var rootPage = new ContentPage { Title = "Root" };
            var modal1 = new ContentPage { Title = "Modal 1" };
            var modal2 = new ContentPage { Title = "Modal 2" };

            navModel.Push(rootPage, null);
            navModel.PushModal(modal1);
            navModel.PushModal(modal2);

            // Act
            navModel.PopModal();
            navModel.PopModal();
            var modals = navModel.Modals;

            // Assert
            Assert.Empty(modals);
        }

        /// <summary>
        /// Tests that the Modals property returns a read-only collection that cannot be modified.
        /// </summary>
        [Fact]
        public void Modals_ReturnedCollection_IsReadOnly()
        {
            // Arrange
            var navModel = new NavigationModel();
            var rootPage = new ContentPage { Title = "Root" };
            var modal = new ContentPage { Title = "Modal" };

            navModel.Push(rootPage, null);
            navModel.PushModal(modal);

            // Act
            var modals = navModel.Modals;

            // Assert
            Assert.IsAssignableFrom<IReadOnlyList<Page>>(modals);

            // Verify it's truly read-only by attempting to cast to mutable collection
            Assert.False(modals is IList<Page>);
            Assert.False(modals is ICollection<Page>);
        }

        /// <summary>
        /// Tests that subsequent calls to the Modals property reflect live updates to the modal stack.
        /// </summary>
        [Fact]
        public void Modals_SubsequentCalls_ReflectLiveUpdates()
        {
            // Arrange
            var navModel = new NavigationModel();
            var rootPage = new ContentPage { Title = "Root" };
            var modal = new ContentPage { Title = "Modal" };

            navModel.Push(rootPage, null);

            // Act & Assert - Initial state
            var initialModals = navModel.Modals;
            Assert.Empty(initialModals);

            // Act & Assert - After push
            navModel.PushModal(modal);
            var modalsAfterPush = navModel.Modals;
            Assert.Single(modalsAfterPush);
            Assert.Equal(modal, modalsAfterPush[0]);

            // Act & Assert - After pop
            navModel.PopModal();
            var modalsAfterPop = navModel.Modals;
            Assert.Empty(modalsAfterPop);
        }

        /// <summary>
        /// Tests that Roots returns an empty enumerable when the NavigationModel is empty.
        /// Validates the behavior when no navigation stacks have been created.
        /// Expected result: Empty enumerable with no elements.
        /// </summary>
        [Fact]
        public void Roots_WhenEmpty_ReturnsEmptyEnumerable()
        {
            // Arrange
            var navModel = new NavigationModel();

            // Act
            var roots = navModel.Roots;

            // Assert
            Assert.Empty(roots);
        }

        /// <summary>
        /// Tests that Roots returns an empty enumerable after clearing the NavigationModel.
        /// Validates the behavior when navigation stacks are cleared.
        /// Expected result: Empty enumerable with no elements.
        /// </summary>
        [Fact]
        public void Roots_AfterClear_ReturnsEmptyEnumerable()
        {
            // Arrange
            var navModel = new NavigationModel();
            var page = new ContentPage();
            navModel.Push(page, null);

            // Act
            navModel.Clear();
            var roots = navModel.Roots;

            // Assert
            Assert.Empty(roots);
        }

        /// <summary>
        /// Tests that Roots returns the single page when only one page is pushed as root.
        /// Validates the behavior with a single navigation stack containing one page.
        /// Expected result: Enumerable containing the single root page.
        /// </summary>
        [Fact]
        public void Roots_WithSingleRootPage_ReturnsThatPage()
        {
            // Arrange
            var navModel = new NavigationModel();
            var rootPage = new ContentPage();

            // Act
            navModel.Push(rootPage, null);
            var roots = navModel.Roots;

            // Assert
            Assert.Single(roots);
            Assert.Equal(rootPage, roots.First());
        }

        /// <summary>
        /// Tests that Roots returns only the root page when multiple pages are pushed in a single stack.
        /// Validates the behavior with a single navigation stack containing multiple pages.
        /// Expected result: Enumerable containing only the first (root) page, not the subsequent pages.
        /// </summary>
        [Fact]
        public void Roots_WithMultiplePagesInSingleStack_ReturnsOnlyRootPage()
        {
            // Arrange
            var navModel = new NavigationModel();
            var rootPage = new ContentPage();
            var secondPage = new ContentPage();
            var thirdPage = new ContentPage();

            // Act
            navModel.Push(rootPage, null);
            navModel.Push(secondPage, rootPage);
            navModel.Push(thirdPage, secondPage);
            var roots = navModel.Roots;

            // Assert
            Assert.Single(roots);
            Assert.Equal(rootPage, roots.First());
        }

        /// <summary>
        /// Tests that Roots returns the root page from each navigation stack when multiple stacks exist.
        /// Validates the behavior with multiple independent navigation stacks created via modal pushes.
        /// Expected result: Enumerable containing the root page from each navigation stack.
        /// </summary>
        [Fact]
        public void Roots_WithMultipleNavigationStacks_ReturnsAllRoots()
        {
            // Arrange
            var navModel = new NavigationModel();
            var firstRoot = new ContentPage();
            var secondRoot = new ContentPage();
            var thirdRoot = new ContentPage();

            // Act
            navModel.Push(firstRoot, null);
            navModel.PushModal(secondRoot);
            navModel.PushModal(thirdRoot);
            var roots = navModel.Roots;

            // Assert
            Assert.Equal(3, roots.Count());
            Assert.True(roots.SequenceEqual(new[] { firstRoot, secondRoot, thirdRoot }));
        }

        /// <summary>
        /// Tests that Roots returns roots from all stacks when mixing regular navigation and modal navigation.
        /// Validates the behavior with complex navigation including both regular pushes and modal pushes.
        /// Expected result: Enumerable containing the root page from each navigation stack in creation order.
        /// </summary>
        [Fact]
        public void Roots_WithMixedNavigationAndModals_ReturnsAllRootsInOrder()
        {
            // Arrange
            var navModel = new NavigationModel();
            var mainRoot = new ContentPage();
            var mainSecond = new ContentPage();
            var modalRoot1 = new ContentPage();
            var modalSecond1 = new ContentPage();
            var modalRoot2 = new ContentPage();

            // Act
            navModel.Push(mainRoot, null);
            navModel.Push(mainSecond, mainRoot);
            navModel.PushModal(modalRoot1);
            navModel.Push(modalSecond1, modalRoot1);
            navModel.PushModal(modalRoot2);
            var roots = navModel.Roots;

            // Assert
            Assert.Equal(3, roots.Count());
            Assert.True(roots.SequenceEqual(new[] { mainRoot, modalRoot1, modalRoot2 }));
        }

        /// <summary>
        /// Tests that Roots enumerable can be iterated multiple times and produces consistent results.
        /// Validates the behavior of the yield-based enumerable implementation.
        /// Expected result: Multiple iterations produce the same sequence of root pages.
        /// </summary>
        [Fact]
        public void Roots_CanBeIteratedMultipleTimes_ProducesConsistentResults()
        {
            // Arrange
            var navModel = new NavigationModel();
            var root1 = new ContentPage();
            var root2 = new ContentPage();
            navModel.Push(root1, null);
            navModel.PushModal(root2);

            // Act
            var roots = navModel.Roots;
            var firstIteration = roots.ToList();
            var secondIteration = roots.ToList();

            // Assert
            Assert.Equal(2, firstIteration.Count);
            Assert.Equal(2, secondIteration.Count);
            Assert.True(firstIteration.SequenceEqual(secondIteration));
            Assert.True(firstIteration.SequenceEqual(new[] { root1, root2 }));
        }

        /// <summary>
        /// Tests that changes to the navigation model are reflected in subsequent calls to Roots.
        /// Validates the behavior when the underlying navigation structure changes after getting Roots.
        /// Expected result: New calls to Roots reflect the current state of navigation stacks.
        /// </summary>
        [Fact]
        public void Roots_ReflectsChangesToNavigationModel_WhenAccessedAgain()
        {
            // Arrange
            var navModel = new NavigationModel();
            var root1 = new ContentPage();

            // Act & Assert - Initial state
            navModel.Push(root1, null);
            var initialRoots = navModel.Roots.ToList();
            Assert.Single(initialRoots);
            Assert.Equal(root1, initialRoots.First());

            // Act & Assert - After adding modal
            var root2 = new ContentPage();
            navModel.PushModal(root2);
            var updatedRoots = navModel.Roots.ToList();
            Assert.Equal(2, updatedRoots.Count);
            Assert.True(updatedRoots.SequenceEqual(new[] { root1, root2 }));

            // Act & Assert - After clearing
            navModel.Clear();
            var clearedRoots = navModel.Roots.ToList();
            Assert.Empty(clearedRoots);
        }

        /// <summary>
        /// Tests that Tree property returns an empty read-only list when NavigationModel is newly instantiated.
        /// Verifies the initial state of the navigation tree structure.
        /// Expected result: Tree should return a non-null, empty IReadOnlyList.
        /// </summary>
        [Fact]
        public void Tree_WhenNewlyInstantiated_ReturnsEmptyReadOnlyList()
        {
            // Arrange
            var navModel = new NavigationModel();

            // Act
            var tree = navModel.Tree;

            // Assert
            Assert.NotNull(tree);
            Assert.Empty(tree);
            Assert.IsAssignableFrom<IReadOnlyList<IReadOnlyList<Page>>>(tree);
        }

        /// <summary>
        /// Tests that Tree property consistently returns the same reference on multiple calls.
        /// Verifies that the property getter is stable and doesn't create new instances.
        /// Expected result: Multiple calls to Tree should return the same reference.
        /// </summary>
        [Fact]
        public void Tree_WhenCalledMultipleTimes_ReturnsSameReference()
        {
            // Arrange
            var navModel = new NavigationModel();

            // Act
            var tree1 = navModel.Tree;
            var tree2 = navModel.Tree;

            // Assert
            Assert.Same(tree1, tree2);
        }

        /// <summary>
        /// Tests that Tree property returns a read-only collection that cannot be modified directly.
        /// Verifies the immutability contract of the Tree property.
        /// Expected result: Tree should be read-only and not allow direct modifications.
        /// </summary>
        [Fact]
        public void Tree_ReturnsReadOnlyCollection_CannotBeModifiedDirectly()
        {
            // Arrange
            var navModel = new NavigationModel();

            // Act
            var tree = navModel.Tree;

            // Assert
            Assert.IsAssignableFrom<IReadOnlyList<IReadOnlyList<Page>>>(tree);
            Assert.False(tree is IList);
            Assert.False(tree is ICollection);
        }

        /// <summary>
        /// Tests that Clear method properly clears both navigation tree and modal stack when both are empty.
        /// Verifies that calling Clear on an already empty NavigationModel doesn't cause any issues.
        /// </summary>
        [Fact]
        public void Clear_WhenBothCollectionsEmpty_ClearsSuccessfully()
        {
            // Arrange
            var navModel = new NavigationModel();

            // Act
            navModel.Clear();

            // Assert
            Assert.Equal(0, navModel.Modals.Count);
            Assert.Equal(0, navModel.Tree.Count);
        }

        /// <summary>
        /// Tests that Clear method properly clears both navigation tree and modal stack when both contain pages.
        /// Validates that all pages are removed from both collections after calling Clear.
        /// </summary>
        [Fact]
        public void Clear_WhenBothCollectionsHaveItems_ClearsAllItems()
        {
            // Arrange
            var navModel = new NavigationModel();
            var page1 = new ContentPage();
            var page2 = new ContentPage();
            var modalPage = new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);
            navModel.PushModal(modalPage);

            // Verify setup - both collections should have items
            Assert.True(navModel.Modals.Count > 0);
            Assert.True(navModel.Tree.Count > 0);

            // Act
            navModel.Clear();

            // Assert
            Assert.Equal(0, navModel.Modals.Count);
            Assert.Equal(0, navModel.Tree.Count);
        }

        /// <summary>
        /// Tests that Clear method properly clears navigation tree when it contains pages but modal stack is empty.
        /// Ensures Clear works correctly when only one collection has items.
        /// </summary>
        [Fact]
        public void Clear_WhenOnlyNavigationTreeHasItems_ClearsNavigationTree()
        {
            // Arrange
            var navModel = new NavigationModel();
            var page1 = new ContentPage();
            var page2 = new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);

            // Verify setup - navigation tree should have items, modal stack should be empty
            Assert.Equal(0, navModel.Modals.Count);
            Assert.True(navModel.Tree.Count > 0);

            // Act
            navModel.Clear();

            // Assert
            Assert.Equal(0, navModel.Modals.Count);
            Assert.Equal(0, navModel.Tree.Count);
        }

        /// <summary>
        /// Tests that Clear method properly clears modal stack when it contains pages but navigation tree is empty.
        /// This scenario tests the edge case where only modals exist without any navigation tree items.
        /// </summary>
        [Fact]
        public void Clear_WhenOnlyModalStackHasItems_ClearsModalStack()
        {
            // Arrange
            var navModel = new NavigationModel();
            var modalPage = new ContentPage();

            navModel.PushModal(modalPage);

            // Verify setup - modal stack should have items, and tree will have been created by PushModal
            Assert.True(navModel.Modals.Count > 0);
            Assert.True(navModel.Tree.Count > 0);

            // Act
            navModel.Clear();

            // Assert
            Assert.Equal(0, navModel.Modals.Count);
            Assert.Equal(0, navModel.Tree.Count);
        }

        /// <summary>
        /// Tests that Clear method can be called multiple times without causing exceptions.
        /// Verifies that calling Clear on an already cleared NavigationModel is safe.
        /// </summary>
        [Fact]
        public void Clear_CalledMultipleTimes_DoesNotThrowException()
        {
            // Arrange
            var navModel = new NavigationModel();
            var page1 = new ContentPage();
            var modalPage = new ContentPage();

            navModel.Push(page1, null);
            navModel.PushModal(modalPage);

            // Act & Assert - should not throw
            navModel.Clear();
            navModel.Clear();
            navModel.Clear();

            // Verify final state
            Assert.Equal(0, navModel.Modals.Count);
            Assert.Equal(0, navModel.Tree.Count);
        }

        /// <summary>
        /// Tests that InsertPageBefore successfully inserts a page before an existing page in the current navigation stack.
        /// Verifies that the page is inserted at the correct position and the navigation structure remains valid.
        /// </summary>
        [Fact]
        public void InsertPageBefore_ValidBeforePage_InsertsPageAtCorrectIndex()
        {
            // Arrange
            var navModel = new NavigationModel();
            var page1 = new ContentPage();
            var page2 = new ContentPage();
            var pageToInsert = new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);

            // Act
            navModel.InsertPageBefore(pageToInsert, page2);

            // Assert
            var roots = navModel.Roots.ToList();
            var currentStack = roots.First().Cast<Page>().ToList();

            Assert.Equal(3, currentStack.Count);
            Assert.Equal(page1, currentStack[0]);
            Assert.Equal(pageToInsert, currentStack[1]);
            Assert.Equal(page2, currentStack[2]);
            Assert.Equal(page2, navModel.CurrentPage);
        }

        /// <summary>
        /// Tests that InsertPageBefore successfully inserts a page before the first page in the navigation stack.
        /// Verifies that the insertion works correctly when targeting the root page.
        /// </summary>
        [Fact]
        public void InsertPageBefore_BeforeFirstPage_InsertsPageAtBeginning()
        {
            // Arrange
            var navModel = new NavigationModel();
            var page1 = new ContentPage();
            var page2 = new ContentPage();
            var pageToInsert = new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);

            // Act
            navModel.InsertPageBefore(pageToInsert, page1);

            // Assert
            var roots = navModel.Roots.ToList();
            var currentStack = roots.First().Cast<Page>().ToList();

            Assert.Equal(3, currentStack.Count);
            Assert.Equal(pageToInsert, currentStack[0]);
            Assert.Equal(page1, currentStack[1]);
            Assert.Equal(page2, currentStack[2]);
            Assert.Equal(page2, navModel.CurrentPage);
        }

        /// <summary>
        /// Tests that InsertPageBefore throws ArgumentException when the before page is not found in the current navigation context.
        /// Verifies that the method properly validates that the before page exists in the current stack.
        /// </summary>
        [Fact]
        public void InsertPageBefore_BeforePageNotInContext_ThrowsArgumentException()
        {
            // Arrange
            var navModel = new NavigationModel();
            var page1 = new ContentPage();
            var page2 = new ContentPage();
            var pageToInsert = new ContentPage();
            var pageNotInContext = new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => navModel.InsertPageBefore(pageToInsert, pageNotInContext));
            Assert.Equal("before must be in the current navigation context", exception.Message);
        }

        /// <summary>
        /// Tests that InsertPageBefore throws ArgumentException when the before page is null.
        /// Verifies proper handling of null reference for the before parameter.
        /// </summary>
        [Fact]
        public void InsertPageBefore_NullBeforePage_ThrowsArgumentException()
        {
            // Arrange
            var navModel = new NavigationModel();
            var page1 = new ContentPage();
            var pageToInsert = new ContentPage();

            navModel.Push(page1, null);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => navModel.InsertPageBefore(pageToInsert, null));
            Assert.Equal("before must be in the current navigation context", exception.Message);
        }

        /// <summary>
        /// Tests that InsertPageBefore throws InvalidOperationException when called on an empty navigation model.
        /// Verifies that the method properly handles the case where no navigation stack exists.
        /// </summary>
        [Fact]
        public void InsertPageBefore_EmptyNavigationModel_ThrowsInvalidOperationException()
        {
            // Arrange
            var navModel = new NavigationModel();
            var pageToInsert = new ContentPage();
            var beforePage = new ContentPage();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => navModel.InsertPageBefore(pageToInsert, beforePage));
        }

        /// <summary>
        /// Tests that InsertPageBefore works correctly when there are multiple navigation stacks with modals.
        /// Verifies that the insertion operates on the current (last) navigation stack.
        /// </summary>
        [Fact]
        public void InsertPageBefore_WithModalStack_InsertsInCurrentStack()
        {
            // Arrange
            var navModel = new NavigationModel();
            var page1 = new ContentPage();
            var page2 = new ContentPage();
            var modalPage1 = new ContentPage();
            var modalPage2 = new ContentPage();
            var pageToInsert = new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);
            navModel.PushModal(modalPage1);
            navModel.Push(modalPage2, modalPage1);

            // Act
            navModel.InsertPageBefore(pageToInsert, modalPage2);

            // Assert
            Assert.Equal(modalPage2, navModel.CurrentPage);
            var modals = navModel.Modals.ToList();
            Assert.Equal(2, modals.Count);
            Assert.Equal(modalPage1, modals[0]);
            Assert.Equal(pageToInsert, modals[1]);
        }

        /// <summary>
        /// Tests that InsertPageBefore fails when trying to insert before a page that exists in a different navigation stack.
        /// Verifies that the method only considers pages in the current navigation context.
        /// </summary>
        [Fact]
        public void InsertPageBefore_BeforePageInDifferentStack_ThrowsArgumentException()
        {
            // Arrange
            var navModel = new NavigationModel();
            var page1 = new ContentPage();
            var page2 = new ContentPage();
            var modalPage1 = new ContentPage();
            var pageToInsert = new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);
            navModel.PushModal(modalPage1);

            // Act & Assert - try to insert before page2 while in modal context
            var exception = Assert.Throws<ArgumentException>(() => navModel.InsertPageBefore(pageToInsert, page2));
            Assert.Equal("before must be in the current navigation context", exception.Message);
        }

        /// <summary>
        /// Tests that PopModal throws InvalidNavigationException when called on an empty NavigationModel.
        /// This exercises the condition _navTree.Count <= 1 when no navigation trees exist.
        /// </summary>
        [Fact]
        public void PopModal_EmptyNavigationModel_ThrowsInvalidNavigationException()
        {
            // Arrange
            var navModel = new NavigationModel();

            // Act & Assert
            var exception = Assert.Throws<InvalidNavigationException>(() => navModel.PopModal());
            Assert.Equal("Can't pop modal without any modals pushed", exception.Message);
        }

        /// <summary>
        /// Tests that PopModal throws InvalidNavigationException when called with only base navigation (no modals pushed).
        /// This exercises the condition _navTree.Count <= 1 when only the base navigation tree exists.
        /// </summary>
        [Fact]
        public void PopModal_OnlyBaseNavigation_ThrowsInvalidNavigationException()
        {
            // Arrange
            var navModel = new NavigationModel();
            var basePage = new ContentPage();
            navModel.Push(basePage, null);

            // Act & Assert
            var exception = Assert.Throws<InvalidNavigationException>(() => navModel.PopModal());
            Assert.Equal("Can't pop modal without any modals pushed", exception.Message);
        }

        /// <summary>
        /// Tests that PopModal works correctly when the root page is a Shell.
        /// When the root is a Shell, navigation lifecycle events should not be sent.
        /// </summary>
        [Fact]
        public void PopModal_WithShellAsRoot_DoesNotSendNavigationEvents()
        {
            // Arrange
            var navModel = new NavigationModel();
            var shell = new TestShell();
            var modal = new TestPage();

            navModel.Push(shell, null);
            navModel.PushModal(modal);

            // Act
            var poppedModal = navModel.PopModal();

            // Assert
            Assert.Equal(modal, poppedModal);
            Assert.Equal(shell, navModel.CurrentPage);
            Assert.False(modal.NavigatingFromCalled);
            Assert.False(modal.DisappearingCalled);
            Assert.False(shell.AppearingCalled);
        }

        /// <summary>
        /// Tests that PopModal works correctly when the root page is not a Shell.
        /// When the root is not a Shell, navigation lifecycle events should be sent.
        /// </summary>
        [Fact]
        public void PopModal_WithContentPageAsRoot_SendsNavigationEvents()
        {
            // Arrange
            var navModel = new NavigationModel();
            var basePage = new TestPage();
            var modal = new TestPage();

            navModel.Push(basePage, null);
            navModel.PushModal(modal);

            // Act
            var poppedModal = navModel.PopModal();

            // Assert
            Assert.Equal(modal, poppedModal);
            Assert.Equal(basePage, navModel.CurrentPage);
            Assert.True(modal.NavigatingFromCalled);
            Assert.True(modal.DisappearingCalled);
            Assert.True(basePage.AppearingCalled);
        }

        private class TestPage : ContentPage
        {
            public bool NavigatingFromCalled { get; private set; }
            public bool DisappearingCalled { get; private set; }
            public bool AppearingCalled { get; private set; }

            protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
            {
                NavigatingFromCalled = true;
                base.OnNavigatingFrom(args);
            }

            protected override void OnDisappearing()
            {
                DisappearingCalled = true;
                base.OnDisappearing();
            }

            protected override void OnAppearing()
            {
                AppearingCalled = true;
                base.OnAppearing();
            }
        }

        private class TestShell : Shell
        {
            public bool AppearingCalled { get; private set; }

            protected override void OnAppearing()
            {
                AppearingCalled = true;
                base.OnAppearing();
            }
        }

        /// <summary>
        /// Tests that RemovePage returns false when called with null page parameter.
        /// </summary>
        [Fact]
        public void RemovePage_NullPage_ReturnsFalse()
        {
            // Arrange
            var navModel = new NavigationModel();
            var page1 = new ContentPage();
            navModel.Push(page1, null);

            // Act
            var result = navModel.RemovePage(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that RemovePage returns false when navigation model is empty.
        /// </summary>
        [Fact]
        public void RemovePage_EmptyNavigationModel_ReturnsFalse()
        {
            // Arrange
            var navModel = new NavigationModel();
            var page = new ContentPage();

            // Act
            var result = navModel.RemovePage(page);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that RemovePage successfully removes and returns true when page exists in the last stack.
        /// </summary>
        [Fact]
        public void RemovePage_PageExistsInLastStack_ReturnsTrueAndRemovesPage()
        {
            // Arrange
            var navModel = new NavigationModel();
            var page1 = new ContentPage();
            var page2 = new ContentPage();
            var page3 = new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);
            navModel.Push(page3, page2);

            // Act
            var result = navModel.RemovePage(page2);

            // Assert
            Assert.True(result);
            Assert.Equal(page3, navModel.CurrentPage);
        }

        /// <summary>
        /// Tests that RemovePage successfully removes and returns true when page exists in an earlier stack.
        /// </summary>
        [Fact]
        public void RemovePage_PageExistsInEarlierStack_ReturnsTrueAndRemovesPage()
        {
            // Arrange
            var navModel = new NavigationModel();
            var rootPage = new ContentPage();
            var modalPage = new ContentPage();
            var pageToRemove = new ContentPage();

            // Create first stack
            navModel.Push(rootPage, null);
            navModel.Push(pageToRemove, rootPage);

            // Create second stack via modal
            navModel.PushModal(modalPage);

            // Act
            var result = navModel.RemovePage(pageToRemove);

            // Assert
            Assert.True(result);
            Assert.Equal(modalPage, navModel.CurrentPage);
        }

        /// <summary>
        /// Tests that RemovePage returns false when page doesn't exist in any stack.
        /// </summary>
        [Fact]
        public void RemovePage_PageDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var navModel = new NavigationModel();
            var page1 = new ContentPage();
            var page2 = new ContentPage();
            var nonExistentPage = new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);

            // Act
            var result = navModel.RemovePage(nonExistentPage);

            // Assert
            Assert.False(result);
            Assert.Equal(page2, navModel.CurrentPage);
        }

        /// <summary>
        /// Tests that RemovePage works correctly when there's only a single stack with single page.
        /// </summary>
        [Fact]
        public void RemovePage_SingleStackSinglePage_ReturnsTrueAndRemovesPage()
        {
            // Arrange
            var navModel = new NavigationModel();
            var page = new ContentPage();
            navModel.Push(page, null);

            // Act
            var result = navModel.RemovePage(page);

            // Assert
            Assert.True(result);
            Assert.Null(navModel.CurrentPage);
        }

        /// <summary>
        /// Tests that RemovePage finds and removes page from the first stack when multiple stacks exist.
        /// </summary>
        [Fact]
        public void RemovePage_PageInFirstStackWithMultipleStacks_ReturnsTrueAndRemovesPage()
        {
            // Arrange
            var navModel = new NavigationModel();
            var rootPage = new ContentPage();
            var secondPage = new ContentPage();
            var modal1 = new ContentPage();
            var modal2 = new ContentPage();

            // Create first stack
            navModel.Push(rootPage, null);
            navModel.Push(secondPage, rootPage);

            // Create additional stacks
            navModel.PushModal(modal1);
            navModel.PushModal(modal2);

            // Act
            var result = navModel.RemovePage(secondPage);

            // Assert
            Assert.True(result);
            Assert.Equal(modal2, navModel.CurrentPage);
        }

        /// <summary>
        /// Tests that RemovePage finds and removes page from a middle stack when multiple stacks exist.
        /// </summary>
        [Fact]
        public void RemovePage_PageInMiddleStack_ReturnsTrueAndRemovesPage()
        {
            // Arrange
            var navModel = new NavigationModel();
            var rootPage = new ContentPage();
            var modal1 = new ContentPage();
            var modal2 = new ContentPage();
            var modal3 = new ContentPage();

            // Create first stack
            navModel.Push(rootPage, null);

            // Create middle stack with the page to remove
            navModel.PushModal(modal1);
            navModel.Push(modal2, modal1);

            // Create final stack
            navModel.PushModal(modal3);

            // Act
            var result = navModel.RemovePage(modal2);

            // Assert
            Assert.True(result);
            Assert.Equal(modal3, navModel.CurrentPage);
        }

        /// <summary>
        /// Tests RemovePage behavior with various page existence scenarios using parameterized test.
        /// </summary>
        /// <param name="pageExists">Whether the page to remove exists in the navigation model</param>
        /// <param name="expectedResult">Expected return value from RemovePage</param>
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void RemovePage_ParameterizedPageExistence_ReturnsExpectedResult(bool pageExists, bool expectedResult)
        {
            // Arrange
            var navModel = new NavigationModel();
            var page1 = new ContentPage();
            var page2 = new ContentPage();
            var pageToRemove = pageExists ? page1 : new ContentPage();

            navModel.Push(page1, null);
            navModel.Push(page2, page1);

            // Act
            var result = navModel.RemovePage(pageToRemove);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that LastRoot returns null when the navigation tree is empty.
        /// Verifies the behavior when no navigation stacks have been created.
        /// Expected result: null is returned.
        /// </summary>
        [Fact]
        public void LastRoot_WhenNavigationTreeIsEmpty_ReturnsNull()
        {
            // Arrange
            var navModel = new NavigationModel();

            // Act
            var result = navModel.LastRoot;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that LastRoot returns the first page of the last navigation stack when there is a single navigation stack.
        /// Verifies the behavior with one navigation stack containing one page.
        /// Expected result: the root page of the single navigation stack is returned.
        /// </summary>
        [Fact]
        public void LastRoot_WhenSingleNavigationStackWithOnePage_ReturnsRootPage()
        {
            // Arrange
            var navModel = new NavigationModel();
            var rootPage = new ContentPage();
            navModel.Push(rootPage, null);

            // Act
            var result = navModel.LastRoot;

            // Assert
            Assert.Equal(rootPage, result);
        }

        /// <summary>
        /// Tests that LastRoot returns the first page of the last navigation stack when there is a single navigation stack with multiple pages.
        /// Verifies the behavior with one navigation stack containing multiple pages.
        /// Expected result: the root page (first page) of the navigation stack is returned.
        /// </summary>
        [Fact]
        public void LastRoot_WhenSingleNavigationStackWithMultiplePages_ReturnsRootPage()
        {
            // Arrange
            var navModel = new NavigationModel();
            var rootPage = new ContentPage();
            var secondPage = new ContentPage();
            var thirdPage = new ContentPage();

            secondPage.Parent = rootPage;
            thirdPage.Parent = secondPage;

            navModel.Push(rootPage, null);
            navModel.Push(secondPage, rootPage);
            navModel.Push(thirdPage, secondPage);

            // Act
            var result = navModel.LastRoot;

            // Assert
            Assert.Equal(rootPage, result);
        }

        /// <summary>
        /// Tests that LastRoot returns the first page of the most recently created navigation stack when there are multiple navigation stacks.
        /// Verifies the behavior when modals create additional navigation stacks.
        /// Expected result: the root page of the last (most recent) navigation stack is returned.
        /// </summary>
        [Fact]
        public void LastRoot_WhenMultipleNavigationStacks_ReturnsRootPageOfLastStack()
        {
            // Arrange
            var navModel = new NavigationModel();
            var firstRootPage = new ContentPage();
            var secondPageInFirstStack = new ContentPage();
            var modalRootPage = new ContentPage();
            var secondPageInModalStack = new ContentPage();

            secondPageInFirstStack.Parent = firstRootPage;
            secondPageInModalStack.Parent = modalRootPage;

            // Create first navigation stack
            navModel.Push(firstRootPage, null);
            navModel.Push(secondPageInFirstStack, firstRootPage);

            // Create second navigation stack via modal
            navModel.PushModal(modalRootPage);
            navModel.Push(secondPageInModalStack, modalRootPage);

            // Act
            var result = navModel.LastRoot;

            // Assert
            Assert.Equal(modalRootPage, result);
        }

        /// <summary>
        /// Tests that LastRoot returns the correct root page after popping a modal navigation stack.
        /// Verifies the behavior when the last navigation stack is removed.
        /// Expected result: the root page of the previous navigation stack is returned.
        /// </summary>
        [Fact]
        public void LastRoot_AfterPoppingModalStack_ReturnsRootPageOfPreviousStack()
        {
            // Arrange
            var navModel = new NavigationModel();
            var firstRootPage = new ContentPage();
            var modalRootPage = new ContentPage();

            navModel.Push(firstRootPage, null);
            navModel.PushModal(modalRootPage);

            // Verify modal is the last root
            Assert.Equal(modalRootPage, navModel.LastRoot);

            // Pop the modal
            navModel.PopModal();

            // Act
            var result = navModel.LastRoot;

            // Assert
            Assert.Equal(firstRootPage, result);
        }

        /// <summary>
        /// Tests that LastRoot returns null after clearing all navigation stacks.
        /// Verifies the behavior when the navigation model is cleared.
        /// Expected result: null is returned.
        /// </summary>
        [Fact]
        public void LastRoot_AfterClearingNavigationModel_ReturnsNull()
        {
            // Arrange
            var navModel = new NavigationModel();
            var rootPage = new ContentPage();
            navModel.Push(rootPage, null);

            // Verify page was added
            Assert.Equal(rootPage, navModel.LastRoot);

            // Clear the navigation model
            navModel.Clear();

            // Act
            var result = navModel.LastRoot;

            // Assert
            Assert.Null(result);
        }
    }
}