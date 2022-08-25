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

			FlyoutBase.SetContextFlyout(button, menuFlyout);

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
			var menuFlyout = new MenuFlyout();
			menuFlyout.Add(subMenuFlyout);
			FlyoutBase.SetContextFlyout(button, menuFlyout);

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
			var menuFlyout = new MenuFlyout();
			menuFlyout.Add(subMenuFlyout);
			FlyoutBase.SetContextFlyout(button, menuFlyout);

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
			element.Handler = CreateMenuFlyoutHandler((n, h, l, a) => events.Add((n, a)));
		}

		MenuFlyoutHandler CreateMenuFlyoutHandler(Action<string, IMenuFlyoutHandler, IMenuFlyout, ContextFlyoutItemHandlerUpdate?>? action)
		{
			var handler = new NonThrowingMenuFlyoutHandler(
				MenuFlyoutHandler.Mapper,
				new CommandMapper<IMenuFlyout, IMenuFlyoutHandler>(MenuFlyoutHandler.CommandMapper)
				{
					[nameof(IMenuFlyoutHandler.Add)] = (h, l, a) => action?.Invoke(nameof(IMenuFlyoutHandler.Add), h, l, (ContextFlyoutItemHandlerUpdate?)a),
					[nameof(IMenuFlyoutHandler.Remove)] = (h, l, a) => action?.Invoke(nameof(IMenuFlyoutHandler.Remove), h, l, (ContextFlyoutItemHandlerUpdate?)a),
					[nameof(IMenuFlyoutHandler.Clear)] = (h, l, a) => action?.Invoke(nameof(IMenuFlyoutHandler.Clear), h, l, (ContextFlyoutItemHandlerUpdate?)a),
					[nameof(IMenuFlyoutHandler.Insert)] = (h, l, a) => action?.Invoke(nameof(IMenuFlyoutHandler.Insert), h, l, (ContextFlyoutItemHandlerUpdate?)a),
				});

			return handler;
		}

		class NonThrowingMenuFlyoutHandler : MenuFlyoutHandler
		{
			public NonThrowingMenuFlyoutHandler(IPropertyMapper mapper, CommandMapper commandMapper)
				: base(mapper, commandMapper)
			{
			}

			protected override object CreatePlatformElement() => new object();
		}
	}
}
