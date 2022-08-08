using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class LayoutOptionsUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestTypeConverter()
		{
			var converter = new LayoutOptionsConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));
			Assert.Equal(LayoutOptions.Center, converter.ConvertFromInvariantString("LayoutOptions.Center"));
			Assert.Equal(LayoutOptions.Center, converter.ConvertFromInvariantString("Center"));
			Assert.NotEqual(LayoutOptions.CenterAndExpand, converter.ConvertFromInvariantString("Center"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("foo"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("foo.bar"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString("foo.bar.baz"));
		}
	}
}