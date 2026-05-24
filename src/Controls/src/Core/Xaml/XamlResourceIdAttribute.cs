#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>Maps a XAML resource ID to its path and associated type.</summary>
	[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
	public sealed class XamlResourceIdAttribute : Attribute
	{
		/// <summary>Gets or sets the embedded resource identifier.</summary>
		public string ResourceId { get; set; }

		/// <summary>Gets or sets the project path to the XAML file.</summary>
		public string Path { get; set; }

		/// <summary>Gets or sets the type associated with the XAML resource.</summary>
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		public Type Type { get; set; }

		/// <summary>Creates a new <see cref="XamlResourceIdAttribute"/>.</summary>
		/// <param name="resourceId">The embedded resource identifier.</param>
		/// <param name="path">The project path to the XAML file.</param>
		/// <param name="type">The type associated with the XAML resource.</param>
		public XamlResourceIdAttribute(
			string resourceId,
			string path,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
		{
			ResourceId = resourceId;
			Path = path;
			Type = type;
		}

		internal static string GetResourceIdForType(Type type)
		{
			var assembly = type.Assembly;
			foreach (var xria in assembly.GetCustomAttributes<XamlResourceIdAttribute>())
			{
				if (xria.Type == type)
					return xria.ResourceId;
			}
			return null;
		}

		internal static string GetPathForType(Type type)
		{
			var assembly = type.Assembly;
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

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		internal static Type GetTypeForResourceId(Assembly assembly, string resourceId)
		{
			foreach (var xria in assembly.GetCustomAttributes<XamlResourceIdAttribute>())
			{
				if (xria.ResourceId == resourceId)
					return xria.Type;
			}
			return null;
		}

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
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