using NUnit.Framework;

namespace Xamarin.Forms.Shapes.UnitTests
{
	public class PointCollectionTests
	{
		PointCollectionConverter _pointCollectionConverter;

		[SetUp]
		public void SetUp()
		{
			_pointCollectionConverter = new PointCollectionConverter();
		}

		[Test]
		public void ConvertStringToPointCollectionTest()
		{
			PointCollection result = _pointCollectionConverter.ConvertFromInvariantString("0 48, 0 144, 96 150, 100 0, 192 0, 192 96, 50 96, 48 192, 150 200 144 48") as PointCollection;

			Assert.IsNotNull(result);
			Assert.AreEqual(10, result.Count);
		}
	}
}