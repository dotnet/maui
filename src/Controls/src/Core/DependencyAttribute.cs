#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	/// <summary>An attribute that indicates that the specified type provides a concrete implementation of a needed interface.</summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class DependencyAttribute : Attribute
	{
		/// <summary>Creates a new <see cref="Microsoft.Maui.Controls.DependencyAttribute"/> with default values.</summary>
		/// <param name="implementorType">The type of the implementor of the dependency.</param>
		public DependencyAttribute(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type implementorType)
		{
			Implementor = implementorType;
		}

		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		internal Type Implementor { get; private set; }
	}
}