using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class TableRootUnitTests : BaseTestFixture
	{
		[Test]
		public void Ctor()
		{
			const string title = "FooBar";
			var model = new TableRoot(title);
			Assert.AreEqual(title, model.Title);
		}

		[Test]
		public void CtorInvalid()
		{
			Assert.Throws<ArgumentNullException>(() => new TableRoot(null));
		}

		[Test]
		public void TestGetSections()
		{
			var model = new TableRoot("Name") {
				new TableSection ("Section 1") {
					new TextCell { Text = "Item 1.1", Detail = "Hint 1"},
					new TextCell { Text = "Item 1.2", Detail = "Hint 2"},
					new TextCell { Text = "Item 1.3", Detail = "Hint 3"}
				},
				new TableSection ("Section 2") {
					new TextCell { Text = "Item 2.1", Detail = "Hint 1"},
					new TextCell { Text = "Item 2.2", Detail = "Hint 2"},
					new TextCell { Text = "Item 2.3", Detail = "Hint 3"}
				}
			};

			Assert.AreEqual(2, model.Count);
		}

		[Test]
		public void TestCollectionChanged()
		{
			var model = new TableRoot();

			bool changed = false;
			model.CollectionChanged += (sender, e) => changed = true;

			model.Add(new TableSection("Foo"));

			Assert.True(changed);

			changed = false;

			model[0].Add(new TextCell { Text = "Foobar" });

			// Our tree is not supposed to track up like this
			Assert.False(changed);
		}

		[Test]
		public void TestTree()
		{
			var model = new TableRoot("Name") {
				new TableSection ("Section 1") {
					new TextCell { Text = "Item 1.1", Detail = "Hint 1"},
					new TextCell { Text = "Item 1.2", Detail = "Hint 2"},
					new TextCell { Text = "Item 1.3", Detail = "Hint 3"}
				},
				new TableSection {
					new TextCell { Text = "Item 2.1", Detail = "Hint 1"},
					new TextCell { Text = "Item 2.2", Detail = "Hint 2"},
					new TextCell { Text = "Item 2.3", Detail = "Hint 3"}
				}
			};

			Assert.AreEqual("Item 1.1", (model[0][0] as TextCell).Text);
			Assert.AreEqual("Item 1.2", (model[0][1] as TextCell).Text);
			Assert.AreEqual("Item 1.3", (model[0][2] as TextCell).Text);
			Assert.AreEqual("Item 2.1", (model[1][0] as TextCell).Text);
			Assert.AreEqual("Item 2.2", (model[1][1] as TextCell).Text);
			Assert.AreEqual("Item 2.3", (model[1][2] as TextCell).Text);
		}

		//[Test]
		//public void TestAddFromEnumerable ()
		//{;
		//	TableSection first, second;
		//	var model = new TableRoot ();
		//	model.Add (new[] {
		//		first = new TableSection (),
		//		second = new TableSection ()
		//	});

		//	Assert.AreEqual (first, model[0]);
		//	Assert.AreEqual (second, model[1]);
		//}
	}
}