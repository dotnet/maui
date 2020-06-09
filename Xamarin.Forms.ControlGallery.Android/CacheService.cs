using System.IO.IsolatedStorage;
using Xamarin.Forms.Controls;
using IOPath = System.IO.Path;

namespace Xamarin.Forms.ControlGallery.Android
{
	public class CacheService : ICacheService
	{
		public void ClearImageCache ()
		{
			DeleteFilesInDirectory ("ImageLoaderCache");
		}

		static void DeleteFilesInDirectory (string directory)
		{
			using (IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication ()) {
				if (isolatedStorage.DirectoryExists (directory)) {
					var files = isolatedStorage.GetFileNames (IOPath.Combine (directory, "*"));
					foreach (string file in files) {
						isolatedStorage.DeleteFile (IOPath.Combine (directory, file));
					}
				}
			}
		}
	}
}