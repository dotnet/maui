#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class MenuItemTests
    {
        [Fact]
        public void Activated()
        {
            var item = new MenuItem();

            bool activated = false;
            item.Clicked += (sender, args) => activated = true;

            ((IMenuItemController)item).Activate();

            Assert.True(activated);
        }

        [Fact]
        public void Command()
        {
            bool executed = false;
            var param = new object();

            var c = new Command(o =>
            {
                Assert.Same(o, param);
                executed = true;
            });

            var item = new MenuItem { Command = c, CommandParameter = param };
            ((IMenuItemController)item).Activate();

            Assert.True(executed);
        }

        [Fact]
        public void KeyboardAccelerator()
        {
            var item = new MenuFlyoutItem();
            KeyboardAcceleratorModifiers modifiers = KeyboardAcceleratorModifiers.Ctrl;
            string key = "A";
            item.KeyboardAccelerators.Add(new KeyboardAccelerator() { Modifiers = modifiers, Key = key });

            Assert.Equal(item.KeyboardAccelerators[0].Modifiers, modifiers);
            Assert.Equal(item.KeyboardAccelerators[0].Key, key);
        }

        [Fact]
        public void MenuItemsDisabledWhenParentDisabled()
        {
            var item1 = new MenuItem();
            var item2 = new MenuItem();

            var menu = new MenuFlyoutSubItem
            {
                item1, item2
            };

            Assert.True(menu.IsEnabled);
            Assert.True(item1.IsEnabled);
            Assert.True(item2.IsEnabled);

            menu.IsEnabled = false;

            Assert.False(menu.IsEnabled);
            Assert.False(item1.IsEnabled);
            Assert.False(item2.IsEnabled);
        }

        [Fact]
        public void ExplicitlyDisabledMenuItemsRemainsDisabledWhenParentEnabled()
        {
            var item1 = new MenuItem() { IsEnabled = false };

            var menu = new MenuFlyoutSubItem
            {
                item1
            };

            Assert.True(menu.IsEnabled);
            Assert.False(item1.IsEnabled);

            menu.IsEnabled = false;

            Assert.False(menu.IsEnabled);
            Assert.False(item1.IsEnabled);

            menu.IsEnabled = true;

            Assert.True(menu.IsEnabled);
            Assert.False(item1.IsEnabled);
        }

        [Fact]
        public void MenuHierarchyCanBeDisabled()
        {
            var topMenu = new MenuFlyoutSubItem();
            var middleMenu = new MenuFlyoutSubItem();

            var middleItem = new MenuItem();
            var bottomLevelItem1 = new MenuItem();
            var bottomLevelItem2 = new MenuItem();

            middleMenu.Add(bottomLevelItem1);
            middleMenu.Add(bottomLevelItem2);
            topMenu.Add(middleMenu);
            topMenu.Add(middleItem);

            Assert.True(topMenu.IsEnabled);
            Assert.True(middleItem.IsEnabled);
            Assert.True(middleMenu.IsEnabled);
            Assert.True(bottomLevelItem1.IsEnabled);
            Assert.True(bottomLevelItem2.IsEnabled);

            // Disable the entire hierarchy
            topMenu.IsEnabled = false;

            Assert.False(topMenu.IsEnabled);
            Assert.False(middleItem.IsEnabled);
            Assert.False(middleMenu.IsEnabled);
            Assert.False(bottomLevelItem1.IsEnabled);
            Assert.False(bottomLevelItem2.IsEnabled);
        }

        [Fact]
        public void PartialHierarchyCanBeDisabled()
        {
            var topMenu = new MenuFlyoutSubItem();
            var middleMenu = new MenuFlyoutSubItem();

            var middleItem = new MenuItem();
            var bottomLevelItem1 = new MenuItem();
            var bottomLevelItem2 = new MenuItem();

            middleMenu.Add(bottomLevelItem1);
            middleMenu.Add(bottomLevelItem2);
            topMenu.Add(middleMenu);
            topMenu.Add(middleItem);

            Assert.True(topMenu.IsEnabled);
            Assert.True(middleItem.IsEnabled);
            Assert.True(middleMenu.IsEnabled);
            Assert.True(bottomLevelItem1.IsEnabled);
            Assert.True(bottomLevelItem2.IsEnabled);

            // Disable just the bottom level menu
            middleMenu.IsEnabled = false;

            Assert.True(topMenu.IsEnabled);
            Assert.True(middleItem.IsEnabled);
            Assert.False(middleMenu.IsEnabled);
            Assert.False(bottomLevelItem1.IsEnabled);
            Assert.False(bottomLevelItem2.IsEnabled);
        }

        [Fact]
        public async void UpdateMenuBarItemBindingContext()
        {
            bool fired = false;
            var cmd = new Command(() => fired = true);
            var bindingContext = new
            {
                MenuItemCommand = cmd
            };

            MenuFlyoutItem flyout;
            MenuBarItem menuItem;
            var mainPage = new ContentPage
            {
                MenuBarItems =
                {
                    (menuItem = new MenuBarItem
                    {
                        (flyout = new MenuFlyoutItem { })
                    })
                }
            };

            mainPage.BindingContext = bindingContext;
            flyout.SetBinding(MenuFlyoutItem.CommandProperty, new Binding("MenuItemCommand"));

            var page = new ContentPage
            {
                BindingContext = bindingContext
            };

            NavigationPage nav = new NavigationPage(mainPage);
            TestWindow testWindow = new TestWindow(nav);
            await mainPage.Navigation.PushAsync(page);
            await page.Navigation.PopAsync();

            flyout.Command.Execute(null);

            Assert.True(fired);
        }

        /// <summary>
        /// Tests that the IsDestructive property returns the correct default value.
        /// Validates that a newly created MenuItem has IsDestructive set to false by default.
        /// </summary>
        [Fact]
        public void IsDestructive_DefaultValue_ReturnsFalse()
        {
            // Arrange
            var menuItem = new MenuItem();

            // Act
            bool result = menuItem.IsDestructive;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the IsDestructive property can be set to true and retrieved correctly.
        /// Validates the setter and getter functionality when setting to true.
        /// </summary>
        [Fact]
        public void IsDestructive_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var menuItem = new MenuItem();

            // Act
            menuItem.IsDestructive = true;
            bool result = menuItem.IsDestructive;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the IsDestructive property can be set to false and retrieved correctly.
        /// Validates the setter and getter functionality when explicitly setting to false.
        /// </summary>
        [Fact]
        public void IsDestructive_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var menuItem = new MenuItem();

            // Act
            menuItem.IsDestructive = false;
            bool result = menuItem.IsDestructive;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the IsDestructive property can be toggled between true and false values.
        /// Validates that multiple set operations work correctly and maintain state.
        /// </summary>
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void IsDestructive_SetMultipleValues_RetainsCorrectValue(bool firstValue, bool secondValue)
        {
            // Arrange
            var menuItem = new MenuItem();

            // Act
            menuItem.IsDestructive = firstValue;
            bool firstResult = menuItem.IsDestructive;

            menuItem.IsDestructive = secondValue;
            bool secondResult = menuItem.IsDestructive;

            // Assert
            Assert.Equal(firstValue, firstResult);
            Assert.Equal(secondValue, secondResult);
        }

        /// <summary>
        /// Tests that the MenuItem parameterless constructor initializes the object correctly
        /// with all properties set to their expected default values and the object in a valid state.
        /// </summary>
        [Fact]
        public void MenuItem_Constructor_InitializesWithCorrectDefaults()
        {
            // Act
            var menuItem = new MenuItem();

            // Assert
            Assert.Null(menuItem.Command);
            Assert.Null(menuItem.CommandParameter);
            Assert.False(menuItem.IsDestructive);
            Assert.Null(menuItem.IconImageSource);
            Assert.True(menuItem.IsEnabled);
            Assert.Null(menuItem.Text);
        }

        /// <summary>
        /// Tests that the MenuItem constructor creates an object that is in a valid state
        /// and can have events subscribed without throwing exceptions.
        /// </summary>
        [Fact]
        public void MenuItem_Constructor_CreatesValidObjectForEventSubscription()
        {
            // Act
            var menuItem = new MenuItem();
            var eventRaised = false;

            // Assert - Should not throw when subscribing to events
            menuItem.Clicked += (sender, args) => eventRaised = true;

            // Verify the object can be used normally
            Assert.NotNull(menuItem);
            Assert.False(eventRaised);
        }

        /// <summary>
        /// Tests that the MenuItem constructor creates an object that properly implements
        /// all required interfaces and can be cast to them without exceptions.
        /// </summary>
        [Fact]
        public void MenuItem_Constructor_ImplementsRequiredInterfaces()
        {
            // Act
            var menuItem = new MenuItem();

            // Assert - Should be able to cast to all implemented interfaces
            Assert.IsAssignableFrom<BaseMenuItem>(menuItem);
            Assert.IsAssignableFrom<IMenuItemController>(menuItem);
            Assert.IsAssignableFrom<ICommandElement>(menuItem);
            Assert.IsAssignableFrom<Microsoft.Maui.IMenuElement>(menuItem);
            Assert.IsAssignableFrom<IPropertyPropagationController>(menuItem);
        }
    }
}