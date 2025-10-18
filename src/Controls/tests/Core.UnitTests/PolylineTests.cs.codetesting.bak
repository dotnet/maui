using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PolylineTests : BaseTestFixture
	{
		PointCollectionConverter _pointCollectionConverter;


		public PolylineTests()
		{


			_pointCollectionConverter = new PointCollectionConverter();
		}

		[Fact]
		public void CreatePolylineFromStringPointCollectionTest()
		{
			PointCollection points = _pointCollectionConverter.ConvertFromInvariantString("0 48, 0 144, 96 150, 100 0, 192 0, 192 96, 50 96, 48 192, 150 200 144 48") as PointCollection;

			Polyline polyline = new Polyline
			{
				Points = points
			};

			Assert.NotNull(points);
			Assert.NotNull(polyline);
			Assert.Equal(10, points.Count);
		}
	}
}