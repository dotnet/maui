using NUnit.Framework;
using FormsRectangle = Microsoft.Maui.Controls.Shapes.Rectangle;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class RectTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();

		}

		[Test]
		public void RadiusCanBeSetFromStyle()
		{
			var rectangle = new FormsRectangle();

			Assert.AreEqual(0.0, rectangle.RadiusX);
			rectangle.SetValue(FormsRectangle.RadiusXProperty, 10.0, true);
			Assert.AreEqual(10.0, rectangle.RadiusX);

			Assert.AreEqual(0.0, rectangle.RadiusY);
			rectangle.SetValue(FormsRectangle.RadiusYProperty, 10.0, true);
			Assert.AreEqual(10.0, rectangle.RadiusY);
		}
	}
}