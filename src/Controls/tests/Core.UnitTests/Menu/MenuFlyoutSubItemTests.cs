#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Menu
{
	[Category("MenuFlyoutSubItem")]
	public class MenuFlyoutSubItemTests :
		MenuTestBase<MenuFlyoutSubItem, IMenuElement, MenuFlyoutItem, MenuFlyoutSubItemHandlerUpdate>
	{
		protected override int GetIndex(MenuFlyoutSubItemHandlerUpdate handlerUpdate) =>
			handlerUpdate.Index;

		protected override IMenuElement GetItem(MenuFlyoutSubItemHandlerUpdate handlerUpdate) =>
			handlerUpdate.MenuElement;

		protected override void SetHandler(Maui.IElement element, List<(string Name, MenuFlyoutSubItemHandlerUpdate? Args)> events)
		{
			element.Handler = CreateMenuFlyoutSubItemHandler((n, h, l, a) => events.Add((n, a)));
		}

		MenuFlyoutSubItemHandler CreateMenuFlyoutSubItemHandler(Action<string, IMenuFlyoutSubItemHandler, IMenuFlyoutSubItem, MenuFlyoutSubItemHandlerUpdate?>? action)
		{
			var handler = new NonThrowingMenuFlyoutSubItemHandler(
				MenuFlyoutSubItemHandler.Mapper,
				new CommandMapper<IMenuFlyoutSubItem, IMenuFlyoutSubItemHandler>(MenuFlyoutSubItemHandler.CommandMapper)
				{
					[nameof(IMenuFlyoutSubItemHandler.Add)] = (h, l, a) => action?.Invoke(nameof(IMenuFlyoutSubItemHandler.Add), h, l, (MenuFlyoutSubItemHandlerUpdate?)a),
					[nameof(IMenuFlyoutSubItemHandler.Remove)] = (h, l, a) => action?.Invoke(nameof(IMenuFlyoutSubItemHandler.Remove), h, l, (MenuFlyoutSubItemHandlerUpdate?)a),
					[nameof(IMenuFlyoutSubItemHandler.Clear)] = (h, l, a) => action?.Invoke(nameof(IMenuFlyoutSubItemHandler.Clear), h, l, (MenuFlyoutSubItemHandlerUpdate?)a),
					[nameof(IMenuFlyoutSubItemHandler.Insert)] = (h, l, a) => action?.Invoke(nameof(IMenuFlyoutSubItemHandler.Insert), h, l, (MenuFlyoutSubItemHandlerUpdate?)a),
				});

			return handler;
		}

		class NonThrowingMenuFlyoutSubItemHandler : MenuFlyoutSubItemHandler
		{
			public NonThrowingMenuFlyoutSubItemHandler(IPropertyMapper mapper, CommandMapper commandMapper)
				: base(mapper, commandMapper)
			{
			}

			protected override object CreatePlatformElement() => new object();
		}
	}
}
