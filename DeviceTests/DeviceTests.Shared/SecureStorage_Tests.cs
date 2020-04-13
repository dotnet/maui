using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests
{
    public class SecureStorage_Tests
    {
        public SecureStorage_Tests()
        {
            SecureStorage.RemoveAll();
        }

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
        [InlineData("test.txt", "data1", "data2")]
        public async Task Saves_Same_Key_Twice(string key, string data1, string data2)
        {
            await SecureStorage.SetAsync(key, data1);
            await SecureStorage.SetAsync(key, data2);

            var c = await SecureStorage.GetAsync(key);

            Assert.Equal(data2, c);
        }

#if __ANDROID__
        [Theory]
        [InlineData("test.txt", "data")]
        public async Task Fix_Corrupt_Key(string key, string data)
        {
            // set a valid key
            SecureStorage.AlwaysUseAsymmetricKeyStorage = true;
            await SecureStorage.SetAsync(key, data);

            // simulate corrupt the key
            var prefKey = "SecureStorageKey";
            var mainKey = "A2PfJSNdEDjM+422tpu7FqFcVQQbO3ti/DvnDnIqrq9CFwaBi6NdXYcicjvMW6nF7X/Clpto5xerM41U1H4qtWJDO0Ijc5QNTHGZl9tDSbXJ6yDCDDnEDryj2uTa8DiHoNcNX68QtcV3at4kkJKXXAwZXSC88a73/xDdh1u5gUdCeXJzVc5vOY6QpAGUH0bjR5NHrqEQNNGDdquFGN9n2ZJPsEK6C9fx0QwCIL+uldpAYSWrpmUIr+/0X7Y0mJpN84ldygEVxHLBuVrzB4Bbu5XGLUN/0Sr2plWcKm7XhM6wp3JRW6Eae2ozys42p1YLeM0HXWrhTqP6FRPkS6mOtw==";

            Preferences.Set(prefKey, mainKey, SecureStorage.Alias);

            var c = await SecureStorage.GetAsync(key);
            Assert.Null(c);

            // try to reset and get again
            await SecureStorage.SetAsync(key, data);
            c = await SecureStorage.GetAsync(key);

            Assert.Equal(data, c);
        }
#endif

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

            var result = SecureStorage.Remove(key);

            Assert.True(result);

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

#if __ANDROID__
        [Theory]
        [InlineData("test-key", "value1")]
        public async Task Legacy_Key(string key, string data)
        {
            byte[] encryptedData = null;
            lock (locker)
            {
                var ks = new AndroidKeyStore(Platform.AppContext, SecureStorage.Alias, SecureStorage.AlwaysUseAsymmetricKeyStorage);
                encryptedData = ks.Encrypt(data);
            }

            var encStr = Convert.ToBase64String(encryptedData);
            Preferences.Set(Utils.Md5Hash(key), encStr, Alias);

            // Ensure we read back out the right key
            var c = await SecureStorage.GetAsync(key);

            Assert.Equal(data, c);
        }
#endif
    }
}
