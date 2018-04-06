using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    public class Clipboard_Tests
    {
        [Theory]
        [InlineData("text")]
        [InlineData("some really long test text")]
        public Task Set_Clipboard_Values(string text)
        {
            return Utils.OnMainThread(async () =>
            {
                Clipboard.SetText(text);

                await Task.Delay(100);

                Assert.True(Clipboard.HasText);
            });
        }

        [Theory]
        [InlineData("text")]
        [InlineData("some really long test text")]
        public Task Get_Clipboard_Values(string text)
        {
            return Utils.OnMainThread(async () =>
            {
                Clipboard.SetText(text);
                var clipText = await Clipboard.GetTextAsync();

                Assert.NotNull(clipText);
                Assert.Equal(text, clipText);
            });
        }
    }
}
