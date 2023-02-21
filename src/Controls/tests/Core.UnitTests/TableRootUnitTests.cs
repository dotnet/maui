using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class TableRootUnitTests : BaseTestFixture
	{
		[Fact]
		public void Ctor()
		{
			const string title = "FooBar";
			var model = new TableRoot(title);
			Assert.Equal(title, model.Title);
		}

		[Fact]
		public void CtorInvalid()
		{
			Assert.Throws<ArgumentNullException>(() => new TableRoot(null));
		}

		[Fact]
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

			Assert.Equal(2, model.Count);
		}

		[Fact]
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

		[Fact]
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

			Assert.Equal("Item 1.1", (model[0][0] as TextCell).Text);
			Assert.Equal("Item 1.2", (model[0][1] as TextCell).Text);
			Assert.Equal("Item 1.3", (model[0][2] as TextCell).Text);
			Assert.Equal("Item 2.1", (model[1][0] as TextCell).Text);
			Assert.Equal("Item 2.2", (model[1][1] as TextCell).Text);
			Assert.Equal("Item 2.3", (model[1][2] as TextCell).Text);
		}

		//[Fact]
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
