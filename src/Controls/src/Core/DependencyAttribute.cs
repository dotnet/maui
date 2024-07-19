#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/DependencyAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.DependencyAttribute']/Docs/*" />
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	[Obsolete("Use the service collection in the MauiAppBuilder instead. This will be removed in a future release.")]
	public sealed class DependencyAttribute : Attribute
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/DependencyAttribute.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public DependencyAttribute(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type implementorType)
		{
			Implementor = implementorType;
		}

		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		internal Type Implementor { get; private set; }
	}
}