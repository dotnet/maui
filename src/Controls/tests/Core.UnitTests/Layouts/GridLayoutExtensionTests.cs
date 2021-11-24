using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	[TestFixture]
	public class GridLayoutExtensionTests
	{
		[Test]
		public void AddViewRowColumn()
		{
			var gridLayout = new GridLayout();

			var label = new Label();

			gridLayout.Add(label, 3, 4);

			Assert.AreEqual(3, gridLayout.GetColumn(label));
			Assert.AreEqual(4, gridLayout.GetRow(label));
		}

		[Test]
		public void AddViewDefaultRow()
		{
			var gridLayout = new GridLayout();

			var label = new Label();

			gridLayout.Add(label, 3);

			Assert.AreEqual(3, gridLayout.GetColumn(label));
			Assert.AreEqual(0, gridLayout.GetRow(label));
		}

		[Test]
		public void AddViewDefaultColumn()
		{
			var gridLayout = new GridLayout();

			var label = new Label();

			gridLayout.Add(label, row: 3);

			Assert.AreEqual(0, gridLayout.GetColumn(label));
			Assert.AreEqual(3, gridLayout.GetRow(label));
		}
	}
}
