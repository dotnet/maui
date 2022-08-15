#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Menu
{
	[Category("MenuFlyout")]
	public class ContextFlyoutTests :
		MenuTestBase<MenuFlyout, IMenuElement, MenuFlyoutItem, ContextFlyoutItemHandlerUpdate>
	{
		[Fact]
		public void BindingContextPropagatesWhenContextFlyoutIsAlreadySetOnParent()
		{
			Button button = new Button();
			var subMenuFlyout = new MenuFlyoutSubItem();
			var menuFlyoutItem = new MenuFlyoutItem();
			var menuFlyout = new MenuFlyout();
			menuFlyout.Add(subMenuFlyout);
			subMenuFlyout.Add(menuFlyoutItem);

			button.ContextFlyout = menuFlyout;

			var bc = new Object();
			button.BindingContext = bc;

			Assert.Same(bc, subMenuFlyout.BindingContext);
			Assert.Same(bc, menuFlyout.BindingContext);
		}

		[Fact]
		public void BindingContextPropagatesWhenContextFlyoutIsSetAfterParentBindingContextIsSet()
		{
			Button button = new Button();
			var subMenuFlyout = new MenuFlyoutSubItem();
			var menuFlyoutItem = new MenuFlyoutItem();
			subMenuFlyout.Add(menuFlyoutItem);

			var bc = new Object();
			button.BindingContext = bc;
			button.MenuFlyout.Add(subMenuFlyout);

			Assert.Same(bc, subMenuFlyout.BindingContext);
			Assert.Same(bc, menuFlyout.BindingContext);
		}

		[Fact]
		public void BindingContextPropagatesToAddedFlyoutItems()
		{
			Button button = new Button();
			var subMenuFlyout = new MenuFlyoutSubItem();
			var menuFlyoutItem = new MenuFlyoutItem();

			var bc = new Object();
			button.BindingContext = bc;
			button.MenuFlyout.Add(subMenuFlyout);

			// Add submenu after MenuFlyout is already set on Button
			subMenuFlyout.Add(menuFlyoutItem);

			Assert.Same(bc, subMenuFlyout.BindingContext);
			Assert.Same(bc, menuFlyoutItem.BindingContext);
		}

		protected override int GetIndex(ContextFlyoutItemHandlerUpdate handlerUpdate)
			=> handlerUpdate.Index;

		protected override IMenuElement GetItem(ContextFlyoutItemHandlerUpdate handlerUpdate) =>
			handlerUpdate.MenuElement;

		protected override void SetHandler(IElement element, List<(string Name, ContextFlyoutItemHandlerUpdate? Args)> events)
		{
			element.Handler = CreateContextFlyoutHandler((n, h, l, a) => events.Add((n, a)));
		}

		ContextFlyoutHandler CreateContextFlyoutHandler(Action<string, IMenuFlyoutHandler, IMenuFlyout, ContextFlyoutItemHandlerUpdate?>? action)
		{
			var handler = new NonThrowingContextFlyoutHandler(
				ContextFlyoutHandler.Mapper,
				new CommandMapper<IMenuFlyout, IMenuFlyoutHandler>(ContextFlyoutHandler.CommandMapper)
				{
					[nameof(IMenuFlyoutHandler.Add)] = (h, l, a) => action?.Invoke(nameof(IMenuFlyoutHandler.Add), h, l, (ContextFlyoutItemHandlerUpdate?)a),
					[nameof(IMenuFlyoutHandler.Remove)] = (h, l, a) => action?.Invoke(nameof(IMenuFlyoutHandler.Remove), h, l, (ContextFlyoutItemHandlerUpdate?)a),
					[nameof(IMenuFlyoutHandler.Clear)] = (h, l, a) => action?.Invoke(nameof(IMenuFlyoutHandler.Clear), h, l, (ContextFlyoutItemHandlerUpdate?)a),
					[nameof(IMenuFlyoutHandler.Insert)] = (h, l, a) => action?.Invoke(nameof(IMenuFlyoutHandler.Insert), h, l, (ContextFlyoutItemHandlerUpdate?)a),
				});

			return handler;
		}

		class NonThrowingContextFlyoutHandler : ContextFlyoutHandler
		{
			public NonThrowingContextFlyoutHandler(IPropertyMapper mapper, CommandMapper commandMapper)
				: base(mapper, commandMapper)
			{
			}

			protected override object CreatePlatformElement() => new object();
		}
	}
}
