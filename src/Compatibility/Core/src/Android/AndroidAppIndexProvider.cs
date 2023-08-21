//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using System.Diagnostics.CodeAnalysis;
using Android.Content;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public class AndroidAppIndexProvider : IAppIndexingProvider
	{
		[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = AppLinksAssemblyName + ".dll is not always present.")]
		[UnconditionalSuppressMessage("Trimming", "IL2035", Justification = AppLinksAssemblyName + ".dll is not always present.")]
		[UnconditionalSuppressMessage("Trimming", "IL2072", Justification = AppLinksAssemblyName + ".dll is not always present.")]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, AppLinksAssemblyName + "." + AppLinksClassName, AppLinksAssemblyName)]
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

		const string AppLinksAssemblyName = "Microsoft.Maui.Controls.Compatibility.Platform.Android.AppLinks";
		const string AppLinksClassName = "AndroidAppLinks";
	}
}