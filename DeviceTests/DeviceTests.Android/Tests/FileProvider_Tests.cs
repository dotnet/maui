using System;
using System.IO;
using System.Linq;
using Xamarin.Essentials;
using Xunit;
using AndroidEnvironment = Android.OS.Environment;

namespace DeviceTests.Shared
{
    public class Android_FileProvider_Tests
    {
        [Fact]
        [Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
        public void Share_Simple_Text_File_Test()
        {
            // Save a local cache data directory file
            var file = CreateFile(FileSystem.AppDataDirectory, "share-test.txt");

            // Make sure it is where we expect it to be
            Assert.False(FileProvider.IsFileInPublicLocation(file));

            // Actually get a safe shareable file uri
            var shareableUri = Platform.GetShareableFileUri(new ReadOnlyFile(file));

            // Launch an intent to let tye user pick where to open this content
            var intent = new Android.Content.Intent(Android.Content.Intent.ActionSend);
            intent.SetType("text/plain");
            intent.PutExtra(Android.Content.Intent.ExtraStream, shareableUri);
            intent.PutExtra(Android.Content.Intent.ExtraTitle, "Title Here");
            intent.SetFlags(Android.Content.ActivityFlags.GrantReadUriPermission);

            var intentChooser = Android.Content.Intent.CreateChooser(intent, "Pick something");

            Platform.CurrentActivity.StartActivity(intentChooser);
        }

        [Theory]
        [InlineData(true, FileProviderLocation.Internal)]
        [InlineData(true, FileProviderLocation.PreferExternal)]
        [InlineData(false, FileProviderLocation.Internal)]
        [InlineData(false, FileProviderLocation.PreferExternal)]
        public void Get_Shareable_Uri(bool failAccess, FileProviderLocation location)
        {
            // Always fail to simulate unmounted media
            FileProvider.AlwaysFailExternalMediaAccess = failAccess;

            try
            {
                // Save a local cache data directory file
                var file = CreateFile(FileSystem.AppDataDirectory);

                // Make sure it is where we expect it to be
                Assert.False(FileProvider.IsFileInPublicLocation(file));

                // Actually get a safe shareable file uri
                var shareableUri = GetShareableUri(file, location);

                // Determine where the file should be found
                var isInternal = failAccess || location == FileProviderLocation.Internal;
                var expectedCache = isInternal ? "internal_cache" : "external_cache";
                var expectedCacheDir = isInternal
                    ? Platform.AppContext.CacheDir.AbsolutePath
                    : Platform.AppContext.ExternalCacheDir.AbsolutePath;

                // Make sure the uri is what we expected
                Assert.NotNull(shareableUri);
                Assert.Equal("content", shareableUri.Scheme);
                Assert.Equal("com.xamarin.essentials.devicetests.fileProvider", shareableUri.Authority);
                Assert.Equal(4, shareableUri.PathSegments.Count);
                Assert.Equal(expectedCache, shareableUri.PathSegments[0]);
                Assert.Equal("2203693cc04e0be7f4f024d5f9499e13", shareableUri.PathSegments[1]);
                Assert.True(Guid.TryParseExact(shareableUri.PathSegments[2], "N", out var guid));
                Assert.Equal(Path.GetFileName(file), shareableUri.PathSegments[3]);

                // Make sure the underlying file exists
                var realPath = Path.Combine(shareableUri.PathSegments.ToArray())
                    .Replace(expectedCache, expectedCacheDir);
                Assert.True(File.Exists(realPath));
            }
            finally
            {
                FileProvider.AlwaysFailExternalMediaAccess = false;
            }
        }

        [Fact]
        public void No_Media_Fails_Get_External_Cache_Shareable_Uri()
        {
            // Always fail to simulate unmounted media
            FileProvider.AlwaysFailExternalMediaAccess = true;

            try
            {
                // Save a local cache data directory file
                var file = CreateFile(FileSystem.AppDataDirectory);

                // Make sure it is where we expect it to be
                Assert.False(FileProvider.IsFileInPublicLocation(file));

                // try get a uri, but fail as there is no external storage
                Assert.Throws<InvalidOperationException>(() => GetShareableUri(file, FileProviderLocation.External));
            }
            finally
            {
                FileProvider.AlwaysFailExternalMediaAccess = false;
            }
        }

        [Fact]
        public void Get_External_Cache_Shareable_Uri()
        {
            // Save a local cache data directory file
            var file = CreateFile(FileSystem.AppDataDirectory);

            // Make sure it is where we expect it to be
            Assert.False(FileProvider.IsFileInPublicLocation(file));

            // Actually get a safe shareable file uri
            var shareableUri = GetShareableUri(file, FileProviderLocation.External);

            // Make sure the uri is what we expected
            Assert.NotNull(shareableUri);
            Assert.Equal("content", shareableUri.Scheme);
            Assert.Equal("com.xamarin.essentials.devicetests.fileProvider", shareableUri.Authority);
            Assert.Equal(4, shareableUri.PathSegments.Count);
            Assert.Equal("external_cache", shareableUri.PathSegments[0]);
            Assert.Equal("2203693cc04e0be7f4f024d5f9499e13", shareableUri.PathSegments[1]);
            Assert.True(Guid.TryParseExact(shareableUri.PathSegments[2], "N", out var guid));
            Assert.Equal(Path.GetFileName(file), shareableUri.PathSegments[3]);

            // Make sure the underlying file exists
            var realPath = Path.Combine(shareableUri.PathSegments.ToArray())
                .Replace("external_cache", Platform.AppContext.ExternalCacheDir.AbsolutePath);
            Assert.True(File.Exists(realPath));
        }

        [Theory]
        [InlineData(FileProviderLocation.External)]
        [InlineData(FileProviderLocation.Internal)]
        [InlineData(FileProviderLocation.PreferExternal)]
        public void Get_Existing_Internal_Cache_Shareable_Uri(FileProviderLocation location)
        {
            // Save a local cache directory file
            var file = CreateFile(Platform.AppContext.CacheDir.AbsolutePath);

            // Make sure it is where we expect it to be
            Assert.True(FileProvider.IsFileInPublicLocation(file));

            // Actually get a safe shareable file uri
            var shareableUri = GetShareableUri(file, location);

            // Make sure the uri is what we expected
            Assert.NotNull(shareableUri);
            Assert.Equal("content", shareableUri.Scheme);
            Assert.Equal("com.xamarin.essentials.devicetests.fileProvider", shareableUri.Authority);
            Assert.Equal(new[] { "internal_cache", Path.GetFileName(file) }, shareableUri.PathSegments);
        }

        [Theory]
        [InlineData(FileProviderLocation.External)]
        [InlineData(FileProviderLocation.Internal)]
        [InlineData(FileProviderLocation.PreferExternal)]
        public void Get_Existing_External_Cache_Shareable_Uri(FileProviderLocation location)
        {
            // Save an external cache directory file
            var file = CreateFile(Platform.AppContext.ExternalCacheDir.AbsolutePath);

            // Make sure it is where we expect it to be
            Assert.True(FileProvider.IsFileInPublicLocation(file));

            // Actually get a safe shareable file uri
            var shareableUri = GetShareableUri(file, location);

            // Make sure the uri is what we expected
            Assert.NotNull(shareableUri);
            Assert.Equal("content", shareableUri.Scheme);
            Assert.Equal("com.xamarin.essentials.devicetests.fileProvider", shareableUri.Authority);
            Assert.Equal(new[] { "external_cache", Path.GetFileName(file) }, shareableUri.PathSegments);
        }

        [Theory]
        [InlineData(FileProviderLocation.External)]
        [InlineData(FileProviderLocation.Internal)]
        [InlineData(FileProviderLocation.PreferExternal)]
        public void Get_Existing_External_Shareable_Uri(FileProviderLocation location)
        {
            // Save an external directory file

            #if !__ANDROID_29__
            var externalRoot = AndroidEnvironment.ExternalStorageDirectory.AbsolutePath;
            #endif

            var root = Platform.AppContext.GetExternalFilesDir(null).AbsolutePath;
            var file = CreateFile(root);

            // Make sure it is where we expect it to be
            Assert.True(FileProvider.IsFileInPublicLocation(file));

            // Actually get a safe shareable file uri
            var shareableUri = GetShareableUri(file, location);

            // Make sure the uri is what we expected
            Assert.NotNull(shareableUri);
            Assert.Equal("content", shareableUri.Scheme);
            Assert.Equal("com.xamarin.essentials.devicetests.fileProvider", shareableUri.Authority);

            #if !__ANDROID_29__
            // replace the real root with the providers "root"
            var segements = Path.Combine(root.Replace(externalRoot, "external_files"), Path.GetFileName(file));

            Assert.Equal(segements.Split(Path.DirectorySeparatorChar), shareableUri.PathSegments);
            #endif
        }

        static string CreateFile(string root, string name = "the-file.txt")
        {
            var file = Path.Combine(root, name);

            if (File.Exists(file))
                File.Delete(file);

            File.WriteAllText(file, "The file contents.");

            return file;
        }

        static Android.Net.Uri GetShareableUri(string file, FileProviderLocation location)
        {
            try
            {
                // use the specific location
                FileProvider.TemporaryLocation = location;

                // get the uri
                return Platform.GetShareableFileUri(new ReadOnlyFile(file));
            }
            finally
            {
                // reset the location
                FileProvider.TemporaryLocation = FileProviderLocation.PreferExternal;
            }
        }
    }
}
