using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    public class SecureStorage_Tests
    {
        [Theory]
        [InlineData("test.txt", "data", true, true)]
        [InlineData("noextension", "data2", true, false)]
        [InlineData("funny*&$%@!._/\\chars", "data3", true, false)]
        [InlineData("test.txt2", "data2", false, true)]
        [InlineData("noextension2", "data22", false, false)]
        [InlineData("funny*&$%@!._/\\chars2", "data32", false, false)]
        public async Task Saves_And_Loads(string key, string data, bool emulatePreApi23, bool emulateNonEnglishLocale)
        {
#if __IOS__
            // Try the new platform specific api
            await SecureStorage.SetAsync(key, data, Security.SecAccessible.AfterFirstUnlock);

            var b = await SecureStorage.GetAsync(key);

            Assert.Equal(data, b);
#endif

#if __ANDROID__
            SecureStorage.AlwaysUseAsymmetricKeyStorage = emulatePreApi23;

            if (emulateNonEnglishLocale)
            {
                Platform.SetLocale(new Java.Util.Locale("ar"));
            }
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

        [Theory]
        [InlineData("KEY_TO_REMOVE1", true)]
        [InlineData("KEY_TO_REMOVE2", false)]
        public async Task Remove_Key(string key, bool emulatePreApi23)
        {
#if __ANDROID__
            SecureStorage.AlwaysUseAsymmetricKeyStorage = emulatePreApi23;
#endif
            await SecureStorage.SetAsync(key, "Irrelevant Data");

            SecureStorage.Remove(key);

            var v = await SecureStorage.GetAsync(key);

            Assert.Null(v);
        }

        [Theory]
        [InlineData(true, new[] { "KEYS_TO_REMOVEA1", "KEYS_TO_REMOVEA2" })]
        [InlineData(false, new[] { "KEYS_TO_REMOVEB1", "KEYS_TO_REMOVEB2" })]
        public async Task Remove_All_Keys(bool emulatePreApi23, string[] keys)
        {
#if __ANDROID__
            SecureStorage.AlwaysUseAsymmetricKeyStorage = emulatePreApi23;
#endif

            // Set a couple keys
            foreach (var key in keys)
                await SecureStorage.SetAsync(key, "Irrelevant Data");

            // Remove them all
            SecureStorage.RemoveAll();

            // Make sure they are all removed
            foreach (var key in keys)
                Assert.Null(await SecureStorage.GetAsync(key));
        }

#if __ANDROID__
        [Fact]
        public async Task Asymmetric_to_Symmetric_API_Upgrade()
        {
            var key = "asym_to_sym_upgrade";
            var expected = "this is the value";

            SecureStorage.RemoveAll();

            // Emulate pre api 23
            SecureStorage.AlwaysUseAsymmetricKeyStorage = true;

            await SecureStorage.SetAsync(key, expected);

            // Simulate Upgrading to API23+
            SecureStorage.AlwaysUseAsymmetricKeyStorage = false;

            var v = await SecureStorage.GetAsync(key);

            SecureStorage.RemoveAll();

            Assert.Equal(expected, v);
        }
#endif
    }
}
