using Android.Content;
using System;

namespace System.Maui.Platform.Android
{
	public class AndroidAppIndexProvider : IAppIndexingProvider
	{
		public AndroidAppIndexProvider(Context context)
		{
			var fullyQualifiedName = $"{AppLinksAssemblyName}.{AppLinksClassName}, {AppLinksAssemblyName}";
			var type = Type.GetType(fullyQualifiedName, throwOnError: false);
			if (type != null)
			{
				AppLinks = Activator.CreateInstance(type, new object[] { context }, null) as IAppLinks;
			}
		}

		public IAppLinks AppLinks { get; }

		const string AppLinksAssemblyName = "System.Maui.Platform.Android.AppLinks";
		const string AppLinksClassName = "AndroidAppLinks";
	}
}