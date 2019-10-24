using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Core;

namespace Xamarin.Forms.Platform.WinRT
{
	internal class WindowsPhonePlatformServices
		: WindowsBasePlatformServices
	{
		public WindowsPhonePlatformServices (CoreDispatcher dispatcher)
			: base (dispatcher)
		{
		}

		public override Assembly[] GetAssemblies()
		{
			var files = Package.Current.InstalledLocation.GetFilesAsync().AsTask().Result;
			
			List<Assembly> assemblies = new List<Assembly> (files.Count);
			for (int i = 0; i < files.Count; i++) {
				StorageFile file = files[i];
				if (file.Name.Length < 3)
					continue;

				string extension = file.Name.Substring (file.Name.Length - 3, 3).ToLower();
				if (extension != "dll" && extension != "exe")
					continue;

				try {
					Assembly assembly = Assembly.Load (new AssemblyName { Name = Path.GetFileNameWithoutExtension (file.Name) });
					assemblies.Add (assembly);
				} catch (IOException) {
				} catch (BadImageFormatException) {
				}
			}

			return assemblies.ToArray();
		}
	}
}