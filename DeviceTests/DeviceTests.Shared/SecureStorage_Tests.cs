using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    public class SecureStorage_Tests
    {
        [Theory]
        [InlineData("test.txt", "data", true)]
        [InlineData("noextension", "data2", true)]
        [InlineData("funny*&$%@!._/\\chars", "data3", true)]
        [InlineData("test.txt2", "data2", false)]
        [InlineData("noextension2", "data22", false)]
        [InlineData("funny*&$%@!._/\\chars2", "data32", false)]
        public async Task Saves_And_Loads(string key, string data, bool emulatePreApi23)
        {
#if __IOS__
            // Try the new platform specific api
            await SecureStorage.SetAsync(key, data, Security.SecAccessible.AfterFirstUnlock);

            var b = await SecureStorage.GetAsync(key, Security.SecAccessible.AfterFirstUnlock);

            Assert.Equal(data, b);
#endif

#if __ANDROID__
            SecureStorage.AlwaysUseAsymmetricKeyStorage = emulatePreApi23;
#endif

            await SecureStorage.SetAsync(key, data);

            var c = await SecureStorage.GetAsync(key);

            Assert.Equal(data, c);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Non_Existent_Key_Returns_Null(bool emulatePreApi23)
        {
#if __ANDROID__
            SecureStorage.AlwaysUseAsymmetricKeyStorage = emulatePreApi23;
#endif
            var v = await SecureStorage.GetAsync("THIS_KEY_SHOULD_NOT_EXIST");

            Assert.Null(v);
        }
    }
}
