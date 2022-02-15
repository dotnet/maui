using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
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
		public void MenuInvalidateFiresPropertyChanged()
		{
			string text = "hello";
			int count = 0;
			var menu = new TestMenu();

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
			var menu = new TestMenu();

			menu.PropertyChanged += (s, e) =>
			{
				count = count + 1;
			};

			menu.Add(new MenuItem());

			Assert.AreEqual(1, count);
		}

		[Test]
		public void MenuInvalidateWorksOnClear()
		{
			string text = "hello";
			int count = 0;
			var menu = new TestMenu();

			menu.PropertyChanged += (s, e) =>
			{
				count = count + 1;
			};

			menu.Add(new MenuItem());
			menu.Clear();

			Assert.AreEqual(2, count);
		}

		[Test]
		public void MenuInvalidateWorksOnInsertAndRemove()
		{
			string text = "hello";
			int count = 0;
			var menu = new TestMenu();

			menu.PropertyChanged += (s, e) =>
			{
				count = count + 1;
			};

			menu.Insert(0, new MenuItem());

			Assert.AreEqual(1, count);

			menu.RemoveAt(0);

			Assert.AreEqual(2, count);
		}

		class TestMenu : Menu<MenuItem>
		{

		}
	}
}
