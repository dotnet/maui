using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    public class ScreenLock_Tests
    {
        [Fact]
        public Task ScreenLock_Locks()
        {
            return Utils.OnMainThread(() =>
            {
                Assert.False(ScreenLock.IsActive);

                ScreenLock.RequestActive();
                Assert.True(ScreenLock.IsActive);

                ScreenLock.RequestRelease();
                Assert.False(ScreenLock.IsActive);
            });
        }

        [Fact]
        public Task ScreenLock_Unlocks_Without_Locking()
        {
            return Utils.OnMainThread(() =>
            {
                Assert.False(ScreenLock.IsActive);

                ScreenLock.RequestRelease();
                Assert.False(ScreenLock.IsActive);
            });
        }

        [Fact]
        public Task ScreenLock_Locks_Only_Once()
        {
            return Utils.OnMainThread(() =>
            {
                Assert.False(ScreenLock.IsActive);

                ScreenLock.RequestActive();
                Assert.True(ScreenLock.IsActive);
                ScreenLock.RequestActive();
                Assert.True(ScreenLock.IsActive);

                ScreenLock.RequestRelease();
                Assert.False(ScreenLock.IsActive);
            });
        }
    }
}
