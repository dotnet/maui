using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	[TestFixture]
	public class GridExtensionTests
	{
		[Test]
		public void AddViewRowColumn()
		{
			var grid = new Grid();

			var label = new Label();

			grid.Add(label, 3, 4);

			Assert.AreEqual(3, grid.GetColumn(label));
			Assert.AreEqual(4, grid.GetRow(label));
		}

		[Test]
		public void AddViewDefaultRow()
		{
			var grid = new Grid();

			var label = new Label();

			grid.Add(label, 3);

			Assert.AreEqual(3, grid.GetColumn(label));
			Assert.AreEqual(0, grid.GetRow(label));
		}

		[Test]
		public void AddViewDefaultColumn()
		{
			var grid = new Grid();

			var label = new Label();

			grid.Add(label, row: 3);

			Assert.AreEqual(0, grid.GetColumn(label));
			Assert.AreEqual(3, grid.GetRow(label));
		}
	}
}
