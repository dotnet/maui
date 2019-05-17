using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class FilePicker_Tests
    {
        [Fact]
        public async Task PickFileAsync_Fail_On_NetStandard()
        {
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => FilePicker.PickFileAsync());
        }
    }
}
