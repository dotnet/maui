using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Xamarin.Forms.Internals
{
	public static class ReflectionExtensions
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static FieldInfo GetField(this Type type, Func<FieldInfo, bool> predicate)
		{
			return GetFields(type).FirstOrDefault(predicate);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static FieldInfo GetField(this Type type, string name)
		{
			return type.GetField(fi => fi.Name == name);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IEnumerable<FieldInfo> GetFields(this Type type)
		{
			return GetParts(type, i => i.DeclaredFields);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IEnumerable<PropertyInfo> GetProperties(this Type type)
		{
			return GetParts(type, ti => ti.DeclaredProperties);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static PropertyInfo GetProperty(this Type type, string name)
		{
			Type t = type;
			while (t != null)
			{
				TypeInfo ti = t.GetTypeInfo();
				PropertyInfo property = ti.GetDeclaredProperty(name);
				if (property != null)
					return property;

				t = ti.BaseType;
			}

			return null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool IsAssignableFrom(this Type self, Type c)
		{
			return self.GetTypeInfo().IsAssignableFrom(c.GetTypeInfo());
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool IsInstanceOfType(this Type self, object o)
		{
			return self.GetTypeInfo().IsAssignableFrom(o.GetType().GetTypeInfo());
		}

		static IEnumerable<T> GetParts<T>(Type type, Func<TypeInfo, IEnumerable<T>> selector)
		{
			Type t = type;
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