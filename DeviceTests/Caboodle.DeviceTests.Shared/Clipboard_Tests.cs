using System.Threading.Tasks;
using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.DeviceTests
{
    public class Clipboard_Tests
    {
        [Theory]
        [InlineData("text")]
        public void Set_Clipboard_Values(string text)
        {
            Clipboard.SetText(text);

            Assert.True(Clipboard.HasText);
        }

        [Theory]
        [InlineData("text")]
        public async Task Get_Clipboard_Values(string text)
        {
            Clipboard.SetText(text);
            var clipText = await Clipboard.GetTextAsync();

            Assert.NotNull(clipText);
            Assert.Equal(text, clipText);
        }
    }
}
