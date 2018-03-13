using System.Threading.Tasks;
using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.Tests
{
    public class Clipboard_Tests
    {
        [Fact]
        public void Clipboard_SetText_Fail_On_NetStandard()
        {
            Assert.Throws<NotImplentedInReferenceAssembly>(() => Clipboard.SetText("Text"));
        }

        [Fact]
        public void Clipboard_HasText_Fail_On_NetStandard()
        {
            Assert.Throws<NotImplentedInReferenceAssembly>(() => Clipboard.HasText);
        }

        [Fact]
        public async Task Clipboard_GetText_Fail_On_NetStandard()
        {
            await Assert.ThrowsAsync<NotImplentedInReferenceAssembly>(() => Clipboard.GetTextAsync());
        }
    }
}
