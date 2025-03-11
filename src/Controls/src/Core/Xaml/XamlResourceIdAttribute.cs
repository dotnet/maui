#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Xaml/XamlResourceIdAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.Xaml.XamlResourceIdAttribute']/Docs/*" />
	[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
	public sealed class XamlResourceIdAttribute : Attribute
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Xaml/XamlResourceIdAttribute.xml" path="//Member[@MemberName='ResourceId']/Docs/*" />
		public string ResourceId { get; set; }
		/// <include file="../../../docs/Microsoft.Maui.Controls.Xaml/XamlResourceIdAttribute.xml" path="//Member[@MemberName='Path']/Docs/*" />
		public string Path { get; set; }
		/// <include file="../../../docs/Microsoft.Maui.Controls.Xaml/XamlResourceIdAttribute.xml" path="//Member[@MemberName='Type']/Docs/*" />
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		public Type Type { get; set; }

		/// <include file="../../../docs/Microsoft.Maui.Controls.Xaml/XamlResourceIdAttribute.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public XamlResourceIdAttribute(
			string resourceId,
			string path,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
		{
			ResourceId = resourceId;
			Path = path;
			Type = type;
		}

		private const int ResourceIdIndex = 0;
		private const int PathIndex = 1;
		private const int TypeIndex = 2;

		internal static string GetResourceIdForType(Type type)
		{
			var assembly = type.Assembly;
			try
			{
				foreach (var xria in assembly.GetCustomAttributes<XamlResourceIdAttribute>())
				{
					if (xria.Type == type)
						return xria.ResourceId;
				}
			}
			catch
			{
				foreach (var xria in assembly.GetCustomAttributesData().Where(data => data.AttributeType == typeof(XamlResourceIdAttribute)))
				{
					if (xria.NamedArguments[TypeIndex].TypedValue.Value is Type argType && argType == type)
						return xria.NamedArguments[ResourceIdIndex].TypedValue.Value?.ToString();
				}
			}
			return null;
		}

		internal static string GetPathForType(Type type)
		{
			var assembly = type.Assembly;
			try
			{
				foreach (var xria in assembly.GetCustomAttributes<XamlResourceIdAttribute>())
				{
					if (xria.Type == type)
						return xria.Path;
				}
			}
			catch
			{
				foreach (var xria in assembly.GetCustomAttributesData().Where(data => data.AttributeType == typeof(XamlResourceIdAttribute)))
				{
					if (xria.NamedArguments[TypeIndex].TypedValue.Value is Type argType && argType == type)
						return xria.NamedArguments[PathIndex].TypedValue.Value?.ToString();
				}
			}
			return null;
		}

		internal static string GetResourceIdForPath(Assembly assembly, string path)
		{
			try
			{
				foreach (var xria in assembly.GetCustomAttributes<XamlResourceIdAttribute>())
				{
					if (xria.Path == path)
						return xria.ResourceId;
				}
			}
			catch
			{
				foreach (var xria in assembly.GetCustomAttributesData().Where(data => data.AttributeType == typeof(XamlResourceIdAttribute)))
				{
					if (xria.NamedArguments[PathIndex].TypedValue.Value?.ToString() == path)
						return xria.NamedArguments[ResourceIdIndex].TypedValue.Value?.ToString();
				}
			}
			return null;
		}

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		internal static Type GetTypeForResourceId(Assembly assembly, string resourceId)
		{
			try
			{
				foreach (var xria in assembly.GetCustomAttributes<XamlResourceIdAttribute>())
				{
					if (xria.ResourceId == resourceId)
						return xria.Type;
				}
			}
			catch
			{
				foreach (var xria in assembly.GetCustomAttributesData().Where(data => data.AttributeType == typeof(XamlResourceIdAttribute)))
				{
					if (xria.NamedArguments[ResourceIdIndex].TypedValue.Value?.ToString() == resourceId)
						return xria.NamedArguments[TypeIndex].TypedValue.Value as Type;
				}
			}
			return null;
		}

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		internal static Type GetTypeForPath(Assembly assembly, string path)
		{
			try
			{
				foreach (var xria in assembly.GetCustomAttributes<XamlResourceIdAttribute>())
				{
					if (xria.Path == path)
						return xria.Type;
				}
			}
			catch
			{
				foreach (var xria in assembly.GetCustomAttributesData().Where(data => data.AttributeType == typeof(XamlResourceIdAttribute)))
				{
					if (xria.NamedArguments[PathIndex].TypedValue.Value?.ToString() == path)
						return xria.NamedArguments[TypeIndex].TypedValue.Value as Type;
				}
			}
			return null;
		}
	}
}