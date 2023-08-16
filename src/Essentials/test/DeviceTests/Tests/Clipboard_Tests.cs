using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("Clipboard")]
	public class Clipboard_Tests
	{
		[UITheory]
		[InlineData("text")]
		[InlineData("some really long test text")]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public async Task Set_Clipboard_Values(string text)
		{
				await Clipboard.SetTextAsync(text);
				Assert.True(Clipboard.HasText);
		}

		[UITheory]
		[InlineData("text")]
		[InlineData("some really long test text")]
		[Trait(Traits.UI, Traits.FeatureSupport.Supported)]
		public async Task Get_Clipboard_Values(string text)
		{
				await Clipboard.SetTextAsync(text);
				var clipText = await Clipboard.GetTextAsync();

				Assert.NotNull(clipText);
				Assert.Equal(text, clipText);
		}
	}
}
