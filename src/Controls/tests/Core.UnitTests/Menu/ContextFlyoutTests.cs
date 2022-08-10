#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Menu
{
	[Category("ContextFlyout")]
	public class ContextFlyoutTests :
		MenuTestBase<ContextFlyout, IMenuElement, MenuFlyoutItem, ContextFlyoutItemHandlerUpdate>
	{
		[Fact]
		public void BindingContextPropagatesWhenContextFlyoutIsAlreadySetOnParent()
		{
			Button button = new Button();
			var subMenuFlyout = new MenuFlyoutSubItem();
			var menuFlyout = new MenuFlyoutItem();
			subMenuFlyout.Add(menuFlyout);
			button.ContextFlyout.Add(subMenuFlyout);

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
			var menuFlyout = new MenuFlyoutItem();
			subMenuFlyout.Add(menuFlyout);

			var bc = new Object();
			button.BindingContext = bc;
			button.ContextFlyout.Add(subMenuFlyout);

			Assert.Same(bc, subMenuFlyout.BindingContext);
			Assert.Same(bc, menuFlyout.BindingContext);
		}

		[Fact]
		public void BindingContextPropagatesToAddedFlyoutItems()
		{
			Button button = new Button();
			var subMenuFlyout = new MenuFlyoutSubItem();
			var menuFlyout = new MenuFlyoutItem();

			var bc = new Object();
			button.BindingContext = bc;
			button.ContextFlyout.Add(subMenuFlyout);

			// Add submenu after ContextFlyout is already set on Button
			subMenuFlyout.Add(menuFlyout);

			Assert.Same(bc, subMenuFlyout.BindingContext);
			Assert.Same(bc, menuFlyout.BindingContext);
		}

		protected override int GetIndex(ContextFlyoutItemHandlerUpdate handlerUpdate)
			=> handlerUpdate.Index;

		protected override IMenuElement GetItem(ContextFlyoutItemHandlerUpdate handlerUpdate) =>
			handlerUpdate.MenuElement;

		protected override void SetHandler(IElement element, List<(string Name, ContextFlyoutItemHandlerUpdate? Args)> events)
		{
			element.Handler = CreateContextFlyoutHandler((n, h, l, a) => events.Add((n, a)));
		}

		ContextFlyoutHandler CreateContextFlyoutHandler(Action<string, IContextFlyoutHandler, IContextFlyout, ContextFlyoutItemHandlerUpdate?>? action)
		{
			var handler = new NonThrowingContextFlyoutHandler(
				ContextFlyoutHandler.Mapper,
				new CommandMapper<IContextFlyout, IContextFlyoutHandler>(ContextFlyoutHandler.CommandMapper)
				{
					[nameof(IContextFlyoutHandler.Add)] = (h, l, a) => action?.Invoke(nameof(IContextFlyoutHandler.Add), h, l, (ContextFlyoutItemHandlerUpdate?)a),
					[nameof(IContextFlyoutHandler.Remove)] = (h, l, a) => action?.Invoke(nameof(IContextFlyoutHandler.Remove), h, l, (ContextFlyoutItemHandlerUpdate?)a),
					[nameof(IContextFlyoutHandler.Clear)] = (h, l, a) => action?.Invoke(nameof(IContextFlyoutHandler.Clear), h, l, (ContextFlyoutItemHandlerUpdate?)a),
					[nameof(IContextFlyoutHandler.Insert)] = (h, l, a) => action?.Invoke(nameof(IContextFlyoutHandler.Insert), h, l, (ContextFlyoutItemHandlerUpdate?)a),
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
