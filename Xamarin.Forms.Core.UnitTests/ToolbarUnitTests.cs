using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ToolbarUnitTests : BaseTestFixture
	{
		[Test]
		public void TestConstructor ()
		{
			Toolbar toolbar = new Toolbar ();

			Assert.That (toolbar.Items, Is.Empty);
		}

		[Test]
		public void TestAdd ()
		{
			Toolbar toolbar = new Toolbar ();

			ToolbarItem item = new ToolbarItem ("Foo", "Bar.jpg", () => {});
			toolbar.Add (item);

			Assert.AreEqual (item, toolbar.Items [0]);
		}

		[Test]
		public void TestRemove ()
		{
			Toolbar toolbar = new Toolbar ();
			
			ToolbarItem item = new ToolbarItem ("Foo", "Bar.jpg", () => {});
			ToolbarItem item2 = new ToolbarItem ("Foo", "Bar.jpg", () => {});
			toolbar.Add (item);
			toolbar.Add (item2);

			toolbar.Remove (item);

			Assert.AreEqual (item2, toolbar.Items [0]);
		}

		[Test]
		public void TestItemAdded ()
		{
			Toolbar toolbar = new Toolbar ();
			
			ToolbarItem item = new ToolbarItem ("Foo", "Bar.jpg", () => {});

			bool added = false;
			toolbar.ItemAdded += (sender, e) => added = true;

			toolbar.Add (item);
			
			Assert.True (added);

			added = false;
			toolbar.Add (item);

			Assert.False (added);
		}

		[Test]
		public void TestItemRemoved ()
		{
			Toolbar toolbar = new Toolbar ();
			
			ToolbarItem item = new ToolbarItem ("Foo", "Bar.jpg", () => {});
			toolbar.Add (item);
			
			bool removed = false;
			toolbar.ItemRemoved += (sender, e) => removed = true;

			toolbar.Remove (item);
			
			Assert.True (removed);
		}

		[Test]
		public void TestClear ()
		{
			var toolbar = new Toolbar ();
			
			var item = new ToolbarItem ("Foo", "Bar.jpg", () => {});
			var item2 = new ToolbarItem ("Foo", "Bar.jpg", () => {});

			toolbar.Add (item);
			toolbar.Add (item2);

			toolbar.Clear ();

			Assert.That (toolbar.Items, Is.Empty);
		}

		[Test]
		public void ToolBarItemAddedEventArgs ()
		{
			var toolbar = new Toolbar ();

			var item = new ToolbarItem ("Foo", "Bar.jpg", () => { });
			var item2 = new ToolbarItem ("Foo", "Bar.jpg", () => { });

			ToolbarItem itemArg = null;

			toolbar.ItemAdded += (s, e) => {
				itemArg = e.ToolbarItem;
			};

			toolbar.Add (item);
			Assert.AreEqual (item, itemArg);

			toolbar.Add (item2);
			Assert.AreEqual (item2, itemArg);
		}

		[Test]
		public void ToolBarItemRemovedEventArgs ()
		{
			var toolbar = new Toolbar ();

			var item = new ToolbarItem ("Foo", "Bar.jpg", () => { });
			var item2 = new ToolbarItem ("Foo", "Bar.jpg", () => { });

			toolbar.Add (item);
			toolbar.Add (item2);

			ToolbarItem itemArg = null;

			toolbar.ItemRemoved += (s, e) => {
				itemArg = e.ToolbarItem;
			};

			toolbar.Remove (item);
			Assert.AreEqual (item, itemArg);

			toolbar.Remove (item2);
			Assert.AreEqual (item2, itemArg);
		}
	}
	
}
