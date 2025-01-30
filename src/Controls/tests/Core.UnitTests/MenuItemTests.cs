using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class MenuItemTests
		: MenuItemTests<MenuItem>
	{
	}


	public abstract class MenuItemTests<T>
		: CommandSourceTests<T>
		where T : MenuItem, new()
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

		protected override T CreateSource()
		{
			return new T();
		}

		protected override void Activate(T source)
		{
			((IMenuItemController)source).Activate();
		}

		protected override BindableProperty IsEnabledProperty
		{
			get { return MenuItem.IsEnabledProperty; }
		}

		protected override BindableProperty CommandProperty
		{
			get { return MenuItem.CommandProperty; }
		}

		protected override BindableProperty CommandParameterProperty
		{
			get { return MenuItem.CommandParameterProperty; }
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
	}
}
