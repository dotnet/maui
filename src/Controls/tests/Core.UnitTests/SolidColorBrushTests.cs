using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class SolidColorBrushTests : BaseTestFixture
	{
		[Fact]
		public void TestConstructor()
		{
			SolidColorBrush solidColorBrush = new SolidColorBrush();
			Assert.Null(solidColorBrush.Color);
		}

		[Fact]
		public void TestConstructorUsingColor()
		{
			SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Red);
			Assert.Equal(solidColorBrush.Color, Colors.Red);
		}

		[Fact]
		public void TestEmptySolidColorBrush()
		{
			SolidColorBrush solidColorBrush = new SolidColorBrush();
			Assert.True(solidColorBrush.IsEmpty);

			SolidColorBrush red = Brush.Red;
			Assert.False(red.IsEmpty);
		}

		[Fact]
		public void TestNullOrEmptySolidColorBrush()
		{
			SolidColorBrush nullSolidColorBrush = null;
			Assert.True(Brush.IsNullOrEmpty(nullSolidColorBrush));

			SolidColorBrush emptySolidColorBrush = new SolidColorBrush();
			Assert.True(Brush.IsNullOrEmpty(emptySolidColorBrush));

			SolidColorBrush solidColorBrush = Brush.Yellow;
			Assert.False(Brush.IsNullOrEmpty(solidColorBrush));
		}

		[Fact]
		public void TestDefaultBrushes()
		{
			SolidColorBrush black = Brush.Black;
			Assert.NotNull(black.Color);
			Assert.Equal(black.Color, Colors.Black);

			SolidColorBrush white = Brush.White;
			Assert.NotNull(white.Color);
			Assert.Equal(white.Color, Colors.White);
		}

		[Fact]
		// https://github.com/dotnet/maui/issues/27281
		public void SolidColorBrushEqualsComparesColorValues()
		{
			// Create two Color instances with identical RGBA values but different object references
			// This simulates what happens with OnPlatform<Color> which creates new Color instances
			var color1 = new Color(1.0f, 0.0f, 0.0f, 1.0f);
			var color2 = new Color(1.0f, 0.0f, 0.0f, 1.0f);

			// Verify these are different instances
			Assert.False(ReferenceEquals(color1, color2));

			// Verify the Color.Equals method returns true for same values
			Assert.True(color1.Equals(color2));

			// Create SolidColorBrush instances with these colors
			var brush1 = new SolidColorBrush(color1);
			var brush2 = new SolidColorBrush(color2);

			// This is the bug from issue #27281: SolidColorBrush.Equals uses '==' for Color comparison
			// which compares references instead of values, causing infinite loops with DynamicResource
			// and OnPlatform<Color> because OnPlatform creates new Color instances each time
			Assert.True(brush1.Equals(brush2));
		}

		[Fact]
		public void TestHasTransparencySolidColorBrush()
		{
			SolidColorBrush nullBrush = null;
			Assert.False(Brush.HasTransparency(nullBrush));

			SolidColorBrush solidColorBrush = new SolidColorBrush();
			Assert.False(Brush.HasTransparency(solidColorBrush));

			SolidColorBrush red = new SolidColorBrush(Colors.Red);
			Assert.False(Brush.HasTransparency(red));

			SolidColorBrush transparentBrush = new SolidColorBrush(Colors.Transparent);
			Assert.True(Brush.HasTransparency(transparentBrush));

			SolidColorBrush semiTransparentBrush = new SolidColorBrush(Color.FromRgba(255, 0, 0, 0.5));
			Assert.True(Brush.HasTransparency(semiTransparentBrush));

			Assert.True(Brush.HasTransparency(Brush.Transparent));

			Assert.False(Brush.HasTransparency(Brush.Black));
		}
	}
}