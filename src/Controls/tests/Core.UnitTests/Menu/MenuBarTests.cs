#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Handlers;
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
	}
}
