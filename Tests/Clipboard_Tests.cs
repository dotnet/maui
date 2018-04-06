using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class Clipboard_Tests
    {
        [Fact]
        public void Clipboard_SetText_Fail_On_NetStandard()
        {
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Clipboard.SetText("Text"));
        }

        [Fact]
        public void Clipboard_HasText_Fail_On_NetStandard()
        {
            Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Clipboard.HasText);
        }

        [Fact]
        public async Task Clipboard_GetText_Fail_On_NetStandard()
        {
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Clipboard.GetTextAsync());
        }
    }
}
