using System;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

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

		[Fact]
		public void DefaultSectionTitle()
		{
			Assert.Null(new TestModel().ProtectedSectionTitle());
		}

		[Fact]
		public void DefualtSectionIndexTitles()
		{
			Assert.Null(new TestModel().GetSectionIndexTitles());
		}

		[Fact]
		public void DefaultHeaderCell()
		{
			Assert.Null(new TestModel().GetHeaderCell(0));
		}

		[Fact]
		public void DefaultCellFromObject()
		{
			var model = new TestModel();
			var cell = model.GetCell(0, 5);

			Assert.IsType<TextCell>(cell);

			var textCell = (TextCell)cell;
			Assert.Equal("Foo", textCell.Text);
		}

		[Fact]
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

		[Fact]
		public void RowSelectedForObject()
		{
			var model = new TestModel();
			string result = null;
			model.ItemSelected += (sender, arg) => result = (string)arg.Data;

			model.RowSelected("Foobar");
			Assert.Equal("Foobar", result);
		}
	}
}
