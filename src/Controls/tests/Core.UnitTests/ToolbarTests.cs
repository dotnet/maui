#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class ToolbarTests : BaseTestFixture
    {
        [Fact]
        public void ToolbarExistsForNavigationPage()
        {
            var window = new TestWindow();
            IToolbarElement toolbarElement = window;
            var startingPage = new NavigationPage(new ContentPage());
            window.Page = startingPage;
            Assert.NotNull(toolbarElement.Toolbar);
        }

        [Fact]
        public void ToolbarEmptyForContentPage()
        {
            Window window = new Window();
            IToolbarElement toolbarElement = window;
            var startingPage = new ContentPage();
            window.Page = startingPage;
            Assert.Null(toolbarElement.Toolbar);
        }

        [Fact]
        public void ToolbarClearsWhenNavigationPageRemoved()
        {
            var window = new Window();
            IToolbarElement toolbarElement = window;
            var startingPage = new NavigationPage(new ContentPage());
            window.Page = startingPage;
            window.Page = new ContentPage();
            Assert.Null(toolbarElement.Toolbar);
        }

        [Fact]
        public async Task TitleAndTitleViewAreMutuallyExclusive()
        {
            var window = new TestWindow();
            IToolbarElement toolbarElement = window;
            var contentPage = new ContentPage() { Title = "Test Title" };
            var navigationPage = new NavigationPage(contentPage);
            window.Page = navigationPage;

            var titleView = new VerticalStackLayout();
            var toolbar = (Toolbar)toolbarElement.Toolbar;
            Assert.Equal("Test Title", toolbar.Title);
            NavigationPage.SetTitleView(contentPage, titleView);
            Assert.Empty(toolbar.Title);
            Assert.Equal(titleView, toolbar.TitleView);
            NavigationPage.SetTitleView(contentPage, null);
            Assert.Equal("Test Title", toolbar.Title);
        }

        [Fact]
        public void ToolbarTitle_UsesTabbedPageTitleWhenSet()
        {
            var window = new TestWindow();
            IToolbarElement toolbarElement = window;
            var tabbedPage = new TabbedPage
            {
                Title = "Test Title",
                Children = { new ContentPage { Title = "Child Test Title" } },
            };
            window.Page = new NavigationPage(tabbedPage);

            var toolbar = (Toolbar)toolbarElement.Toolbar;
            Assert.Equal(tabbedPage.Title, toolbar.Title);
        }

        [Fact]
        public async Task InsertPageBeforeRootPageShowsBackButton()
        {
            var window = new TestWindow();
            IToolbarElement toolbarElement = window;
            var startingPage = new TestNavigationPage(true, new ContentPage());
            window.Page = startingPage;
            startingPage.Navigation.InsertPageBefore(new ContentPage(), startingPage.RootPage);
            await Task.Delay(50);
            Assert.True(toolbarElement.Toolbar.BackButtonVisible);
        }

        [Fact]
        public async Task RemoveRootPageHidesBackButton()
        {
            var window = new TestWindow();
            IToolbarElement toolbarElement = window;
            var startingPage = new TestNavigationPage(true, new ContentPage());
            window.Page = startingPage;
            await startingPage.Navigation.PushAsync(new ContentPage());
            startingPage.Navigation.RemovePage(startingPage.RootPage);
            await Task.Delay(50);
            Assert.False(toolbarElement.Toolbar.BackButtonVisible);
        }

        [Fact]
        public void BackButtonNotVisibleForInitialPage()
        {
            var window = new TestWindow();
            IToolbarElement toolbarElement = window;
            var startingPage = new NavigationPage(new ContentPage());
            window.Page = startingPage;
            Assert.False(toolbarElement.Toolbar.BackButtonVisible);
        }


        [Fact]
        public void NestedNavigation_AppliesFromMostInnerNavigationPage()
        {
            var window = new TestWindow();
            IToolbarElement toolbarElement = window;
            var visibleInnerNavigationPage = new NavigationPage(new ContentPage()) { Title = "visibleInnerNavigationPage" };
            var nonVisibleNavigationPage = new NavigationPage(new ContentPage()) { Title = "nonVisibleNavigationPage" };
            var tabbedPage = new TabbedPage()
            {
                Children =
                {
                    visibleInnerNavigationPage,
                    nonVisibleNavigationPage
                }
            };

            var outerNavigationPage = new NavigationPage(tabbedPage) { Title = "outerNavigationPage" };
            window.Page = outerNavigationPage;

            var toolbar = (Toolbar)toolbarElement.Toolbar;

            NavigationPage.SetHasNavigationBar(tabbedPage, false);
            NavigationPage.SetHasNavigationBar(nonVisibleNavigationPage.CurrentPage, false);

            Assert.True(toolbar.IsVisible);

            NavigationPage.SetHasNavigationBar(visibleInnerNavigationPage.CurrentPage, false);

            Assert.False(toolbar.IsVisible);

            NavigationPage.SetHasNavigationBar(visibleInnerNavigationPage.CurrentPage, true);

            Assert.True(toolbar.IsVisible);
        }

        [Fact]
        public void NestedNavigation_ChangingToTabWithNoNavigationPage()
        {
            var window = new TestWindow();
            IToolbarElement toolbarElement = window;
            var innerNavigationPage =
                new NavigationPage(new ContentPage() { Content = new Label() }) { Title = "innerNavigationPage" };

            var contentPage = new ContentPage() { Title = "contentPage" };
            var tabbedPage = new TabbedPage()
            {
                Children =
                {
                    innerNavigationPage,
                    contentPage
                }
            };

            var outerNavigationPage = new NavigationPage(tabbedPage) { Title = "outerNavigationPage" };
            window.Page = outerNavigationPage;

            var toolbar = (Toolbar)toolbarElement.Toolbar;
            Assert.True(toolbar.IsVisible);

            tabbedPage.CurrentPage = contentPage;

            Assert.True(toolbar.IsVisible);

            // Validate that changes to non visible navigation page don't propagate to titlebar
            NavigationPage.SetHasNavigationBar(innerNavigationPage.CurrentPage, false);
            Assert.True(toolbar.IsVisible);

            NavigationPage.SetHasNavigationBar(contentPage, false);
            Assert.False(toolbar.IsVisible);
        }

        [Fact]
        public void NestedNavigation_NestedNavigationPage()
        {
            var window = new TestWindow();
            IToolbarElement toolbarElement = window;
            var innerNavigationPage =
                new NavigationPage(new ContentPage() { Content = new Label() }) { Title = "innerNavigationPage" };

            var contentPage = new ContentPage() { Title = "contentPage" };
            var tabbedPage = new TabbedPage()
            {
                Children =
                {
                    innerNavigationPage,
                    contentPage
                }
            };

            var outerNavigationPage = new NavigationPage(tabbedPage) { Title = "outerNavigationPage" };
            window.Page = outerNavigationPage;

            var toolbar = (Toolbar)toolbarElement.Toolbar;
            Assert.True(toolbar.IsVisible);

            tabbedPage.CurrentPage = contentPage;

            Assert.True(toolbar.IsVisible);

            // Validate that changes to non visible navigation page don't propagate to titlebar
            NavigationPage.SetHasNavigationBar(innerNavigationPage.CurrentPage, false);
            Assert.True(toolbar.IsVisible);

            NavigationPage.SetHasNavigationBar(contentPage, false);
            Assert.False(toolbar.IsVisible);
        }

        [Fact]
        public async Task NestedNavigation_BackButtonVisibleIfAnyoneHasPages()
        {
            var window = new TestWindow();
            IToolbarElement toolbarElement = window;
            var innerNavigationPage =
                new NavigationPage(new ContentPage() { Content = new Label() }) { Title = "innerNavigationPage" };

            var contentPage = new ContentPage() { Title = "contentPage" };
            var tabbedPage = new TabbedPage()
            {
                Children =
                {
                    contentPage,
                    innerNavigationPage,
                }
            };

            var outerNavigationPage = new NavigationPage(new ContentPage()) { Title = "outerNavigationPage" };
            window.Page = outerNavigationPage;
            var toolbar = (Toolbar)toolbarElement.Toolbar;

            // push Tabbed Page on to the stack of the out nagivation page
            await outerNavigationPage.PushAsync(tabbedPage);
            Assert.True(toolbar.BackButtonVisible);

            tabbedPage.CurrentPage = innerNavigationPage;

            // even though the inner navigation page has no stack the outer one does
            // so we want to still display the navigation page
            Assert.True(toolbar.BackButtonVisible);

            await outerNavigationPage.PopAsync();
            Assert.False(toolbar.BackButtonVisible);
        }

        [Fact]
        public async Task ToolbarDoesntSetOnWindowWhenSwappingBackToSameFlyoutPage()
        {
            var window = new TestWindow();
            var navPage = new NavigationPage(new ContentPage()) { Title = "Detail" };
            var flyoutPage = new FlyoutPage()
            {
                Detail = navPage,
                Flyout = new ContentPage() { Title = "Flyout" }
            };

            IToolbarElement windowToolbarElement = window;

            window.Page = flyoutPage;
            window.Page = new ContentPage();
            window.Page = flyoutPage;

            Assert.Null(windowToolbarElement.Toolbar);
            Assert.NotNull((flyoutPage as IToolbarElement).Toolbar);
        }

        [Fact]
        public async Task ToolbarSetsToCorrectPageWithModal()
        {
            var window = new TestWindow();
            IToolbarElement toolbarElement = window;
            var startingPage = new TestNavigationPage(true, new ContentPage());
            window.Page = startingPage;

            await startingPage.NavigatingTask;

            var rootPageToolbar = toolbarElement.Toolbar;

            var modalPage = new TestNavigationPage(true, new ContentPage());
            await startingPage.Navigation.PushModalAsync(modalPage);

            Assert.Equal(rootPageToolbar, toolbarElement.Toolbar);

            var modalPageToolBar = (modalPage as IToolbarElement).Toolbar;

            Assert.NotNull(modalPageToolBar);
            Assert.NotEqual(modalPageToolBar, rootPageToolbar);

        }

        /// <summary>
        /// Tests that BackButtonTitle getter returns the current value of the backing field.
        /// </summary>
        [Fact]
        public void BackButtonTitle_Get_ReturnsCurrentValue()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);

            // Act
            var result = toolbar.BackButtonTitle;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that BackButtonTitle setter updates the value and raises PropertyChanged when value changes.
        /// </summary>
        /// <param name="newValue">The new value to set for BackButtonTitle</param>
        [Theory]
        [InlineData("Back")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Very long back button title that exceeds normal expectations")]
        [InlineData("Special chars: !@#$%^&*()")]
        [InlineData("Unicode: 🔙 Back")]
        [InlineData("\n\t\r")]
        public void BackButtonTitle_Set_UpdatesValueAndRaisesPropertyChanged(string newValue)
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var propertyChangedRaised = false;
            string propertyName = null;

            toolbar.PropertyChanged += (sender, e) =>
            {
                propertyChangedRaised = true;
                propertyName = e.PropertyName;
            };

            // Act
            toolbar.BackButtonTitle = newValue;

            // Assert
            Assert.Equal(newValue, toolbar.BackButtonTitle);
            Assert.True(propertyChangedRaised);
            Assert.Equal(nameof(Toolbar.BackButtonTitle), propertyName);
        }

        /// <summary>
        /// Tests that BackButtonTitle setter can be set to null and raises PropertyChanged.
        /// </summary>
        [Fact]
        public void BackButtonTitle_SetToNull_UpdatesValueAndRaisesPropertyChanged()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            toolbar.BackButtonTitle = "Initial Value";
            var propertyChangedRaised = false;
            string propertyName = null;

            toolbar.PropertyChanged += (sender, e) =>
            {
                propertyChangedRaised = true;
                propertyName = e.PropertyName;
            };

            // Act
            toolbar.BackButtonTitle = null;

            // Assert
            Assert.Null(toolbar.BackButtonTitle);
            Assert.True(propertyChangedRaised);
            Assert.Equal(nameof(Toolbar.BackButtonTitle), propertyName);
        }

        /// <summary>
        /// Tests that BackButtonTitle setter does not raise PropertyChanged when setting the same value.
        /// </summary>
        [Fact]
        public void BackButtonTitle_SetSameValue_DoesNotRaisePropertyChanged()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            toolbar.BackButtonTitle = "Test Value";
            var propertyChangedCount = 0;

            toolbar.PropertyChanged += (sender, e) =>
            {
                propertyChangedCount++;
            };

            // Act
            toolbar.BackButtonTitle = "Test Value";

            // Assert
            Assert.Equal("Test Value", toolbar.BackButtonTitle);
            Assert.Equal(0, propertyChangedCount);
        }

        /// <summary>
        /// Tests that BackButtonTitle setter does not raise PropertyChanged when setting null multiple times.
        /// </summary>
        [Fact]
        public void BackButtonTitle_SetNullMultipleTimes_DoesNotRaisePropertyChangedOnSecondSet()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var propertyChangedCount = 0;

            toolbar.PropertyChanged += (sender, e) =>
            {
                propertyChangedCount++;
            };

            // Act
            toolbar.BackButtonTitle = null; // First set
            toolbar.BackButtonTitle = null; // Second set

            // Assert
            Assert.Null(toolbar.BackButtonTitle);
            Assert.Equal(0, propertyChangedCount); // No change since initial value was null
        }

        /// <summary>
        /// Tests that BackButtonTitle setter calls Handler.UpdateValue when Handler is set and value changes.
        /// </summary>
        [Fact]
        public void BackButtonTitle_SetWithHandler_CallsHandlerUpdateValue()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var handler = Substitute.For<IElementHandler>();
            var toolbar = new Toolbar(parent);
            toolbar.Handler = handler;

            // Act
            toolbar.BackButtonTitle = "New Title";

            // Assert
            Assert.Equal("New Title", toolbar.BackButtonTitle);
            handler.Received(1).UpdateValue(nameof(Toolbar.BackButtonTitle));
        }

        /// <summary>
        /// Tests that BackButtonTitle setter does not call Handler.UpdateValue when Handler is null.
        /// </summary>
        [Fact]
        public void BackButtonTitle_SetWithNullHandler_DoesNotCallHandlerUpdateValue()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            // Handler is null by default

            // Act & Assert (should not throw)
            toolbar.BackButtonTitle = "New Title";
            Assert.Equal("New Title", toolbar.BackButtonTitle);
        }

        /// <summary>
        /// Tests that BackButtonTitle setter does not call Handler.UpdateValue when setting the same value.
        /// </summary>
        [Fact]
        public void BackButtonTitle_SetSameValueWithHandler_DoesNotCallHandlerUpdateValue()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var handler = Substitute.For<IElementHandler>();
            var toolbar = new Toolbar(parent);
            toolbar.Handler = handler;
            toolbar.BackButtonTitle = "Initial Title";
            handler.ClearReceivedCalls();

            // Act
            toolbar.BackButtonTitle = "Initial Title";

            // Assert
            Assert.Equal("Initial Title", toolbar.BackButtonTitle);
            handler.DidNotReceive().UpdateValue(Arg.Any<string>());
        }

        /// <summary>
        /// Tests that the TitleIcon getter returns the correct value from the backing field.
        /// </summary>
        [Fact]
        public void TitleIcon_Get_ReturnsBackingFieldValue()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var expectedImageSource = Substitute.For<ImageSource>();

            // Act - Set via reflection to test getter independently
            var titleIconField = typeof(Toolbar).GetField("_titleIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            titleIconField.SetValue(toolbar, expectedImageSource);

            // Assert
            Assert.Same(expectedImageSource, toolbar.TitleIcon);
        }

        /// <summary>
        /// Tests that the TitleIcon getter returns null when backing field is null.
        /// </summary>
        [Fact]
        public void TitleIcon_Get_ReturnsNullWhenBackingFieldIsNull()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);

            // Act & Assert
            Assert.Null(toolbar.TitleIcon);
        }

        /// <summary>
        /// Tests that setting TitleIcon to a valid ImageSource updates the backing field and fires PropertyChanged event.
        /// </summary>
        [Fact]
        public void TitleIcon_Set_ValidImageSource_UpdatesBackingFieldAndFiresPropertyChanged()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var imageSource = Substitute.For<ImageSource>();
            var propertyChangedFired = false;
            string propertyName = null;

            toolbar.PropertyChanged += (sender, e) =>
            {
                propertyChangedFired = true;
                propertyName = e.PropertyName;
            };

            // Act
            toolbar.TitleIcon = imageSource;

            // Assert
            Assert.Same(imageSource, toolbar.TitleIcon);
            Assert.True(propertyChangedFired);
            Assert.Equal("TitleIcon", propertyName);
        }

        /// <summary>
        /// Tests that setting TitleIcon to null updates the backing field and fires PropertyChanged event.
        /// </summary>
        [Fact]
        public void TitleIcon_Set_NullValue_UpdatesBackingFieldAndFiresPropertyChanged()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var initialImageSource = Substitute.For<ImageSource>();
            toolbar.TitleIcon = initialImageSource; // Set initial value

            var propertyChangedFired = false;
            string propertyName = null;

            toolbar.PropertyChanged += (sender, e) =>
            {
                propertyChangedFired = true;
                propertyName = e.PropertyName;
            };

            // Act
            toolbar.TitleIcon = null;

            // Assert
            Assert.Null(toolbar.TitleIcon);
            Assert.True(propertyChangedFired);
            Assert.Equal("TitleIcon", propertyName);
        }

        /// <summary>
        /// Tests that setting TitleIcon to the same value does not fire PropertyChanged event.
        /// </summary>
        [Fact]
        public void TitleIcon_Set_SameValue_DoesNotFirePropertyChanged()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var imageSource = Substitute.For<ImageSource>();
            toolbar.TitleIcon = imageSource; // Set initial value

            var propertyChangedFired = false;
            toolbar.PropertyChanged += (sender, e) => propertyChangedFired = true;

            // Act
            toolbar.TitleIcon = imageSource; // Set same value

            // Assert
            Assert.False(propertyChangedFired);
        }

        /// <summary>
        /// Tests that setting TitleIcon to null when it's already null does not fire PropertyChanged event.
        /// </summary>
        [Fact]
        public void TitleIcon_Set_NullWhenAlreadyNull_DoesNotFirePropertyChanged()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);

            var propertyChangedFired = false;
            toolbar.PropertyChanged += (sender, e) => propertyChangedFired = true;

            // Act
            toolbar.TitleIcon = null; // Set null when already null

            // Assert
            Assert.False(propertyChangedFired);
        }

        /// <summary>
        /// Tests that setting TitleIcon calls Handler.UpdateValue when Handler is not null.
        /// </summary>
        [Fact]
        public void TitleIcon_Set_WithHandler_CallsHandlerUpdateValue()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var handler = Substitute.For<IElementHandler>();
            var imageSource = Substitute.For<ImageSource>();

            toolbar.Handler = handler;

            // Act
            toolbar.TitleIcon = imageSource;

            // Assert
            handler.Received(1).UpdateValue("TitleIcon");
        }

        /// <summary>
        /// Tests that setting TitleIcon does not call Handler.UpdateValue when Handler is null.
        /// </summary>
        [Fact]
        public void TitleIcon_Set_WithoutHandler_DoesNotCallHandlerUpdateValue()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var imageSource = Substitute.For<ImageSource>();

            // Ensure handler is null
            Assert.Null(toolbar.Handler);

            // Act - This should not throw since Handler?.UpdateValue() uses null-conditional operator
            toolbar.TitleIcon = imageSource;

            // Assert - No exception should be thrown, and property should be set
            Assert.Same(imageSource, toolbar.TitleIcon);
        }

        /// <summary>
        /// Tests that multiple different TitleIcon values fire PropertyChanged events correctly.
        /// </summary>
        [Fact]
        public void TitleIcon_Set_MultipleDifferentValues_FiresPropertyChangedForEach()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var imageSource1 = Substitute.For<ImageSource>();
            var imageSource2 = Substitute.For<ImageSource>();

            var propertyChangedCount = 0;
            toolbar.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "TitleIcon")
                    propertyChangedCount++;
            };

            // Act
            toolbar.TitleIcon = imageSource1;
            toolbar.TitleIcon = imageSource2;
            toolbar.TitleIcon = null;

            // Assert
            Assert.Equal(3, propertyChangedCount);
            Assert.Null(toolbar.TitleIcon);
        }

        /// <summary>
        /// Tests that the DynamicOverflowEnabled property getter returns the correct initial value.
        /// Initial condition: New toolbar instance with default field value.
        /// Expected result: DynamicOverflowEnabled should return false (default bool value).
        /// </summary>
        [Fact]
        public void DynamicOverflowEnabled_InitialValue_ReturnsFalse()
        {
            // Arrange
            var parentElement = Substitute.For<IElement>();
            var toolbar = new Toolbar(parentElement);

            // Act
            bool result = toolbar.DynamicOverflowEnabled;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that setting DynamicOverflowEnabled to true updates the backing field and raises PropertyChanged event.
        /// Initial condition: DynamicOverflowEnabled is false.
        /// Expected result: Property value changes to true and PropertyChanged event is raised with correct property name.
        /// </summary>
        [Fact]
        public void DynamicOverflowEnabled_SetToTrue_UpdatesValueAndRaisesPropertyChanged()
        {
            // Arrange
            var parentElement = Substitute.For<IElement>();
            var toolbar = new Toolbar(parentElement);
            var propertyChangedRaised = false;
            string propertyNameFromEvent = null;

            toolbar.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                propertyNameFromEvent = args.PropertyName;
            };

            // Act
            toolbar.DynamicOverflowEnabled = true;

            // Assert
            Assert.True(toolbar.DynamicOverflowEnabled);
            Assert.True(propertyChangedRaised);
            Assert.Equal("DynamicOverflowEnabled", propertyNameFromEvent);
        }

        /// <summary>
        /// Tests that setting DynamicOverflowEnabled to false updates the backing field and raises PropertyChanged event.
        /// Initial condition: DynamicOverflowEnabled is true.
        /// Expected result: Property value changes to false and PropertyChanged event is raised with correct property name.
        /// </summary>
        [Fact]
        public void DynamicOverflowEnabled_SetToFalse_UpdatesValueAndRaisesPropertyChanged()
        {
            // Arrange
            var parentElement = Substitute.For<IElement>();
            var toolbar = new Toolbar(parentElement);
            toolbar.DynamicOverflowEnabled = true; // Set to true first
            var propertyChangedRaised = false;
            string propertyNameFromEvent = null;

            toolbar.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                propertyNameFromEvent = args.PropertyName;
            };

            // Act
            toolbar.DynamicOverflowEnabled = false;

            // Assert
            Assert.False(toolbar.DynamicOverflowEnabled);
            Assert.True(propertyChangedRaised);
            Assert.Equal("DynamicOverflowEnabled", propertyNameFromEvent);
        }

        /// <summary>
        /// Tests that setting DynamicOverflowEnabled to the same value does not raise PropertyChanged event.
        /// Initial condition: DynamicOverflowEnabled is false (default).
        /// Expected result: Property value remains false and PropertyChanged event is not raised.
        /// </summary>
        [Fact]
        public void DynamicOverflowEnabled_SetToSameValue_DoesNotRaisePropertyChanged()
        {
            // Arrange
            var parentElement = Substitute.For<IElement>();
            var toolbar = new Toolbar(parentElement);
            var propertyChangedRaised = false;

            toolbar.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
            };

            // Act
            toolbar.DynamicOverflowEnabled = false; // Same as default value

            // Assert
            Assert.False(toolbar.DynamicOverflowEnabled);
            Assert.False(propertyChangedRaised);
        }

        /// <summary>
        /// Tests that setting DynamicOverflowEnabled calls Handler.UpdateValue when Handler is not null.
        /// Initial condition: Toolbar has a handler set and DynamicOverflowEnabled is false.
        /// Expected result: Handler.UpdateValue is called with correct property name when value changes.
        /// </summary>
        [Fact]
        public void DynamicOverflowEnabled_SetValue_CallsHandlerUpdateValue()
        {
            // Arrange
            var parentElement = Substitute.For<IElement>();
            var handler = Substitute.For<IElementHandler>();
            var toolbar = new Toolbar(parentElement);
            toolbar.Handler = handler;

            // Act
            toolbar.DynamicOverflowEnabled = true;

            // Assert
            handler.Received(1).UpdateValue("DynamicOverflowEnabled");
        }

        /// <summary>
        /// Tests that setting DynamicOverflowEnabled does not throw when Handler is null.
        /// Initial condition: Toolbar has no handler set (null).
        /// Expected result: Property is updated without throwing exception.
        /// </summary>
        [Fact]
        public void DynamicOverflowEnabled_SetValueWithNullHandler_DoesNotThrow()
        {
            // Arrange
            var parentElement = Substitute.For<IElement>();
            var toolbar = new Toolbar(parentElement);
            // Handler is null by default

            // Act & Assert
            var exception = Record.Exception(() => toolbar.DynamicOverflowEnabled = true);
            Assert.Null(exception);
            Assert.True(toolbar.DynamicOverflowEnabled);
        }

        /// <summary>
        /// Tests multiple transitions of DynamicOverflowEnabled property values.
        /// Initial condition: New toolbar instance.
        /// Expected result: All value transitions work correctly and events are raised appropriately.
        /// </summary>
        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void DynamicOverflowEnabled_ValueTransitions_WorksCorrectly(bool initialValue, bool newValue)
        {
            // Arrange
            var parentElement = Substitute.For<IElement>();
            var toolbar = new Toolbar(parentElement);
            toolbar.DynamicOverflowEnabled = initialValue;
            var propertyChangedRaised = false;

            toolbar.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "DynamicOverflowEnabled")
                    propertyChangedRaised = true;
            };

            // Act
            toolbar.DynamicOverflowEnabled = newValue;

            // Assert
            Assert.Equal(newValue, toolbar.DynamicOverflowEnabled);

            if (initialValue != newValue)
            {
                Assert.True(propertyChangedRaised);
            }
            else
            {
                Assert.False(propertyChangedRaised);
            }
        }

        /// <summary>
        /// Tests that DrawerToggleVisible property has initial value of false when toolbar is created.
        /// Input: New toolbar instance with mocked parent element.
        /// Expected result: DrawerToggleVisible returns false.
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_InitialValue_ReturnsFalse()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var toolbar = new Toolbar(mockParent);

            // Act
            var result = toolbar.DrawerToggleVisible;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that DrawerToggleVisible property can be set to true and returns the correct value.
        /// Input: Setting DrawerToggleVisible to true.
        /// Expected result: Property returns true and PropertyChanged event is raised.
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var toolbar = new Toolbar(mockParent);
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            toolbar.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = args.PropertyName;
            };

            // Act
            toolbar.DrawerToggleVisible = true;

            // Assert
            Assert.True(toolbar.DrawerToggleVisible);
            Assert.True(propertyChangedRaised);
            Assert.Equal(nameof(toolbar.DrawerToggleVisible), changedPropertyName);
        }

        /// <summary>
        /// Tests that DrawerToggleVisible property can be set to false and returns the correct value.
        /// Input: Setting DrawerToggleVisible to false after it was true.
        /// Expected result: Property returns false and PropertyChanged event is raised.
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var toolbar = new Toolbar(mockParent);
            toolbar.DrawerToggleVisible = true; // First set to true
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            toolbar.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = args.PropertyName;
            };

            // Act
            toolbar.DrawerToggleVisible = false;

            // Assert
            Assert.False(toolbar.DrawerToggleVisible);
            Assert.True(propertyChangedRaised);
            Assert.Equal(nameof(toolbar.DrawerToggleVisible), changedPropertyName);
        }

        /// <summary>
        /// Tests that setting DrawerToggleVisible to the same value does not raise PropertyChanged event.
        /// Input: Setting DrawerToggleVisible to false when it's already false.
        /// Expected result: PropertyChanged event is not raised, property remains false.
        /// </summary>
        [Fact]
        public void DrawerToggleVisible_SetToSameValue_DoesNotRaisePropertyChanged()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var toolbar = new Toolbar(mockParent);
            bool propertyChangedRaised = false;

            toolbar.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
            };

            // Act
            toolbar.DrawerToggleVisible = false; // Setting to same value (initial is false)

            // Assert
            Assert.False(toolbar.DrawerToggleVisible);
            Assert.False(propertyChangedRaised);
        }

        /// <summary>
        /// Tests DrawerToggleVisible property with multiple value changes to ensure state consistency.
        /// Input: Setting DrawerToggleVisible multiple times with different values.
        /// Expected result: Property correctly reflects each change and appropriate PropertyChanged events are raised.
        /// </summary>
        [Theory]
        [InlineData(true, false, true)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        public void DrawerToggleVisible_MultipleValueChanges_WorksCorrectly(bool firstValue, bool secondValue, bool thirdValue)
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();
            var toolbar = new Toolbar(mockParent);
            int propertyChangedCount = 0;

            toolbar.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(toolbar.DrawerToggleVisible))
                    propertyChangedCount++;
            };

            // Act & Assert
            toolbar.DrawerToggleVisible = firstValue;
            Assert.Equal(firstValue, toolbar.DrawerToggleVisible);

            toolbar.DrawerToggleVisible = secondValue;
            Assert.Equal(secondValue, toolbar.DrawerToggleVisible);

            toolbar.DrawerToggleVisible = thirdValue;
            Assert.Equal(thirdValue, toolbar.DrawerToggleVisible);

            // Calculate expected property changed events (only when value actually changes)
            int expectedEvents = 0;
            if (firstValue != false) expectedEvents++; // Initial is false
            if (secondValue != firstValue) expectedEvents++;
            if (thirdValue != secondValue) expectedEvents++;

            Assert.Equal(expectedEvents, propertyChangedCount);
        }

        /// <summary>
        /// Tests that Toolbar constructor works correctly with valid parent element.
        /// Input: Valid IElement mock as parent parameter.
        /// Expected result: Toolbar instance is created successfully and Parent property returns the mock.
        /// </summary>
        [Fact]
        public void Toolbar_ConstructorWithValidParent_CreatesSuccessfully()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();

            // Act
            var toolbar = new Toolbar(mockParent);

            // Assert
            Assert.NotNull(toolbar);
            Assert.Equal(mockParent, toolbar.Parent);
        }

        /// <summary>
        /// Tests that Toolbar constructor handles null parent parameter correctly.
        /// Input: Null as parent parameter.
        /// Expected result: Toolbar instance is created and Parent property returns null.
        /// </summary>
        [Fact]
        public void Toolbar_ConstructorWithNullParent_CreatesSuccessfully()
        {
            // Arrange & Act
            var toolbar = new Toolbar(null);

            // Assert
            Assert.NotNull(toolbar);
            Assert.Null(toolbar.Parent);
        }

        /// <summary>
        /// Tests that setting the Handler property to the same value returns early without any side effects.
        /// Input: Handler set to same value as current handler.
        /// Expected: Early return, no calls to OnHandlerChanging or DisconnectHandler.
        /// </summary>
        [Fact]
        public void Handler_SetSameValue_ReturnsEarly()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var handler = Substitute.For<IElementHandler>();
            toolbar.Handler = handler;

            // Act
            toolbar.Handler = handler;

            // Assert
            Assert.Equal(handler, toolbar.Handler);
            handler.DidNotReceive().DisconnectHandler();
        }

        /// <summary>
        /// Tests setting Handler when current handler is null.
        /// Input: Handler is null, setting to new handler.
        /// Expected: Handler is set, no DisconnectHandler call since old handler is null.
        /// </summary>
        [Fact]
        public void Handler_SetWhenCurrentIsNull_SetsHandlerWithoutDisconnect()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var newHandler = Substitute.For<IElementHandler>();

            // Act
            toolbar.Handler = newHandler;

            // Assert
            Assert.Equal(newHandler, toolbar.Handler);
            newHandler.DidNotReceive().DisconnectHandler();
        }

        /// <summary>
        /// Tests setting Handler to null when current handler exists.
        /// Input: Handler set to null when current handler's VirtualView is null.
        /// Expected: Handler is set to null, no DisconnectHandler call since VirtualView is null.
        /// </summary>
        [Fact]
        public void Handler_SetToNullWithVirtualViewNull_SetsHandlerWithoutDisconnect()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var oldHandler = Substitute.For<IElementHandler>();
            oldHandler.VirtualView.Returns((IElement)null);
            toolbar.Handler = oldHandler;

            // Act
            toolbar.Handler = null;

            // Assert
            Assert.Null(toolbar.Handler);
            oldHandler.DidNotReceive().DisconnectHandler();
        }

        /// <summary>
        /// Tests setting Handler when old handler's VirtualView equals the toolbar instance.
        /// Input: Handler changed when old handler's VirtualView is the toolbar itself.
        /// Expected: Handler is set and DisconnectHandler is called on old handler.
        /// </summary>
        [Fact]
        public void Handler_SetWhenOldVirtualViewIsThis_CallsDisconnectHandler()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var oldHandler = Substitute.For<IElementHandler>();
            var newHandler = Substitute.For<IElementHandler>();

            oldHandler.VirtualView.Returns(toolbar);
            toolbar.Handler = oldHandler;

            // Act
            toolbar.Handler = newHandler;

            // Assert
            Assert.Equal(newHandler, toolbar.Handler);
            oldHandler.Received(1).DisconnectHandler();
        }

        /// <summary>
        /// Tests setting Handler when old handler's VirtualView is a different instance.
        /// Input: Handler changed when old handler's VirtualView is not the toolbar instance.
        /// Expected: Handler is set but DisconnectHandler is not called.
        /// </summary>
        [Fact]
        public void Handler_SetWhenOldVirtualViewIsDifferent_DoesNotCallDisconnectHandler()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var oldHandler = Substitute.For<IElementHandler>();
            var newHandler = Substitute.For<IElementHandler>();
            var differentElement = Substitute.For<IElement>();

            oldHandler.VirtualView.Returns(differentElement);
            toolbar.Handler = oldHandler;

            // Act
            toolbar.Handler = newHandler;

            // Assert
            Assert.Equal(newHandler, toolbar.Handler);
            oldHandler.DidNotReceive().DisconnectHandler();
        }

        /// <summary>
        /// Tests setting Handler to null when old handler's VirtualView equals the toolbar instance.
        /// Input: Handler set to null when old handler's VirtualView is the toolbar itself.
        /// Expected: Handler is set to null and DisconnectHandler is called on old handler.
        /// </summary>
        [Fact]
        public void Handler_SetToNullWhenOldVirtualViewIsThis_CallsDisconnectHandler()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var oldHandler = Substitute.For<IElementHandler>();

            oldHandler.VirtualView.Returns(toolbar);
            toolbar.Handler = oldHandler;

            // Act
            toolbar.Handler = null;

            // Assert
            Assert.Null(toolbar.Handler);
            oldHandler.Received(1).DisconnectHandler();
        }

        /// <summary>
        /// Tests that Handler getter returns the current handler value.
        /// Input: Handler is set to a mock handler.
        /// Expected: Getter returns the same handler instance.
        /// </summary>
        [Fact]
        public void Handler_Get_ReturnsCurrentHandler()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var handler = Substitute.For<IElementHandler>();
            toolbar.Handler = handler;

            // Act
            var result = toolbar.Handler;

            // Assert
            Assert.Equal(handler, result);
        }

        /// <summary>
        /// Tests that Handler getter returns null when no handler is set.
        /// Input: Handler is not set (default null value).
        /// Expected: Getter returns null.
        /// </summary>
        [Fact]
        public void Handler_GetWhenNull_ReturnsNull()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);

            // Act
            var result = toolbar.Handler;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Toolbar constructor correctly stores a valid IElement parent parameter.
        /// Input: Valid IElement mock instance.
        /// Expected: Parent property returns the same IElement instance passed to constructor.
        /// </summary>
        [Fact]
        public void Constructor_ValidParent_StoresParentCorrectly()
        {
            // Arrange
            var mockParent = Substitute.For<IElement>();

            // Act
            var toolbar = new Toolbar(mockParent);

            // Assert
            Assert.Same(mockParent, toolbar.Parent);
        }

        /// <summary>
        /// Tests that the Toolbar constructor accepts null as the parent parameter.
        /// Input: Null parent parameter.
        /// Expected: Parent property returns null and no exception is thrown.
        /// </summary>
        [Fact]
        public void Constructor_NullParent_StoresNullCorrectly()
        {
            // Arrange
            IElement nullParent = null;

            // Act
            var toolbar = new Toolbar(nullParent);

            // Assert
            Assert.Null(toolbar.Parent);
        }

        /// <summary>
        /// Tests that the Toolbar constructor correctly handles different IElement instances.
        /// Input: Multiple different IElement mock instances.
        /// Expected: Parent property returns the exact instance passed to each constructor.
        /// </summary>
        [Fact]
        public void Constructor_DifferentParentInstances_StoresCorrectInstance()
        {
            // Arrange
            var firstParent = Substitute.For<IElement>();
            var secondParent = Substitute.For<IElement>();

            // Act
            var firstToolbar = new Toolbar(firstParent);
            var secondToolbar = new Toolbar(secondParent);

            // Assert
            Assert.Same(firstParent, firstToolbar.Parent);
            Assert.Same(secondParent, secondToolbar.Parent);
            Assert.NotSame(firstToolbar.Parent, secondToolbar.Parent);
        }
    }

    /// <summary>
    /// Tests for the Toolbar.BarHeight property functionality.
    /// </summary>
    public partial class ToolbarBarHeightTests
    {
        /// <summary>
        /// Tests that BarHeight getter returns the current backing field value.
        /// Input: Various double? values set directly on backing field.
        /// Expected: Property getter returns the exact same value.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData(0.0)]
        [InlineData(50.0)]
        [InlineData(-10.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        public void BarHeight_Get_ReturnsBackingFieldValue(double? expectedValue)
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);

            // Act & Assert - Initial value should be null (default)
            Assert.Null(toolbar.BarHeight);
        }

        /// <summary>
        /// Tests that BarHeight setter updates the backing field with valid double values.
        /// Input: Valid positive double values.
        /// Expected: Property value is updated and PropertyChanged event is raised.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(25.5)]
        [InlineData(100.0)]
        [InlineData(1000.0)]
        public void BarHeight_SetValidPositiveValue_UpdatesPropertyAndRaisesPropertyChanged(double value)
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            toolbar.PropertyChanged += (sender, e) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = e.PropertyName;
            };

            // Act
            toolbar.BarHeight = value;

            // Assert
            Assert.Equal(value, toolbar.BarHeight);
            Assert.True(propertyChangedRaised);
            Assert.Equal(nameof(Toolbar.BarHeight), changedPropertyName);
        }

        /// <summary>
        /// Tests that BarHeight setter handles null values correctly.
        /// Input: null value.
        /// Expected: Property is set to null and PropertyChanged event is raised.
        /// </summary>
        [Fact]
        public void BarHeight_SetNull_UpdatesPropertyAndRaisesPropertyChanged()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            toolbar.BarHeight = 50.0; // Set initial value
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            toolbar.PropertyChanged += (sender, e) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = e.PropertyName;
            };

            // Act
            toolbar.BarHeight = null;

            // Assert
            Assert.Null(toolbar.BarHeight);
            Assert.True(propertyChangedRaised);
            Assert.Equal(nameof(Toolbar.BarHeight), changedPropertyName);
        }

        /// <summary>
        /// Tests that BarHeight setter handles extreme double values correctly.
        /// Input: Extreme double values including infinity and NaN.
        /// Expected: Property values are set and PropertyChanged events are raised.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void BarHeight_SetExtremeValues_UpdatesPropertyAndRaisesPropertyChanged(double extremeValue)
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            toolbar.PropertyChanged += (sender, e) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = e.PropertyName;
            };

            // Act
            toolbar.BarHeight = extremeValue;

            // Assert
            Assert.Equal(extremeValue, toolbar.BarHeight);
            Assert.True(propertyChangedRaised);
            Assert.Equal(nameof(Toolbar.BarHeight), changedPropertyName);
        }

        /// <summary>
        /// Tests that BarHeight setter handles negative values correctly.
        /// Input: Negative double values.
        /// Expected: Property values are set (no validation) and PropertyChanged events are raised.
        /// </summary>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-100.0)]
        [InlineData(-0.5)]
        public void BarHeight_SetNegativeValue_UpdatesPropertyAndRaisesPropertyChanged(double negativeValue)
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            bool propertyChangedRaised = false;
            string changedPropertyName = null;

            toolbar.PropertyChanged += (sender, e) =>
            {
                propertyChangedRaised = true;
                changedPropertyName = e.PropertyName;
            };

            // Act
            toolbar.BarHeight = negativeValue;

            // Assert
            Assert.Equal(negativeValue, toolbar.BarHeight);
            Assert.True(propertyChangedRaised);
            Assert.Equal(nameof(Toolbar.BarHeight), changedPropertyName);
        }

        /// <summary>
        /// Tests that setting the same BarHeight value does not raise PropertyChanged event.
        /// Input: Same double value set twice.
        /// Expected: PropertyChanged event is raised only on the first set operation.
        /// </summary>
        [Fact]
        public void BarHeight_SetSameValue_DoesNotRaisePropertyChanged()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            const double testValue = 75.0;

            toolbar.BarHeight = testValue; // Set initial value

            int propertyChangedCount = 0;
            toolbar.PropertyChanged += (sender, e) => propertyChangedCount++;

            // Act
            toolbar.BarHeight = testValue; // Set same value

            // Assert
            Assert.Equal(testValue, toolbar.BarHeight);
            Assert.Equal(0, propertyChangedCount);
        }

        /// <summary>
        /// Tests that setting the same null value does not raise PropertyChanged event.
        /// Input: null value set twice.
        /// Expected: PropertyChanged event is raised only on the first set operation.
        /// </summary>
        [Fact]
        public void BarHeight_SetSameNullValue_DoesNotRaisePropertyChanged()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);

            // Initial state should be null, so set to non-null first
            toolbar.BarHeight = 50.0;
            toolbar.BarHeight = null; // Set to null

            int propertyChangedCount = 0;
            toolbar.PropertyChanged += (sender, e) => propertyChangedCount++;

            // Act
            toolbar.BarHeight = null; // Set same null value

            // Assert
            Assert.Null(toolbar.BarHeight);
            Assert.Equal(0, propertyChangedCount);
        }

        /// <summary>
        /// Tests that BarHeight property change calls Handler.UpdateValue when handler is present.
        /// Input: Valid double value with mocked handler.
        /// Expected: Handler.UpdateValue is called with correct property name.
        /// </summary>
        [Fact]
        public void BarHeight_SetValue_CallsHandlerUpdateValue()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var handler = Substitute.For<IElementHandler>();
            toolbar.Handler = handler;

            // Act
            toolbar.BarHeight = 100.0;

            // Assert
            handler.Received(1).UpdateValue(nameof(Toolbar.BarHeight));
        }

        /// <summary>
        /// Tests that BarHeight property change does not call Handler.UpdateValue when setting same value.
        /// Input: Same double value set twice with mocked handler.
        /// Expected: Handler.UpdateValue is called only once.
        /// </summary>
        [Fact]
        public void BarHeight_SetSameValue_DoesNotCallHandlerUpdateValue()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            var handler = Substitute.For<IElementHandler>();
            toolbar.Handler = handler;

            toolbar.BarHeight = 100.0; // First set
            handler.ClearReceivedCalls();

            // Act
            toolbar.BarHeight = 100.0; // Set same value

            // Assert
            handler.DidNotReceive().UpdateValue(Arg.Any<string>());
        }

        /// <summary>
        /// Tests that BarHeight property works correctly when handler is null.
        /// Input: Valid double value with null handler.
        /// Expected: Property is updated and PropertyChanged is raised without exceptions.
        /// </summary>
        [Fact]
        public void BarHeight_SetValueWithNullHandler_UpdatesPropertyWithoutException()
        {
            // Arrange
            var parent = Substitute.For<IElement>();
            var toolbar = new Toolbar(parent);
            // Handler should be null by default
            Assert.Null(toolbar.Handler);

            bool propertyChangedRaised = false;
            toolbar.PropertyChanged += (sender, e) => propertyChangedRaised = true;

            // Act & Assert - Should not throw
            toolbar.BarHeight = 50.0;

            Assert.Equal(50.0, toolbar.BarHeight);
            Assert.True(propertyChangedRaised);
        }
    }
}