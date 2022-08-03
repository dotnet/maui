#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Handlers;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests.Menu
{
	[TestFixture, Category("MenuBar")]
	public class MenuBarTests :
		MenuBarTestBase<MenuBar, IMenuBarItem, MenuBarItem, MenuBarHandlerUpdate>
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
