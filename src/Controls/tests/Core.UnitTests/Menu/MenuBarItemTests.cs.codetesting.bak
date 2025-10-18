#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Menu
{
	[Category("MenuBarItem")]
	public class MenuBarItemTests :
		MenuTestBase<MenuBarItem, IMenuElement, MenuFlyoutItem, MenuBarItemHandlerUpdate>
	{
		[Fact]
		public void StartsEnabled()
		{
			MenuBarItem menuBarItem = new MenuBarItem();
			Assert.True(menuBarItem.IsEnabled);
		}

		[Fact]
		public void DisableWorks()
		{
			MenuBarItem menuBarItem = new MenuBarItem();
			menuBarItem.IsEnabled = false;
			Assert.False(menuBarItem.IsEnabled);
		}

		protected override int GetIndex(MenuBarItemHandlerUpdate handlerUpdate) =>
			handlerUpdate.Index;

		protected override IMenuElement GetItem(MenuBarItemHandlerUpdate handlerUpdate) =>
			handlerUpdate.MenuElement;

		protected override void SetHandler(Maui.IElement element, List<(string Name, MenuBarItemHandlerUpdate? Args)> events)
		{
			element.Handler = CreateMenuBarItemHandler((n, h, l, a) => events.Add((n, a)));
		}

		MenuBarItemHandler CreateMenuBarItemHandler(Action<string, IMenuBarItemHandler, IMenuBarItem, MenuBarItemHandlerUpdate?>? action)
		{
			var handler = new NonThrowingMenuBarItemHandler(
				MenuBarItemHandler.Mapper,
				new CommandMapper<IMenuBarItem, IMenuBarItemHandler>(MenuBarItemHandler.CommandMapper)
				{
					[nameof(IMenuBarItemHandler.Add)] = (h, l, a) => action?.Invoke(nameof(IMenuBarItemHandler.Add), h, l, (MenuBarItemHandlerUpdate?)a),
					[nameof(IMenuBarItemHandler.Remove)] = (h, l, a) => action?.Invoke(nameof(IMenuBarItemHandler.Remove), h, l, (MenuBarItemHandlerUpdate?)a),
					[nameof(IMenuBarItemHandler.Clear)] = (h, l, a) => action?.Invoke(nameof(IMenuBarItemHandler.Clear), h, l, (MenuBarItemHandlerUpdate?)a),
					[nameof(IMenuBarItemHandler.Insert)] = (h, l, a) => action?.Invoke(nameof(IMenuBarItemHandler.Insert), h, l, (MenuBarItemHandlerUpdate?)a),
				});

			return handler;
		}

		class NonThrowingMenuBarItemHandler : MenuBarItemHandler
		{
			public NonThrowingMenuBarItemHandler(IPropertyMapper mapper, CommandMapper commandMapper)
				: base(mapper, commandMapper)
			{
			}

			protected override object CreatePlatformElement() => new object();
		}
	}
}
