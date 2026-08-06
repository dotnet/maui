using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Essentials.DeviceTests
{

	[Category("SecureStorage")]
	[Collection(EssentialsStaticStateCollection.Name)]
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
			var alias = Assert.IsType<SecureStorageImplementation>(SecureStorage.Default).Alias;
			var all = PreferencesImplementation.GetSharedPreferences(alias).All;
			Preferences.Set(all.Keys.First(x => !x.StartsWith("_")), corruptData, alias);

			var c = await SecureStorage.GetAsync(key);
			Assert.Null(c);

			// try to reset and get again
			await SecureStorage.SetAsync(key, data);
			c = await SecureStorage.GetAsync(key);
			Assert.Equal(data, c);
		}

		[Fact]
		public void Set_Get_Wait_MultipleTimes()
		{
			for (int i = 0; i < 100; i++)
			{
				var set = SecureStorage.SetAsync(i.ToString(), i.ToString());
				set.Wait();

				var get = SecureStorage.GetAsync(i.ToString());
				get.Wait();

				Assert.Equal(i.ToString(), get.Result);
			}
		}
#endif

#if __IOS__ || MACCATALYST
		[Fact]
		public void AppInfo_Wrapper_Preserves_Default_Accessible()
		{
			var previous = new SecureStorageImplementation
			{
				DefaultAccessible = Security.SecAccessible.WhenUnlockedThisDeviceOnly,
			};
			var wrapper = new AppInfoSecureStorage("test.package", previous);

			Assert.False(wrapper.IsValueCreated);
			Assert.Equal(Security.SecAccessible.WhenUnlockedThisDeviceOnly, wrapper.DefaultAccessible);

			_ = wrapper.Alias;
			var implementation = GetWrappedImplementation(wrapper);

			Assert.True(wrapper.IsValueCreated);
			Assert.Equal(Security.SecAccessible.WhenUnlockedThisDeviceOnly, implementation.DefaultAccessible);

			wrapper.DefaultAccessible = Security.SecAccessible.AfterFirstUnlockThisDeviceOnly;

			Assert.Equal(Security.SecAccessible.AfterFirstUnlockThisDeviceOnly, implementation.DefaultAccessible);

			var successor = new AppInfoSecureStorage("successor.package", wrapper);
			Assert.Equal(Security.SecAccessible.AfterFirstUnlockThisDeviceOnly, successor.DefaultAccessible);
		}

		[Fact]
		public void AppInfo_Bridge_Inherits_Custom_Platform_Default_Accessible_Outside_Facade_Lock()
		{
			var originalAppInfo = AppInfo.Current;
			var originalSecureStorage = SecureStorage.Default;
			var previous = new PlatformSecureStorageStub
			{
				DefaultAccessible = Security.SecAccessible.WhenUnlockedThisDeviceOnly,
			};

			SecureStorage.SetDefault(previous);

			try
			{
				var builder = MauiApp.CreateBuilder();
				builder.Services.AddSingleton<IAppInfo>(
					new StubAppInfo(packageName: "bridged.securestorage.package"));

				using (builder.Build())
				{
					var wrapper = Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);

					Assert.False(wrapper.IsValueCreated);
					Assert.Equal(Security.SecAccessible.WhenUnlockedThisDeviceOnly, wrapper.DefaultAccessible);
					Assert.Equal(1, previous.DefaultAccessibleGetterCount);
				}

				Assert.Same(previous, SecureStorage.Default);
			}
			finally
			{
				SecureStorage.SetDefault(originalSecureStorage);
				AppInfo.SetCurrent(originalAppInfo);
			}
		}

		[Fact]
		public void Overlapping_AppInfo_Bridges_Preserve_Original_Custom_Platform_Default_Accessible()
		{
			var originalAppInfo = AppInfo.Current;
			var originalSecureStorage = SecureStorage.Default;
			var previous = new PlatformSecureStorageStub
			{
				DefaultAccessible = Security.SecAccessible.WhenUnlockedThisDeviceOnly,
			};
			var firstAppInfo = new StubAppInfo("first.securestorage.package");
			MauiApp firstApp = null;
			MauiApp secondApp = null;

			SecureStorage.SetDefault(previous);

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IAppInfo>(firstAppInfo);
				firstApp = firstBuilder.Build();
				var firstWrapper = Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IAppInfo>(
					new StubAppInfo(packageName: "second.securestorage.package"));
				secondApp = secondBuilder.Build();

				secondApp.Dispose();
				secondApp = null;

				Assert.Same(firstAppInfo, AppInfo.Current);
				Assert.Same(firstWrapper, SecureStorage.Default);
				Assert.Equal(
					Security.SecAccessible.WhenUnlockedThisDeviceOnly,
					firstWrapper.DefaultAccessible);

				firstApp.Dispose();
				firstApp = null;
				Assert.Same(previous, SecureStorage.Default);
			}
			finally
			{
				firstApp?.Dispose();
				secondApp?.Dispose();
				SecureStorage.SetDefault(originalSecureStorage);
				AppInfo.SetCurrent(originalAppInfo);
			}
		}

		[Fact]
		public void AppInfo_Bridge_Preserves_Custom_Accessibility_After_Predecessor_Disposal()
		{
			var originalAppInfo = AppInfo.Current;
			var originalSecureStorage = SecureStorage.Default;
			var previous = new PlatformSecureStorageStub
			{
				DefaultAccessible = Security.SecAccessible.WhenUnlockedThisDeviceOnly,
			};
			var successorAppInfo = new StubAppInfo("successor.securestorage.package");
			MauiApp predecessorApp = null;
			MauiApp successorApp = null;

			SecureStorage.SetDefault(previous);

			try
			{
				var predecessorBuilder = MauiApp.CreateBuilder();
				predecessorBuilder.Services.AddSingleton<IAppInfo>(
					new StubAppInfo(packageName: "predecessor.securestorage.package"));
				predecessorApp = predecessorBuilder.Build();
				Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);

				var successorBuilder = MauiApp.CreateBuilder();
				successorBuilder.Services.AddSingleton<IAppInfo>(successorAppInfo);
				successorApp = successorBuilder.Build();
				Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);

				predecessorApp.Dispose();
				predecessorApp = null;
				Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);

				Assert.Same(successorAppInfo, AppInfo.Current);
				var wrapper = Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);
				Assert.Equal(
					Security.SecAccessible.WhenUnlockedThisDeviceOnly,
					wrapper.DefaultAccessible);

				successorApp.Dispose();
				successorApp = null;
				Assert.Same(previous, SecureStorage.Default);
			}
			finally
			{
				successorApp?.Dispose();
				predecessorApp?.Dispose();
				SecureStorage.SetDefault(originalSecureStorage);
				AppInfo.SetCurrent(originalAppInfo);
			}
		}
#endif

#if WINDOWS
		[Fact]
		public async Task Unpackaged_Instances_Share_Current_State()
		{
			var first = new UnpackagedSecureStorageImplementation();
			var second = new UnpackagedSecureStorageImplementation();
			var key = $"test-{Guid.NewGuid():N}";
			byte[] firstValue = [1, 2, 3];
			byte[] secondValue = [4, 5, 6];

			try
			{
				await first.SetAsync(key, firstValue);
				Assert.Equal(firstValue, await second.GetAsync(key));

				await second.SetAsync(key, secondValue);
				Assert.Equal(secondValue, await first.GetAsync(key));
			}
			finally
			{
				first.Remove(key);
			}
		}

		[Fact]
		public async Task Unpackaged_State_Follows_Current_AppDataDirectory()
		{
			var originalFileSystem = FileSystem.Current;
			var root = Path.Combine(FileSystem.CacheDirectory, $"secure-storage-{Guid.NewGuid():N}");
			var firstAppData = Path.Combine(root, "first", "AppData");
			var secondAppData = Path.Combine(root, "second", "AppData");
			var firstKey = $"first-{Guid.NewGuid():N}";
			var secondKey = $"second-{Guid.NewGuid():N}";
			byte[] firstValue = [1, 2, 3];
			byte[] secondValue = [4, 5, 6];

			try
			{
				FileSystem.SetCurrent(new StubFileSystem(firstAppData));
				var first = new UnpackagedSecureStorageImplementation();
				await first.SetAsync(firstKey, firstValue);

				FileSystem.SetCurrent(new StubFileSystem(secondAppData));
				var second = new UnpackagedSecureStorageImplementation();
				Assert.Null(await second.GetAsync(firstKey));
				await second.SetAsync(secondKey, secondValue);

				FileSystem.SetCurrent(new StubFileSystem(firstAppData));
				var restoredFirst = new UnpackagedSecureStorageImplementation();
				Assert.Equal(firstValue, await restoredFirst.GetAsync(firstKey));
				Assert.Null(await restoredFirst.GetAsync(secondKey));
			}
			finally
			{
				FileSystem.SetCurrent(originalFileSystem);
				if (Directory.Exists(root))
					Directory.Delete(root, recursive: true);
			}
		}

		[Fact]
		public async Task Unpackaged_Instance_Uses_Captured_AppDataDirectory()
		{
			var originalFileSystem = FileSystem.Current;
			var root = Path.Combine(FileSystem.CacheDirectory, $"secure-storage-{Guid.NewGuid():N}");
			var firstAppData = Path.Combine(root, "first", "AppData");
			var secondAppData = Path.Combine(root, "second", "AppData");
			var key = $"test-{Guid.NewGuid():N}";
			byte[] value = [1, 2, 3];

			try
			{
				FileSystem.SetCurrent(new StubFileSystem(firstAppData));
				var storage = new UnpackagedSecureStorageImplementation();

				FileSystem.SetCurrent(new StubFileSystem(secondAppData));
				await storage.SetAsync(key, value);
				Assert.Null(await new UnpackagedSecureStorageImplementation().GetAsync(key));
				Assert.Equal(value, await storage.GetAsync(key));
				Assert.True(storage.Remove(key));

				FileSystem.SetCurrent(new StubFileSystem(firstAppData));
				Assert.Null(await new UnpackagedSecureStorageImplementation().GetAsync(key));
			}
			finally
			{
				FileSystem.SetCurrent(originalFileSystem);
				if (Directory.Exists(root))
					Directory.Delete(root, recursive: true);
			}
		}

		[Fact]
		public async Task Unpackaged_Alias_Namespaces_Shared_AppDataDirectory()
		{
			var root = Path.Combine(FileSystem.CacheDirectory, $"secure-storage-{Guid.NewGuid():N}");
			var appDataDirectory = Path.Combine(root, "AppData");
			var key = $"test-{Guid.NewGuid():N}";
			var legacyKey = $"legacy-{Guid.NewGuid():N}";
			byte[] value = [1, 2, 3];
			byte[] legacyValue = [4, 5, 6];
			var first = new UnpackagedSecureStorageImplementation(appDataDirectory, "first.alias");
			var second = new UnpackagedSecureStorageImplementation(appDataDirectory, "second.alias");
			var historical = new UnpackagedSecureStorageImplementation(appDataDirectory);

			try
			{
				Assert.NotEqual(first.CaptureWritePath(), second.CaptureWritePath());
				Assert.Equal(
					Path.Combine(appDataDirectory, "..", "Settings", "securestorage.dat"),
					historical.CaptureWritePath());

				await historical.SetAsync(legacyKey, legacyValue);
				Assert.Equal(legacyValue, await first.GetAsync(legacyKey));
				Assert.True(File.Exists(first.CaptureWritePath()));
				Assert.Equal(legacyValue, await second.GetAsync(legacyKey));
				Assert.True(File.Exists(second.CaptureWritePath()));

				await first.SetAsync(key, value);
				Assert.Equal(value, await first.GetAsync(key));
				Assert.Null(await second.GetAsync(key));
				Assert.Null(await historical.GetAsync(key));
			}
			finally
			{
				if (Directory.Exists(root))
					Directory.Delete(root, recursive: true);
			}
		}

		[Fact]
		public void AppInfo_Bridge_Captures_Owning_FileSystem_AppDataDirectory()
		{
			var originalAppInfo = AppInfo.Current;
			var originalFileSystem = FileSystem.Current;
			var originalSecureStorage = SecureStorage.Default;
			var root = Path.Combine(FileSystem.CacheDirectory, $"secure-storage-{Guid.NewGuid():N}");
			var firstAppData = Path.Combine(root, "first", "AppData");
			var secondAppData = Path.Combine(root, "second", "AppData");
			MauiApp firstApp = null;
			MauiApp secondApp = null;

			try
			{
				var firstBuilder = MauiApp.CreateBuilder();
				firstBuilder.Services.AddSingleton<IFileSystem>(new StubFileSystem(firstAppData));
				firstBuilder.Services.AddSingleton<IAppInfo>(
					new WindowsStubAppInfo("first.securestorage.package"));
				firstApp = firstBuilder.Build();
				var firstWrapper = Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);
				Assert.False(firstWrapper.IsValueCreated);

				var secondBuilder = MauiApp.CreateBuilder();
				secondBuilder.Services.AddSingleton<IFileSystem>(new StubFileSystem(secondAppData));
				secondBuilder.Services.AddSingleton<IAppInfo>(
					new WindowsStubAppInfo("second.securestorage.package"));
				secondApp = secondBuilder.Build();

				var implementation = GetWrappedImplementation(firstWrapper);
				if (SecureStorageImplementation.UsesFileSystemAppDataDirectory)
				{
					Assert.Equal(firstAppData, implementation.UnpackagedAppDataDirectory);
					Assert.True(implementation.NamespaceUnpackagedStorageByAlias);
				}
				else
				{
					Assert.Null(implementation.UnpackagedAppDataDirectory);
				}
			}
			finally
			{
				secondApp?.Dispose();
				firstApp?.Dispose();
				SecureStorage.SetDefault(originalSecureStorage);
				FileSystem.SetCurrent(originalFileSystem);
				AppInfo.SetCurrent(originalAppInfo);
				if (Directory.Exists(root))
					Directory.Delete(root, recursive: true);
			}
		}

		[Fact]
		public void FileSystem_Bridge_Owns_Default_SecureStorage_Only_For_File_Backed_Storage()
		{
			var originalSecureStorage = SecureStorage.Default;
			var root = Path.Combine(FileSystem.CacheDirectory, $"secure-storage-{Guid.NewGuid():N}");
			var appDataDirectory = Path.Combine(root, "AppData");
			MauiApp app = null;

			SecureStorage.SetDefault(null);

			try
			{
				var builder = MauiApp.CreateBuilder();
				builder.Services.AddSingleton<IFileSystem>(
					new StubFileSystem(appDataDirectory));
				app = builder.Build();

				if (SecureStorageImplementation.UsesFileSystemAppDataDirectory)
				{
					var wrapper = Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);
					var implementation = GetWrappedImplementation(wrapper);
					Assert.Equal(appDataDirectory, implementation.UnpackagedAppDataDirectory);
				}
				else
				{
					Assert.Null(GetSecureStorageBackingField());
				}

				app.Dispose();
				app = null;
				Assert.Null(GetSecureStorageBackingField());
			}
			finally
			{
				app?.Dispose();
				SecureStorage.SetDefault(originalSecureStorage);
				if (Directory.Exists(root))
					Directory.Delete(root, recursive: true);
			}
		}

		[Fact]
		public void Packaged_Default_And_AppInfo_Bridge_Do_Not_Read_FileSystem_AppDataDirectory()
		{
			if (SecureStorageImplementation.UsesFileSystemAppDataDirectory)
				return;

			var originalAppInfo = AppInfo.Current;
			var originalFileSystem = FileSystem.Current;
			var originalSecureStorage = SecureStorage.Default;
			var fileSystem = new ThrowingAppDataDirectoryFileSystem();

			try
			{
				AppInfo.SetCurrent(new WindowsStubAppInfo("packaged.default.securestorage"));
				FileSystem.SetCurrent(fileSystem);
				SecureStorage.SetDefault(null);

				var defaultImplementation =
					Assert.IsType<SecureStorageImplementation>(SecureStorage.Default);
				Assert.Null(defaultImplementation.UnpackagedAppDataDirectory);

				SecureStorage.SetDefault(null);
				var builder = MauiApp.CreateBuilder();
				builder.Services.AddSingleton<IFileSystem>(fileSystem);
				builder.Services.AddSingleton<IAppInfo>(
					new WindowsStubAppInfo("packaged.bridged.securestorage"));

				using var app = builder.Build();
				var wrapper = Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);
				var bridgedImplementation = GetWrappedImplementation(wrapper);

				Assert.Null(bridgedImplementation.UnpackagedAppDataDirectory);
			}
			finally
			{
				SecureStorage.SetDefault(originalSecureStorage);
				FileSystem.SetCurrent(originalFileSystem);
				AppInfo.SetCurrent(originalAppInfo);
			}
		}

		[Fact]
		public void AppInfo_Only_Bridge_Uses_Native_AppDataDirectory_When_FileSystem_Is_Owned_By_Another_App()
		{
			if (!SecureStorageImplementation.UsesFileSystemAppDataDirectory)
				return;

			var originalAppInfo = AppInfo.Current;
			var originalFileSystem = FileSystem.Current;
			var originalSecureStorage = SecureStorage.Default;
			var root = Path.Combine(FileSystem.CacheDirectory, $"secure-storage-{Guid.NewGuid():N}");
			var ambientAppData = Path.Combine(root, "ambient", "AppData");
			MauiApp fileSystemApp = null;
			MauiApp appInfoApp = null;

			try
			{
				var fileSystemBuilder = MauiApp.CreateBuilder();
				fileSystemBuilder.Services.AddSingleton<IFileSystem>(
					new StubFileSystem(ambientAppData));
				fileSystemApp = fileSystemBuilder.Build();

				var appInfoBuilder = MauiApp.CreateBuilder();
				appInfoBuilder.Services.AddSingleton<IAppInfo>(
					new WindowsStubAppInfo("custom.securestorage.package"));
				appInfoApp = appInfoBuilder.Build();

				var wrapper = Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);
				var implementation = GetWrappedImplementation(wrapper);
				Assert.Equal(
					FileSystemImplementation.GetDefaultAppDataDirectory(),
					implementation.UnpackagedAppDataDirectory);
				Assert.NotEqual(ambientAppData, implementation.UnpackagedAppDataDirectory);
			}
			finally
			{
				appInfoApp?.Dispose();
				fileSystemApp?.Dispose();
				SecureStorage.SetDefault(originalSecureStorage);
				FileSystem.SetCurrent(originalFileSystem);
				AppInfo.SetCurrent(originalAppInfo);
				if (Directory.Exists(root))
					Directory.Delete(root, recursive: true);
			}
		}

		[Fact]
		public void FileSystem_Only_Bridge_Uses_Owned_AppDataDirectory_When_AppInfo_Is_Owned_By_Another_App()
		{
			if (!SecureStorageImplementation.UsesFileSystemAppDataDirectory)
				return;

			var originalAppInfo = AppInfo.Current;
			var originalFileSystem = FileSystem.Current;
			var originalSecureStorage = SecureStorage.Default;
			var historicalAlias = new SecureStorageImplementation().Alias;
			var root = Path.Combine(FileSystem.CacheDirectory, $"secure-storage-{Guid.NewGuid():N}");
			var ownedAppData = Path.Combine(root, "owned", "AppData");
			MauiApp appInfoApp = null;
			MauiApp fileSystemApp = null;

			try
			{
				var appInfoBuilder = MauiApp.CreateBuilder();
				appInfoBuilder.Services.AddSingleton<IAppInfo>(
					new WindowsStubAppInfo("custom.securestorage.package"));
				appInfoApp = appInfoBuilder.Build();

				var fileSystemBuilder = MauiApp.CreateBuilder();
				fileSystemBuilder.Services.AddSingleton<IFileSystem>(
					new StubFileSystem(ownedAppData));
				fileSystemApp = fileSystemBuilder.Build();

				var wrapper = Assert.IsType<AppInfoSecureStorage>(SecureStorage.Default);
				var implementation = GetWrappedImplementation(wrapper);
				Assert.Equal(ownedAppData, implementation.UnpackagedAppDataDirectory);
				Assert.Equal(historicalAlias, implementation.Alias);
			}
			finally
			{
				fileSystemApp?.Dispose();
				appInfoApp?.Dispose();
				SecureStorage.SetDefault(originalSecureStorage);
				FileSystem.SetCurrent(originalFileSystem);
				AppInfo.SetCurrent(originalAppInfo);
				if (Directory.Exists(root))
					Directory.Delete(root, recursive: true);
			}
		}

		sealed class StubFileSystem : IFileSystem
		{
			public StubFileSystem(string appDataDirectory)
			{
				AppDataDirectory = appDataDirectory;
			}

			public string CacheDirectory => AppDataDirectory;

			public string AppDataDirectory { get; }

			public Task<Stream> OpenAppPackageFileAsync(string filename) =>
				throw new NotSupportedException();

			public Task<bool> AppPackageFileExistsAsync(string filename) =>
				throw new NotSupportedException();
		}

		sealed class ThrowingAppDataDirectoryFileSystem : IFileSystem
		{
			public string CacheDirectory => Path.GetTempPath();

			public string AppDataDirectory =>
				throw new InvalidOperationException("Packaged SecureStorage must not read AppDataDirectory.");

			public Task<Stream> OpenAppPackageFileAsync(string filename) =>
				throw new NotSupportedException();

			public Task<bool> AppPackageFileExistsAsync(string filename) =>
				throw new NotSupportedException();
		}

		class WindowsStubAppInfo : IAppInfo
		{
			public WindowsStubAppInfo(string packageName)
			{
				PackageName = packageName;
			}

			public virtual string PackageName { get; }

			public string Name => "Test";

			public string VersionString => "1.0.0";

			public Version Version => Version.Parse(VersionString);

			public string BuildString => "1";

			public AppTheme RequestedTheme => AppTheme.Light;

			public AppPackagingModel PackagingModel => AppPackagingModel.Packaged;

			public LayoutDirection RequestedLayoutDirection => LayoutDirection.LeftToRight;

			public void ShowSettingsUI()
			{
			}
		}

		static SecureStorageImplementation GetWrappedImplementation(
			AppInfoSecureStorage wrapper)
		{
			var field = typeof(AppInfoSecureStorage)
				.GetField("_implementation", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(field);
			var lazy = Assert.IsType<Lazy<SecureStorageImplementation>>(field.GetValue(wrapper));
			return lazy.Value;
		}

		static ISecureStorage GetSecureStorageBackingField()
		{
			var field = typeof(SecureStorage)
				.GetField("defaultImplementation", BindingFlags.Static | BindingFlags.NonPublic);
			Assert.NotNull(field);
			return (ISecureStorage)field.GetValue(null);
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

		[Fact]
		public void Default_Creation_Uses_Native_PackageName()
		{
			var originalAppInfo = AppInfo.Current;
			var originalSecureStorage = SecureStorage.Default;
			var historicalAlias = new SecureStorageImplementation().Alias;
			var appInfo = new ThrowingPackageNameAppInfo();

			try
			{
				AppInfo.SetCurrent(appInfo);
				SecureStorage.SetDefault(null);

				var implementation = Assert.IsType<SecureStorageImplementation>(SecureStorage.Default);

				Assert.Equal(historicalAlias, implementation.Alias);
			}
			finally
			{
				SecureStorage.SetDefault(originalSecureStorage);
				AppInfo.SetCurrent(originalAppInfo);
			}
		}

#if __IOS__ || MACCATALYST
		static SecureStorageImplementation GetWrappedImplementation(AppInfoSecureStorage wrapper)
		{
			var field = typeof(AppInfoSecureStorage)
				.GetField("_implementation", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(field);
			var lazy = Assert.IsType<Lazy<SecureStorageImplementation>>(field.GetValue(wrapper));
			return lazy.Value;
		}

		sealed class PlatformSecureStorageStub : ISecureStorage, IPlatformSecureStorage
		{
			Security.SecAccessible _defaultAccessible = Security.SecAccessible.AfterFirstUnlock;

			public int DefaultAccessibleGetterCount { get; private set; }

			public Security.SecAccessible DefaultAccessible
			{
				get
				{
					Assert.False(
						Monitor.IsEntered(EssentialsImplementation.GetSyncRoot<ISecureStorage>()),
						"Custom IPlatformSecureStorage.DefaultAccessible must not be read while the SecureStorage facade lock is held.");

					DefaultAccessibleGetterCount++;
					return _defaultAccessible;
				}
				set => _defaultAccessible = value;
			}

			public Task<string> GetAsync(string key) =>
				Task.FromResult<string>(null);

			public Task SetAsync(string key, string value) =>
				Task.CompletedTask;

			public Task SetAsync(string key, string value, Security.SecAccessible accessible) =>
				Task.CompletedTask;

			public bool Remove(string key) =>
				false;

			public void RemoveAll()
			{
			}
		}

		class StubAppInfo : IAppInfo
		{
			readonly string _packageName;

			public StubAppInfo(string packageName)
			{
				_packageName = packageName;
			}

			public virtual string PackageName => _packageName;

			public string Name => "Test";

			public string VersionString => "1.0.0";

			public Version Version => Version.Parse(VersionString);

			public string BuildString => "1";

			public AppTheme RequestedTheme => AppTheme.Light;

			public AppPackagingModel PackagingModel => AppPackagingModel.Packaged;

			public LayoutDirection RequestedLayoutDirection => LayoutDirection.LeftToRight;

			public void ShowSettingsUI()
			{
			}
		}

#endif

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

#if WINDOWS
			(Skip = "IOException on unpackaged: The process cannot access the file...")
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

#if WINDOWS
			(Skip = "IOException on unpackaged: The process cannot access the file...")
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

		sealed class ThrowingPackageNameAppInfo : IAppInfo
		{
			public string PackageName =>
				throw new InvalidOperationException("Implicit SecureStorage must use the native package identity.");

			public string Name => "Test";

			public string VersionString => "1.0.0";

			public Version Version => Version.Parse(VersionString);

			public string BuildString => "1";

			public AppTheme RequestedTheme => AppTheme.Light;

			public AppPackagingModel PackagingModel => AppPackagingModel.Packaged;

			public LayoutDirection RequestedLayoutDirection => LayoutDirection.LeftToRight;

			public void ShowSettingsUI()
			{
			}
		}
	}
}
