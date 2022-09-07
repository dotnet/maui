using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{

	public class GridExtensionTests
	{
		[Fact]
		public void AddViewRowColumn()
		{
			var grid = new Grid();

			var label = new Label();

			grid.Add(label, 3, 4);

			Assert.Equal(3, grid.GetColumn(label));
			Assert.Equal(4, grid.GetRow(label));
		}

		[Fact]
		public void AddViewDefaultRow()
		{
			var grid = new Grid();

			var label = new Label();

			grid.Add(label, 3);

			Assert.Equal(3, grid.GetColumn(label));
			Assert.Equal(0, grid.GetRow(label));
		}

		[Fact]
		public void AddViewDefaultColumn()
		{
			var grid = new Grid();

			var label = new Label();

			grid.Add(label, row: 3);

			Assert.Equal(0, grid.GetColumn(label));
			Assert.Equal(3, grid.GetRow(label));
		}
	}
}
