#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Maui.Platform
{
	internal static class ReflectionExtensions
	{
		const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

		public static FieldInfo? GetField([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)] this Type type, Func<FieldInfo, bool> predicate) =>
			GetFields(type).FirstOrDefault(predicate);

		public static FieldInfo? GetField([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)] this Type type, string name) =>
			type.GetField(name, flags);

		public static IEnumerable<FieldInfo> GetFields([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)] this Type type) =>
			type.GetFields(flags);

		public static IEnumerable<PropertyInfo> GetProperties([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] this Type type) =>
			type.GetProperties(flags);

		public static PropertyInfo? GetProperty([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] this Type type, string name) =>
			type.GetProperty(name, flags);

		internal static object[]? GetCustomAttributesSafe(this Assembly assembly, Type attrType)
		{
			try
			{
#if !NETSTANDARD1_0
				return assembly.GetCustomAttributes(attrType, true);
#else
				return assembly.GetCustomAttributes(attrType).ToArray();
#endif
			}
			catch (FileNotFoundException)
			{
				// Sometimes the previewer doesn't actually have everything required for these loads to work
				// TODO: Register the exception in the Log when we have the Logger ported
			}

			return null;
		}

		public static bool IsAssignableFrom(this Type self, Type c)
		{
			return self.GetTypeInfo().IsAssignableFrom(c.GetTypeInfo());
		}

		public static bool IsInstanceOfType(this Type self, object o)
		{
			return self.GetTypeInfo().IsAssignableFrom(o.GetType().GetTypeInfo());
		}
	}
}