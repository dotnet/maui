using System;
using ObjCRuntime;

namespace Microsoft.Maui.Platform.iOS
{
	internal static class ClassExtensions
	{
		public static Class? GetClass(this Type type)
		{
			var handle = Class.GetHandle(type);
			return handle != IntPtr.Zero ? new Class(handle) : null;
		}
	}
}

