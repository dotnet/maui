using System.IO;
using System.IO.IsolatedStorage;
using System.Maui.Controls;

namespace System.Maui.ControlGallery.Android
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
					var files = isolatedStorage.GetFileNames (Path.Combine (directory, "*"));
					foreach (string file in files) {
						isolatedStorage.DeleteFile (Path.Combine (directory, file));
					}
				}
			}
		}
	}
}