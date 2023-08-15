using Xunit;
using FormsRectangle = Microsoft.Maui.Controls.Shapes.Rectangle;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class RectTests : BaseTestFixture
	{
		[Fact]
		public void RadiusCanBeSetFromStyle()
		{
			var rectangle = new FormsRectangle();

			Assert.Equal(0.0, rectangle.RadiusX);
			rectangle.SetValue(FormsRectangle.RadiusXProperty, 10.0, new SetterSpecificity(SetterSpecificity.StyleImplicit, 0, 0, 0));
			Assert.Equal(10.0, rectangle.RadiusX);

			Assert.Equal(0.0, rectangle.RadiusY);
			rectangle.SetValue(FormsRectangle.RadiusYProperty, 10.0, new SetterSpecificity(SetterSpecificity.StyleImplicit, 0, 0, 0));
			Assert.Equal(10.0, rectangle.RadiusY);
		}
	}
}