using System;
using System.Reflection;

namespace Xamarin.Forms.Xaml
{
	[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
	public sealed class XamlResourceIdAttribute : Attribute
	{
		public string ResourceId { get; set; }
		public string Path { get; set; }
		public Type Type { get; set; }

		public XamlResourceIdAttribute(string resourceId, string path, Type type)
		{
			ResourceId = resourceId;
			Path = path;
			Type = type;
		}

		internal static string GetResourceIdForType(Type type)
		{
			var assembly = type.GetTypeInfo().Assembly;
			foreach (var xria in assembly.GetCustomAttributes<XamlResourceIdAttribute>())
			{
				if (xria.Type == type)
					return xria.ResourceId;
			}
			return null;
		}

		internal static string GetPathForType(Type type)
		{
			var assembly = type.GetTypeInfo().Assembly;
			foreach (var xria in assembly.GetCustomAttributes<XamlResourceIdAttribute>())
			{
				if (xria.Type == type)
					return xria.Path;
			}
			return null;
		}

		internal static string GetResourceIdForPath(Assembly assembly, string path)
		{
			foreach (var xria in assembly.GetCustomAttributes<XamlResourceIdAttribute>())
			{
				if (xria.Path == path)
					return xria.ResourceId;
			}
			return null;
		}

		internal static Type GetTypeForResourceId(Assembly assembly, string resourceId)
		{
			foreach (var xria in assembly.GetCustomAttributes<XamlResourceIdAttribute>())
			{
				if (xria.ResourceId == resourceId)
					return xria.Type;
			}
			return null;
		}

		internal static Type GetTypeForPath(Assembly assembly, string path)
		{
			foreach (var xria in assembly.GetCustomAttributes<XamlResourceIdAttribute>())
			{
				if (xria.Path == path)
					return xria.Type;
			}
			return null;
		}
	}
}