using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Essentials.DeviceTests
{

	[Category("SecureStorage")]
	[Collection("UsesPreferences")]
	public class SecureStorage_Tests
	{
		public SecureStorage_Tests()
		{
			SecureStorage.RemoveAll();
		}

		[Theory
#if MACCATALYST
			(Skip = "Need to configure entitlements.")
#endif
		]
		[InlineData("test.txt", "data")]
		[InlineData("noextension", "data2")]
		[InlineData("funny*&$%@!._/\\chars", "data3")]
		[InlineData("test.txt2", "data2")]
		[InlineData("noextension2", "data22")]
		[InlineData("funny*&$%@!._/\\chars2", "data32")]
		public async Task Saves_And_Loads(string key, string data)
		{
#if __IOS__
			// Try the new platform specific api
			await SecureStorage.SetAsync(key, data, Security.SecAccessible.AfterFirstUnlock);

			var b = await SecureStorage.GetAsync(key);

			Assert.Equal(data, b);
#endif
			await SecureStorage.SetAsync(key, data);

			var c = await SecureStorage.GetAsync(key);

			Assert.Equal(data, c);
		}

		[Theory
#if MACCATALYST
			(Skip = "Need to configure entitlements.")
#endif
		]
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
		public async Task Fix_Corrupt_Data(string key, string data)
		{
			// this operation is only available on API level 23+ devices
			if (!OperatingSystem.IsAndroidVersionAtLeast(23))
				return;

			// set a valid key
			await SecureStorage.SetAsync(key, data);

			// simulate corrupt the key
			var corruptData = "A2PfJSNdEDjM+422tpu7FqFcVQQbO3ti/DvnDnIqrq9CFwaBi6NdXYcicjvMW6nF7X/Clpto5xerM41U1H4qtWJDO0Ijc5QNTHGZl9tDSbXJ6yDCDDnEDryj2uTa8DiHoNcNX68QtcV3at4kkJKXXAwZXSC88a73/xDdh1u5gUdCeXJzVc5vOY6QpAGUH0bjR5NHrqEQNNGDdquFGN9n2ZJPsEK6C9fx0QwCIL+uldpAYSWrpmUIr+/0X7Y0mJpN84ldygEVxHLBuVrzB4Bbu5XGLUN/0Sr2plWcKm7XhM6wp3JRW6Eae2ozys42p1YLeM0HXWrhTqP6FRPkS6mOtw==";
			var all = PreferencesImplementation.GetSharedPreferences(SecureStorageImplementation.Alias).All;
			Preferences.Set(all.Keys.First(x => !x.StartsWith("_")), corruptData, SecureStorageImplementation.Alias);

			var c = await SecureStorage.GetAsync(key);
			Assert.Null(c);

			// try to reset and get again
			await SecureStorage.SetAsync(key, data);
			c = await SecureStorage.GetAsync(key);
			Assert.Equal(data, c);
		}
#endif

		[Fact
#if MACCATALYST
			(Skip = "Need to configure entitlements.")
#endif
		]
		public async Task Non_Existent_Key_Returns_Null()
		{
			var v = await SecureStorage.GetAsync("THIS_KEY_SHOULD_NOT_EXIST");
			Assert.Null(v);
		}

		[Theory
#if MACCATALYST
			(Skip = "Need to configure entitlements.")
#endif
		]
		[InlineData("KEY_TO_REMOVE1")]
		[InlineData("KEY_TO_REMOVE2")]
		public async Task Remove_Key(string key)
		{
			await SecureStorage.SetAsync(key, "Irrelevant Data");

			var result = SecureStorage.Remove(key);
			Assert.True(result);

			var v = await SecureStorage.GetAsync(key);
			Assert.Null(v);
		}

		[Theory
#if MACCATALYST
			(Skip = "Need to configure entitlements.")
#endif
		]
		[InlineData("KEYS_TO_REMOVEA1", "KEYS_TO_REMOVEA2")]
		[InlineData("KEYS_TO_REMOVEB1", "KEYS_TO_REMOVEB2")]
		public async Task Remove_All_Keys(string key1, string key2)
		{
			string[] keys = new[] { key1, key2 };

			// Set a couple keys
			foreach (var key in keys)
				await SecureStorage.SetAsync(key, "Irrelevant Data");

			// Remove them all
			SecureStorage.RemoveAll();

			// Make sure they are all removed
			foreach (var key in keys)
			{
				var result = await SecureStorage.GetAsync(key);
				Assert.Null(result);
			}
		}

#if __ANDROID__
		[Fact]
		public async Task Asymmetric_to_Symmetric_API_Upgrade()
		{
			var key = "asym_to_sym_upgrade";
			var expected = "this is the value";

			SecureStorage.RemoveAll();

			await SecureStorage.SetAsync(key, expected);

			var v = await SecureStorage.GetAsync(key);

			SecureStorage.RemoveAll();

			Assert.Equal(expected, v);
		}
#endif

		[Fact
#if MACCATALYST
			(Skip = "Need to configure entitlements.")
#endif
		]
		public async Task Set_Get_Async_MultipleTimes()
		{
			await Parallel.ForEachAsync(Enumerable.Range(0, 100), async (i, _) =>
				await SecureStorage.SetAsync(i.ToString(), i.ToString())
			);

			for (int i = 0; i < 100; i++)
			{
				var v = await SecureStorage.GetAsync(i.ToString());
				Assert.Equal(i.ToString(), v);
			}
		}

		[Fact
#if MACCATALYST
			(Skip = "Need to configure entitlements.")
#endif
		]
		public async Task Set_Get_Remove_Async_MultipleTimes()
		{
			await Parallel.ForEachAsync(Enumerable.Range(0, 100), async (i, _) =>
			{
				var key = $"key{i}";
				var value = $"value{i}";
				await SecureStorage.SetAsync(key, value);
				var fetched = await SecureStorage.GetAsync(key);
				Assert.Equal(value, fetched);
				SecureStorage.Remove(key);
				fetched = await SecureStorage.GetAsync(key);
				Assert.Null(fetched);
			});
		}
	}
}
