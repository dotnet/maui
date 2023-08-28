using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class KeyboardAcceleratorTypeConverterUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestKeyboardAcceleratorTypeConverter()
		{
			var converter = new AcceleratorTypeConverter();
			string shortCutKeyBinding = "ctrl+A";
			Assert.Equal(KeyboardAccelerator.FromString(shortCutKeyBinding), (KeyboardAccelerator)converter.ConvertFromInvariantString(shortCutKeyBinding));
		}
	}
}
