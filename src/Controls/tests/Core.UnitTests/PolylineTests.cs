using Microsoft.Maui.Controls.Shapes;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PolylineTests : BaseTestFixture
	{
		PointCollectionConverter _pointCollectionConverter;

		[SetUp]
		public override void Setup()
		{
			base.Setup();

			_pointCollectionConverter = new PointCollectionConverter();
		}

		[Test]
		public void CreatePolylineFromStringPointCollectionTest()
		{
			PointCollection points = _pointCollectionConverter.ConvertFromInvariantString("0 48, 0 144, 96 150, 100 0, 192 0, 192 96, 50 96, 48 192, 150 200 144 48") as PointCollection;

			Polyline polyline = new Polyline
			{
				Points = points
			};

			Assert.IsNotNull(points);
			Assert.IsNotNull(polyline);
			Assert.AreEqual(10, points.Count);
		}
	}
}