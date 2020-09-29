using NUnit.Framework;


namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class TypeUnitTests : BaseTestFixture
	{
		[Test]
		public void TestVec2()
		{
			var vec2 = new Vec2();

			Assert.AreEqual(0, vec2.X);
			Assert.AreEqual(0, vec2.Y);

			vec2 = new Vec2(2, 3);

			Assert.AreEqual(2, vec2.X);
			Assert.AreEqual(3, vec2.Y);
		}
	}
}