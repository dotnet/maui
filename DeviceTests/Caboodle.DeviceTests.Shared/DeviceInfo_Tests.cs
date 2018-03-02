using System;
using System.Threading.Tasks;
using Microsoft.Caboodle;
using Xunit;

namespace Caboodle.DeviceTests
{
    public class DeviceInfo_Tests
    {
        [Fact]
        public void Versions_Are_Correct()
        {
#if WINDOWS_UWP
            Assert.Equal(10, DeviceInfo.Version.Major);
            Assert.Equal(0, DeviceInfo.Version.Minor);
            Assert.StartsWith("10.0", DeviceInfo.VersionString);
#else
            Assert.True(DeviceInfo.Version.Major > 0);
#endif
        }

        [Fact]
        public void Identifier_Is_Not_Null()
        {
            Assert.NotNull(DeviceInfo.Identifier);
            Assert.NotEmpty(DeviceInfo.Identifier);
        }

        [Fact]
        public void Platform_Is_Correct()
        {
#if WINDOWS_UWP
            Assert.Equal(DeviceInfo.Platforms.UWP, DeviceInfo.Platform);
#elif __IOS__
            Assert.Equal(DeviceInfo.Platforms.iOS, DeviceInfo.Platform);
#elif __ANDROID__
            Assert.Equal(DeviceInfo.Platforms.Android, DeviceInfo.Platform);
#else
            throw new PlatformNotSupportedException();
#endif
        }

        [Fact]
        public void App_Build_Is_Correct()
        {
            Assert.Equal("1", DeviceInfo.AppBuild);
        }

        [Fact]
        public void App_Versions_Are_Correct()
        {
            Assert.Equal("1.0.1.0", DeviceInfo.AppVersionString);
            Assert.Equal(new Version(1, 0, 1, 0), DeviceInfo.AppVersion);
        }

        [Fact]
        public Task Screen_Metrics_Are_Not_Null()
        {
            return Utils.OnMainThread(() =>
            {
                var metrics = DeviceInfo.ScreenMetrics;

                Assert.True(metrics.Width > 0);
                Assert.True(metrics.Height > 0);
                Assert.True(metrics.Density > 0);
            });
        }
    }
}
