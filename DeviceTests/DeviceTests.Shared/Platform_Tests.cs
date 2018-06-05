using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    public class Platform_Tests
    {
        [Fact]
        public Task IsOnMainThread()
        {
            return Utils.OnMainThread(() =>
            {
                Assert.True(Platform.IsMainThread);
            });
        }

        [Fact]
        public Task IsNotOnMainThread()
        {
            return Task.Run(() =>
            {
                Assert.False(Platform.IsMainThread);
            });
        }
    }
}
