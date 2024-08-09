// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

#pragma warning disable CS8600
#if COMPATIBILITY
namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP
#else
namespace Microsoft.Maui
#endif
{
	/// <summary>
	/// Enables C++ code to simulate ICLRRuntimeHost2::ExecuteAssembly on top of ICLRRuntimeHost2::CreateDelegate in .net core 2.
	/// </summary>
#if COMPATIBILITY
	static class BootstrapHelper
#else
	static class VisualDiagnosticsBootstrapHelper
#endif
	{
		// Do not overload this method. ICLRRuntimeHost2::CreateDelegate cannot resolve overloaded methods.
		[PreserveSig]
		[RequiresUnreferencedCode("The assembly, type, or method may have been trimmed if it wasn't preserved another way.")]
		private static int Bootstrap(
			[MarshalAs(UnmanagedType.LPWStr)] string assemblyPath,
			[MarshalAs(UnmanagedType.LPWStr)] string typeName,
			[MarshalAs(UnmanagedType.LPWStr)] string methodName,
			[MarshalAs(UnmanagedType.LPWStr)] string argument)
		{
			try
			{
				MethodInfo loadFrom = typeof(Assembly).GetMethod("LoadFrom", new Type[] { typeof(string) });
				if (loadFrom is null)
				{
					// LoadFrom is only available in .net core 2.0 and later. Since the target
					// assembly isn't in the normal load path there isn't anything we can do.
					throw new PlatformNotSupportedException();
				}

				Assembly assembly = (Assembly)loadFrom.Invoke(null, new object[] { assemblyPath });
				Type type = assembly?.GetType(typeName);
				MethodInfo method = type?.GetMethod(methodName);
				method?.Invoke(null, new object[] { argument });
				return 0;
			}
			catch (Exception ex)
			{
				return ex.HResult;
			}
		}
	}
}
#pragma warning restore CS8600
