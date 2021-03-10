using Microsoft.Maui.Controls.Shapes;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PolygonTests : BaseTestFixture
	{
		PointCollectionConverter _pointCollectionConverter;

		[SetUp]
		public override void Setup()
		{
			base.Setup();

			_pointCollectionConverter = new PointCollectionConverter();
		}

		[Test]
		public void CreatePolygonFromStringPointCollectionTest()
		{
			PointCollection points = _pointCollectionConverter.ConvertFromInvariantString("0 48, 0 144, 96 150, 100 0, 192 0, 192 96, 50 96, 48 192, 150 200 144 48") as PointCollection;

			Polygon polygon = new Polygon
			{
				Points = points
			};

			Assert.IsNotNull(points);
			Assert.IsNotNull(polygon);
			Assert.AreEqual(10, points.Count);
		}
	}
}