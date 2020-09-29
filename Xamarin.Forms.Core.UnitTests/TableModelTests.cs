using System;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class TableModelTests : BaseTestFixture
	{
		class TestModel : TableModel
		{
			public override int GetRowCount(int section)
			{
				return 10;
			}

			public override int GetSectionCount()
			{
				return 1;
			}

			public override object GetItem(int section, int row)
			{
				return "Foo";
			}

			public string ProtectedSectionTitle()
			{
				return GetSectionTitle(0);
			}
		}

		[Test]
		public void DefaultSectionTitle()
		{
			Assert.IsNull(new TestModel().ProtectedSectionTitle());
		}

		[Test]
		public void DefualtSectionIndexTitles()
		{
			Assert.IsNull(new TestModel().GetSectionIndexTitles());
		}

		[Test]
		public void DefaultHeaderCell()
		{
			Assert.IsNull(new TestModel().GetHeaderCell(0));
		}

		[Test]
		public void DefaultCellFromObject()
		{
			var model = new TestModel();
			var cell = model.GetCell(0, 5);

			Assert.That(cell, Is.TypeOf<TextCell>());

			var textCell = (TextCell)cell;
			Assert.AreEqual("Foo", textCell.Text);
		}

		[Test]
		public void RowLongPressed()
		{
			var model = new TestModel();

			var longPressedItem = "";
			model.ItemLongPressed += (sender, arg) =>
			{
				longPressedItem = (string)arg.Data;
			};

			model.RowLongPressed(0, 5);
		}

		[Test]
		public void RowSelectedForObject()
		{
			var model = new TestModel();
			string result = null;
			model.ItemSelected += (sender, arg) => result = (string)arg.Data;

			model.RowSelected("Foobar");
			Assert.AreEqual("Foobar", result);
		}
	}
}