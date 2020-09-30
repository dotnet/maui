using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	public class DoubleCollectionTests
	{
		DoubleCollectionConverter _doubleCollectionConverter;

		[SetUp]
		public void SetUp()
		{
			_doubleCollectionConverter = new DoubleCollectionConverter();
		}

		[Test]
		public void ConvertStringToDoubleCollectionTest()
		{
			DoubleCollection result = _doubleCollectionConverter.ConvertFromInvariantString("10,110 60,10 110,110") as DoubleCollection;

			Assert.IsNotNull(result);
			Assert.AreEqual(6, result.Count);
		}
	}
}