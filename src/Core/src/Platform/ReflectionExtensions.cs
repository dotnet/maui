#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Maui.Platform
{
	internal static class ReflectionExtensions
	{
		public static FieldInfo? GetField(this Type type, Func<FieldInfo, bool> predicate)
		{
			return GetFields(type).FirstOrDefault(predicate);
		}

		public static IEnumerable<FieldInfo> GetFields(this Type type)
		{
			return GetParts(type, i => i.DeclaredFields);
		}

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

		static IEnumerable<T> GetParts<T>(Type type, Func<TypeInfo, IEnumerable<T>> selector)
		{
			Type? t = type;
			while (t != null)
			{
				TypeInfo ti = t.GetTypeInfo();
				foreach (T f in selector(ti))
					yield return f;
				t = ti.BaseType;
			}
		}
	}
}