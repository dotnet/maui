#nullable enable
using System;
using System.IO;
using System.Reflection;

namespace Microsoft.Maui.Platform
{
	internal static class ReflectionExtensions
	{
		internal static object[]? GetCustomAttributesSafe(this Assembly assembly, Type attrType)
		{
			try
			{
				return assembly.GetCustomAttributes(attrType, true);
			}
			catch (FileNotFoundException)
			{
				// Sometimes the previewer doesn't actually have everything required for these loads to work
				// TODO: Register the exception in the Log when we have the Logger ported
			}

			return null;
		}

		public static bool IsInstanceOfType(this Type self, object o)
		{
			return self.IsAssignableFrom(o.GetType());
		}
	}
}