using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP
{
	/// <summary>
	/// Enables C++ code to simulate ICLRRuntimeHost2::ExecuteAssembly on top of ICLRRuntimeHost2::CreateDelegate in .net core 2.
	/// </summary>
	static class BootstrapHelper
	{

		// Do not overload this method. ICLRRuntimeHost2::CreateDelegate can not resolve overloaded methods.
		[PreserveSig]
		private static int Bootstrap(
			[MarshalAs(UnmanagedType.LPWStr)] string assemblyPath,
			[MarshalAs(UnmanagedType.LPWStr)] string typeName,
			[MarshalAs(UnmanagedType.LPWStr)] string methodName,
			[MarshalAs(UnmanagedType.LPWStr)] string argument)
		{
			try
			{
				MethodInfo loadFrom = typeof(Assembly).GetMethod("LoadFrom", new Type[] { typeof(string) });
				if (loadFrom == null)
				{
					// LoadFrom is only available in .net core 2.0 and later. Since the target
					// assembly isn't in the normal load path there isn't anything we can do.
					throw new PlatformNotSupportedException();
				}

				Assembly assembly = (Assembly)loadFrom.Invoke(null, new object[] { assemblyPath });
				Type type = assembly.GetType(typeName);
				MethodInfo method = type.GetMethod(methodName);
				method.Invoke(null, new object[] { argument });
				return 0;
			}
			catch (Exception ex)
			{
				return ex.HResult;
			}
		}
	}
}
