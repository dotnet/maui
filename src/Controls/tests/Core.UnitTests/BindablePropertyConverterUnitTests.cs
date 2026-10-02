using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class BindablePropertyConverterUnitTests : BaseTestFixture
	{
		[Theory]
		[InlineData("")]
		[InlineData("Label")]
		[InlineData("Label.Text.More")]
		[InlineData("prefix:Label.Text")]
		[InlineData("NotAType.Text")]
		public void InvalidValuesReturnNull(string value)
		{
			var converter = new BindablePropertyConverter();

			Assert.Null(converter.ConvertFromInvariantString(value));
		}
	}
}
