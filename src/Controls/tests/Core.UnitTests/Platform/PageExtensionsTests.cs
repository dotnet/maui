#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class PageExtensionsTests
    {
        /// <summary>
        /// Tests that GetCurrentPage throws ArgumentNullException when currentPage is null.
        /// </summary>
        [Fact]
        public void GetCurrentPage_NullCurrentPage_ThrowsArgumentNullException()
        {
            // Arrange
            Page currentPage = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => PageExtensions.GetCurrentPage(currentPage));
        }

        /// <summary>
        /// Tests that GetCurrentPage returns the modal page when modal stack contains pages.
        /// </summary>
        [Fact]
        public void GetCurrentPage_ModalStackHasPages_ReturnsLastModalPage()
        {
            // Arrange
            var currentPage = Substitute.For<Page>();
            var navigationProxy = Substitute.For<NavigationProxy>();
            var modalPage1 = Substitute.For<Page>();
            var modalPage2 = Substitute.For<Page>();
            var modalStack = new List<Page> { modalPage1, modalPage2 };

            currentPage.NavigationProxy.Returns(navigationProxy);
            navigationProxy.ModalStack.Returns(modalStack);

            // Act
            var result = PageExtensions.GetCurrentPage(currentPage);

            // Assert
            Assert.Equal(modalPage2, result);
        }

        /// <summary>
        /// Tests that GetCurrentPage continues to next condition when modal stack is empty.
        /// </summary>
        [Fact]
        public void GetCurrentPage_EmptyModalStack_ContinuesToNextCondition()
        {
            // Arrange
            var currentPage = Substitute.For<Page>();
            var navigationProxy = Substitute.For<NavigationProxy>();
            var emptyModalStack = new List<Page>();

            currentPage.NavigationProxy.Returns(navigationProxy);
            navigationProxy.ModalStack.Returns(emptyModalStack);

            // Act
            var result = PageExtensions.GetCurrentPage(currentPage);

            // Assert
            Assert.Equal(currentPage, result);
        }

        /// <summary>
        /// Tests that GetCurrentPage recursively calls itself for FlyoutPage Detail.
        /// </summary>
        [Fact]
        public void GetCurrentPage_FlyoutPageWithDetail_ReturnsDetailPageResult()
        {
            // Arrange
            var flyoutPage = Substitute.For<FlyoutPage>();
            var detailPage = Substitute.For<Page>();
            var navigationProxy = Substitute.For<NavigationProxy>();
            var detailNavigationProxy = Substitute.For<NavigationProxy>();
            var emptyModalStack = new List<Page>();

            flyoutPage.NavigationProxy.Returns(navigationProxy);
            flyoutPage.Detail.Returns(detailPage);
            detailPage.NavigationProxy.Returns(detailNavigationProxy);
            navigationProxy.ModalStack.Returns(emptyModalStack);
            detailNavigationProxy.ModalStack.Returns(emptyModalStack);

            // Act
            var result = PageExtensions.GetCurrentPage(flyoutPage);

            // Assert
            Assert.Equal(detailPage, result);
        }

        /// <summary>
        /// Tests that GetCurrentPage handles FlyoutPage with null Detail.
        /// </summary>
        [Fact]
        public void GetCurrentPage_FlyoutPageWithNullDetail_ThrowsNullReferenceException()
        {
            // Arrange
            var flyoutPage = Substitute.For<FlyoutPage>();
            var navigationProxy = Substitute.For<NavigationProxy>();
            var emptyModalStack = new List<Page>();

            flyoutPage.NavigationProxy.Returns(navigationProxy);
            flyoutPage.Detail.Returns((Page)null);
            navigationProxy.ModalStack.Returns(emptyModalStack);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => PageExtensions.GetCurrentPage(flyoutPage));
        }

        /// <summary>
        /// Tests that GetCurrentPage returns PresentedPage for Shell with valid IShellSectionController.
        /// </summary>
        [Fact]
        public void GetCurrentPage_ShellWithValidSectionController_ReturnsPresentedPage()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            var navigationProxy = Substitute.For<NavigationProxy>();
            var shellItem = Substitute.For<ShellItem>();
            var shellSectionController = Substitute.For<IShellSectionController>();
            var presentedPage = Substitute.For<Page>();
            var emptyModalStack = new List<Page>();

            shell.NavigationProxy.Returns(navigationProxy);
            shell.CurrentItem.Returns(shellItem);
            shellItem.CurrentItem.Returns((ShellSection)shellSectionController);
            shellSectionController.PresentedPage.Returns(presentedPage);
            navigationProxy.ModalStack.Returns(emptyModalStack);

            // Act
            var result = PageExtensions.GetCurrentPage(shell);

            // Assert
            Assert.Equal(presentedPage, result);
        }

        /// <summary>
        /// Tests that GetCurrentPage continues when Shell has null CurrentItem.
        /// </summary>
        [Fact]
        public void GetCurrentPage_ShellWithNullCurrentItem_ContinuesToNextCondition()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            var navigationProxy = Substitute.For<NavigationProxy>();
            var emptyModalStack = new List<Page>();

            shell.NavigationProxy.Returns(navigationProxy);
            shell.CurrentItem.Returns((ShellItem)null);
            navigationProxy.ModalStack.Returns(emptyModalStack);

            // Act
            var result = PageExtensions.GetCurrentPage(shell);

            // Assert
            Assert.Equal(shell, result);
        }

        /// <summary>
        /// Tests that GetCurrentPage continues when Shell CurrentItem has null CurrentItem.
        /// </summary>
        [Fact]
        public void GetCurrentPage_ShellCurrentItemWithNullCurrentItem_ContinuesToNextCondition()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            var navigationProxy = Substitute.For<NavigationProxy>();
            var shellItem = Substitute.For<ShellItem>();
            var emptyModalStack = new List<Page>();

            shell.NavigationProxy.Returns(navigationProxy);
            shell.CurrentItem.Returns(shellItem);
            shellItem.CurrentItem.Returns((ShellSection)null);
            navigationProxy.ModalStack.Returns(emptyModalStack);

            // Act
            var result = PageExtensions.GetCurrentPage(shell);

            // Assert
            Assert.Equal(shell, result);
        }

        /// <summary>
        /// Tests that GetCurrentPage continues when Shell CurrentItem.CurrentItem is not IShellSectionController.
        /// </summary>
        [Fact]
        public void GetCurrentPage_ShellCurrentItemNotSectionController_ContinuesToNextCondition()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            var navigationProxy = Substitute.For<NavigationProxy>();
            var shellItem = Substitute.For<ShellItem>();
            var shellSection = Substitute.For<ShellSection>();
            var emptyModalStack = new List<Page>();

            shell.NavigationProxy.Returns(navigationProxy);
            shell.CurrentItem.Returns(shellItem);
            shellItem.CurrentItem.Returns(shellSection);
            navigationProxy.ModalStack.Returns(emptyModalStack);

            // Act
            var result = PageExtensions.GetCurrentPage(shell);

            // Assert
            Assert.Equal(shell, result);
        }

        /// <summary>
        /// Tests that GetCurrentPage recursively calls itself for IPageContainer CurrentPage.
        /// </summary>
        [Fact]
        public void GetCurrentPage_PageContainerWithCurrentPage_ReturnsCurrentPageResult()
        {
            // Arrange
            var pageContainer = Substitute.For<Page, IPageContainer<Page>>();
            var containerCurrentPage = Substitute.For<Page>();
            var navigationProxy = Substitute.For<NavigationProxy>();
            var containerNavigationProxy = Substitute.For<NavigationProxy>();
            var emptyModalStack = new List<Page>();

            pageContainer.NavigationProxy.Returns(navigationProxy);
            ((IPageContainer<Page>)pageContainer).CurrentPage.Returns(containerCurrentPage);
            containerCurrentPage.NavigationProxy.Returns(containerNavigationProxy);
            navigationProxy.ModalStack.Returns(emptyModalStack);
            containerNavigationProxy.ModalStack.Returns(emptyModalStack);

            // Act
            var result = PageExtensions.GetCurrentPage((Page)pageContainer);

            // Assert
            Assert.Equal(containerCurrentPage, result);
        }

        /// <summary>
        /// Tests that GetCurrentPage handles IPageContainer with null CurrentPage.
        /// </summary>
        [Fact]
        public void GetCurrentPage_PageContainerWithNullCurrentPage_ThrowsNullReferenceException()
        {
            // Arrange
            var pageContainer = Substitute.For<Page, IPageContainer<Page>>();
            var navigationProxy = Substitute.For<NavigationProxy>();
            var emptyModalStack = new List<Page>();

            pageContainer.NavigationProxy.Returns(navigationProxy);
            ((IPageContainer<Page>)pageContainer).CurrentPage.Returns((Page)null);
            navigationProxy.ModalStack.Returns(emptyModalStack);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => PageExtensions.GetCurrentPage((Page)pageContainer));
        }

        /// <summary>
        /// Tests that GetCurrentPage returns the original page when no special conditions are met.
        /// </summary>
        [Fact]
        public void GetCurrentPage_RegularPage_ReturnsSamePage()
        {
            // Arrange
            var regularPage = Substitute.For<Page>();
            var navigationProxy = Substitute.For<NavigationProxy>();
            var emptyModalStack = new List<Page>();

            regularPage.NavigationProxy.Returns(navigationProxy);
            navigationProxy.ModalStack.Returns(emptyModalStack);

            // Act
            var result = PageExtensions.GetCurrentPage(regularPage);

            // Assert
            Assert.Equal(regularPage, result);
        }

        /// <summary>
        /// Tests complex nesting scenario: FlyoutPage with Detail that has modal stack.
        /// </summary>
        [Fact]
        public void GetCurrentPage_FlyoutPageWithModalOnDetail_ReturnsModalPage()
        {
            // Arrange
            var flyoutPage = Substitute.For<FlyoutPage>();
            var detailPage = Substitute.For<Page>();
            var modalPage = Substitute.For<Page>();
            var flyoutNavigationProxy = Substitute.For<NavigationProxy>();
            var detailNavigationProxy = Substitute.For<NavigationProxy>();
            var emptyModalStack = new List<Page>();
            var detailModalStack = new List<Page> { modalPage };

            flyoutPage.NavigationProxy.Returns(flyoutNavigationProxy);
            flyoutPage.Detail.Returns(detailPage);
            detailPage.NavigationProxy.Returns(detailNavigationProxy);
            flyoutNavigationProxy.ModalStack.Returns(emptyModalStack);
            detailNavigationProxy.ModalStack.Returns(detailModalStack);

            // Act
            var result = PageExtensions.GetCurrentPage(flyoutPage);

            // Assert
            Assert.Equal(modalPage, result);
        }

        /// <summary>
        /// Tests that GetCurrentPage handles modal stack priority over other page types.
        /// </summary>
        [Fact]
        public void GetCurrentPage_FlyoutPageWithModalStack_ReturnsModalPageNotDetail()
        {
            // Arrange
            var flyoutPage = Substitute.For<FlyoutPage>();
            var detailPage = Substitute.For<Page>();
            var modalPage = Substitute.For<Page>();
            var navigationProxy = Substitute.For<NavigationProxy>();
            var modalStack = new List<Page> { modalPage };

            flyoutPage.NavigationProxy.Returns(navigationProxy);
            flyoutPage.Detail.Returns(detailPage);
            navigationProxy.ModalStack.Returns(modalStack);

            // Act
            var result = PageExtensions.GetCurrentPage(flyoutPage);

            // Assert
            Assert.Equal(modalPage, result);
        }
    }
}
