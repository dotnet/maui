using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace Tests
{
    public class FilePicker_Tests
    {
        [Fact]
        public async Task PickAsync_Fail_On_NetStandard()
        {
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => FilePicker.PickAsync());
        }

        [Fact]
        public async Task PickMultipleAsync_Fail_On_NetStandard()
        {
            await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => FilePicker.PickMultipleAsync());
        }
    }
}
