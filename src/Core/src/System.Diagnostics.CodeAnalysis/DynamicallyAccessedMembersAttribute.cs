// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#if NETSTANDARD2_0 || NETSTANDARD2_1
namespace System.Diagnostics.CodeAnalysis
{
	/// <summary>
	/// Indicates that certain members on a specified <see cref="Type"/> are accessed dynamically,
	/// for example through <see cref="System.Reflection"/>.
	/// </summary>
	/// <remarks>
	/// This allows tools to understand which members are being accessed during the execution
	/// of a program.
	///
	/// This attribute is valid on members whose type is <see cref="Type"/> or <see cref="string"/>.
	///
	/// When this attribute is applied to a location of type <see cref="string"/>, the assumption is
	/// that the string represents a fully qualified type name.
	///
	/// When this attribute is applied to a class, interface, or struct, the members specified
	/// can be accessed dynamically on <see cref="Type"/> instances returned from calling
	/// <see cref="object.GetType"/> on instances of that class, interface, or struct.
	///
	/// If the attribute is applied to a method it's treated as a special case and it implies
	/// the attribute should be applied to the "this" parameter of the method. As such the attribute
	/// should only be used on instance methods of types assignable to System.Type (or string, but no methods
	/// will use it there).
	/// </remarks>
	[AttributeUsage(
		AttributeTargets.Field | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter |
		AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Method |
		AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct,
		Inherited = false)]
#if SYSTEM_PRIVATE_CORELIB
    public
#else
	internal
#endif
	sealed class DynamicallyAccessedMembersAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicallyAccessedMembersAttribute"/> class
		/// with the specified member types.
		/// </summary>
		/// <param name="memberTypes">The types of members dynamically accessed.</param>
		public DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes memberTypes)
		{
			MemberTypes = memberTypes;
		}

		/// <summary>
		/// Gets the <see cref="DynamicallyAccessedMemberTypes"/> which specifies the type
		/// of members dynamically accessed.
		/// </summary>
		public DynamicallyAccessedMemberTypes MemberTypes { get; }
	}
}
#endif
