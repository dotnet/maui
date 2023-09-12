using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class LineTests : BaseTestFixture
	{
		[Fact]
		public void XPointCanBeSetFromStyle()
		{
			var line = new Line();

			Assert.Equal(0.0, line.X1);
			line.SetValue(Line.X1Property, 1.0, new SetterSpecificity(SetterSpecificity.StyleImplicit, 0, 0, 0));
			Assert.Equal(1.0, line.X1);

			Assert.Equal(0.0, line.X2);
			line.SetValue(Line.X2Property, 100.0, new SetterSpecificity(SetterSpecificity.StyleImplicit, 0, 0, 0));
			Assert.Equal(100.0, line.X2);
		}

		[Fact]
		public void YPointCanBeSetFromStyle()
		{
			var line = new Line();

			Assert.Equal(0.0, line.Y1);
			line.SetValue(Line.Y1Property, 1.0, new SetterSpecificity(SetterSpecificity.StyleImplicit, 0, 0, 0));
			Assert.Equal(1.0, line.Y1);

			Assert.Equal(0.0, line.Y2);
			line.SetValue(Line.Y2Property, 10.0, new SetterSpecificity(SetterSpecificity.StyleImplicit, 0, 0, 0));
			Assert.Equal(10.0, line.Y2);
		}
	}
}