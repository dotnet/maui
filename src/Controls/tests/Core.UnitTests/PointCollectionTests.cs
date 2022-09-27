using Xunit;

namespace Microsoft.Maui.Controls.Shapes.UnitTests
{
	public class PointCollectionTests
	{
		PointCollectionConverter _pointCollectionConverter;


		public PointCollectionTests()
		{
			_pointCollectionConverter = new PointCollectionConverter();
		}

		[Fact]
		public void ConvertStringToPointCollectionTest()
		{
			PointCollection result = _pointCollectionConverter.ConvertFromInvariantString("0 48, 0 144, 96 150, 100 0, 192 0, 192 96, 50 96, 48 192, 150 200 144 48") as PointCollection;

			Assert.NotNull(result);
			Assert.Equal(10, result.Count);
		}
	}
}