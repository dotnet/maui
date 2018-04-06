using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class DataTransfer_Tests
    {
        [Fact]
        public async Task Request_Text_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => DataTransfer.RequestAsync("Text"));

        [Fact]
        public async Task Request_Text_Title_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => DataTransfer.RequestAsync("Text", "Title"));

        [Fact]
        public async Task Request_Text_Request_NetStandard() =>
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => DataTransfer.RequestAsync(new ShareTextRequest()));
    }
}
