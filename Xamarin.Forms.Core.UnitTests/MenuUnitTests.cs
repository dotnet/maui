using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class MenuUnitTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
			Application.Current = new MockApplication();
		}

		[TearDown]
		public override void TearDown()
		{
			Application.Current = null;
		}

		[Test]
		public void SetMenuOnMenuItem()
		{
			var item = new MenuItem();
			var menu = new Menu { Text = "Hello" };
			MenuItem.SetMenu(item, menu);

			Assert.AreEqual(menu, MenuItem.GetMenu(item));
		}

		[Test]
		public void AddSubMenuOnMenu()
		{
			var item = new MenuItem();
			var menu = new Menu { Text = "Hello" };
			var submenu = new Menu { Text = "SubMenu Hello" };
			menu.Add(submenu);

			MenuItem.SetMenu(item, menu);

			Assert.AreEqual(MenuItem.GetMenu(item), menu);
			Assert.AreEqual(MenuItem.GetMenu(item)[0], submenu);
			Assert.AreEqual(MenuItem.GetMenu(item)[0].Text, submenu.Text);
		}

		[Test]
		public void SetMenuOnApplicationMainMenu()
		{
			var item = new MenuItem();
			var menu = new Menu { Text = "Hello" };
			Element.SetMenu(Application.Current, menu);
			Assert.GreaterOrEqual(1, Element.GetMenu(Application.Current).Count);
		}

		[Test]
		public void MenuText()
		{
			string text = "hello";
			var menu = new Menu { Text = text };

			Assert.AreEqual(text, menu.Text);
		}

		[Test]
		public void MenuInvalidateFiresPropertyChanged()
		{
			string text = "hello";
			int count = 0;
			var menu = new Menu { Text = text };

			menu.PropertyChanged += (s, e) =>
			{
				count = count + 1;
			};

			menu.Invalidate();

			Assert.AreEqual(1, count);
		}

		[Test]
		public void MenuInvalidateWorksOnAdd()
		{
			string text = "hello";
			int count = 0;
			var menu = new Menu { Text = text };

			menu.PropertyChanged += (s, e) =>
			{
				count = count + 1;
			};

			menu.Add(new Menu());

			Assert.AreEqual(1, count);
		}

		[Test]
		public void MenuInvalidateWorksOnClear()
		{
			string text = "hello";
			int count = 0;
			var menu = new Menu { Text = text };

			menu.PropertyChanged += (s, e) =>
			{
				count = count + 1;
			};

			menu.Add(new Menu());
			menu.Clear();

			Assert.AreEqual(2, count);
		}

		[Test]
		public void MenuInvalidateWorksOnInsertAndRemove()
		{
			string text = "hello";
			int count = 0;
			var menu = new Menu { Text = text };

			menu.PropertyChanged += (s, e) =>
			{
				count = count + 1;
			};

			menu.Insert(0, new Menu());

			Assert.AreEqual(1, count);

			menu.RemoveAt(0);

			Assert.AreEqual(2, count);
		}


		[Test]
		public void MenuFiresPropertyChangedOnAddItems()
		{
			string text = "hello";
			int count = 0;
			var menu = new Menu { Text = text };

			menu.PropertyChanged += (s, e) =>
			{
				count = count + 1;
			};

			menu.Items.Add(new MenuItem());
			Assert.AreEqual(1, count);
		}

	}
}