using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	public class MenuItemTests
		: MenuItemTests<MenuItem>
	{
	}

	[TestFixture]
	public abstract class MenuItemTests<T>
		: CommandSourceTests<T>
		where T : MenuItem, new()
	{
		[Test]
		public void Activated()
		{
			var item = new MenuItem();

			bool activated = false;
			item.Clicked += (sender, args) => activated = true;

			((IMenuItemController)item).Activate();

			Assert.That(activated, Is.True);
		}

		[Test]
		public void Command()
		{
			bool executed = false;
			var param = new object();

			var c = new Command(o =>
			{
				Assert.That(o, Is.SameAs(param));
				executed = true;
			});

			var item = new MenuItem { Command = c, CommandParameter = param };
			((IMenuItemController)item).Activate();

			Assert.That(executed, Is.True);
		}

		[Test]
		public void Accelerator()
		{
			var item = new MenuItem();
			string shourtCutKeyBinding = "ctrl+A";
			MenuItem.SetAccelerator(item, Xamarin.Forms.Accelerator.FromString(shourtCutKeyBinding));

			Assert.AreEqual(MenuItem.GetAccelerator(item).ToString(), shourtCutKeyBinding);
		}

		[Test]
		public void AcceleratorPlus()
		{
			var item = new MenuItem();
			string shourtCutKeyLikeSeparator = "+";
			MenuItem.SetAccelerator(item, Xamarin.Forms.Accelerator.FromString(shourtCutKeyLikeSeparator));

			var accelerator = MenuItem.GetAccelerator(item);
			Assert.AreEqual(accelerator.ToString(), shourtCutKeyLikeSeparator);
			Assert.AreEqual(accelerator.Keys.FirstOrDefault(), shourtCutKeyLikeSeparator);
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