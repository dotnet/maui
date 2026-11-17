#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.UnitTests;
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

		[Fact]
		public void ShowNullTargetThrows()
		{
			Assert.Throws<ArgumentNullException>(() => FlyoutBase.ShowContextFlyout(null!));
		}

		[Fact]
		public void ShowMissingMenuThrows()
		{
			Assert.Throws<InvalidOperationException>(() => FlyoutBase.ShowContextFlyout(new Button()));
		}

		[Fact]
		public void ShowEmptyMenuThrows()
		{
			var target = new Button();
			FlyoutBase.SetContextFlyout(target, new MenuFlyout());
			Assert.Throws<InvalidOperationException>(() => FlyoutBase.ShowContextFlyout(target));
		}

		[Fact]
		public void ShowDisconnectedTargetThrows()
		{
			Assert.Throws<InvalidOperationException>(() => FlyoutBase.ShowContextFlyout(CreateTarget()));
		}

		[Theory]
		[InlineData(false, false, true)]
		[InlineData(true, true, true)]
		[InlineData(true, false, false)]
		public void ShowNoninteractiveTargetThrows(bool enabled, bool transparent, bool visible)
		{
			var target = CreateTarget();
			target.IsEnabled = enabled;
			target.InputTransparent = transparent;
			target.IsVisible = visible;
			target.Handler = new HandlerStub();
			Assert.Throws<InvalidOperationException>(() => FlyoutBase.ShowContextFlyout(target));
		}

		[Fact]
		public void ShowOnWrongThreadThrows()
		{
			bool isInvokeRequired = false;
			DispatcherProviderStubOptions.IsInvokeRequired = () => isInvokeRequired;
			try
			{
				var target = CreateTarget();
				target.Handler = new HandlerStub();
				isInvokeRequired = true;
				Assert.Throws<InvalidOperationException>(() => FlyoutBase.ShowContextFlyout(target));
			}
			finally
			{
				DispatcherProviderStubOptions.IsInvokeRequired = null;
			}
		}

		[Fact]
		public void ShowUsesTargetCommandMapper()
		{
			var target = CreateTarget();
			int requests = 0;
			var commands = new CommandMapper<IView, IViewHandler>(ViewHandler.ViewCommandMapper)
			{
				[ViewHandler.ShowContextFlyoutCommandKey] = (handler, view, args) =>
				{
					Assert.Same(target, view);
					Assert.Same(target.Handler, handler);
					Assert.Null(args);
					requests++;
				}
			};
			target.Handler = new HandlerStub(new PropertyMapper<IView>(), commands);

			FlyoutBase.ShowContextFlyout(target);

			Assert.Equal(1, requests);
		}

		[Fact]
		public void ShowOnUnsupportedPlatformThrows()
		{
			var target = CreateTarget();
			target.Handler = new HandlerStub();
			Assert.Throws<PlatformNotSupportedException>(() => FlyoutBase.ShowContextFlyout(target));
		}

		static Button CreateTarget()
		{
			var target = new Button();
			FlyoutBase.SetContextFlyout(target, new MenuFlyout
			{
				new MenuFlyoutItem { Text = "Action" }
			});
			return target;
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
