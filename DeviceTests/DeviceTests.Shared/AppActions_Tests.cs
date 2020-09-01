using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xunit;

namespace DeviceTests
{
    public class AppActions_Tests
    {
        [Fact]
        public void IsSupported()
        {
            var expectSupported = false;

#if __ANDROID_25__
            expectSupported = true;
#endif

#if __IOS__
            if (Platform.HasOSVersion(9, 0))
                expectSupported = true;
#endif
            Assert.Equal(expectSupported, AppActions.IsSupported);
        }

#if __ANDROID_25__ || __IOS__
        [Fact]
        public void GetSetItems()
        {
            if (AppActions.IsSupported)
            {
                AppActions.Actions = new List<AppAction>
                {
                    new AppAction("TEST1", "Test 1", "This is a test", new System.Uri("myapp://test1")),
                    new AppAction("TEST2", "Test 2", "This is a test 2", new System.Uri("myapp://test2")),
                };

                Assert.Contains(AppActions.Actions, a => a.ActionType == "TEST1");
            }
        }
#endif
    }
}
