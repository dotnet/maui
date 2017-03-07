using Android.Content;
using System;
using System.Reflection;
using System.Linq;
using System.Globalization;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	public class AndroidAppIndexProvider : IAppIndexingProvider
	{
		public AndroidAppIndexProvider(Context context)
		{
			var assemblyAppLinks = GetAssemblyForAppLinks(AppLinksAssemblyName);

			if (assemblyAppLinks != null)
			{
				Type type = assemblyAppLinks.GetType($"{AppLinksAssemblyName}.{AppLinksClassName}");

				if (type != null)
				{
					var applink = Activator.CreateInstance(type, new object[] { context }, null);

					if (applink != null)
					{
						AppLinks = applink as IAppLinks;
					}
				}
			}
		}

		public IAppLinks AppLinks { get; }

		private Assembly GetAssemblyForAppLinks(string assemblyName)
		{
			return Device.GetAssemblies().FirstOrDefault(assembly => assembly.GetName().Name == assemblyName);
		}

		const string AppLinksAssemblyName = "Xamarin.Forms.Platform.Android.AppLinks";
		const string AppLinksClassName = "AndroidAppLinks";
	}
}