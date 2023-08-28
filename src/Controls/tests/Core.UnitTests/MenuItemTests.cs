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
	}
}
