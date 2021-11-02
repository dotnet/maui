// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#if NETSTANDARD2_0 || NETSTANDARD2_1
namespace System.Diagnostics.CodeAnalysis
{
	/// <summary>
	/// States a dependency that one member has on another.
	/// </summary>
	/// <remarks>
	/// This can be used to inform tooling of a dependency that is otherwise not evident purely from
	/// metadata and IL, for example a member relied on via reflection.
	/// </remarks>
	[AttributeUsage(
		AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Method,
		AllowMultiple = true, Inherited = false)]
	internal sealed class DynamicDependencyAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicDependencyAttribute"/> class
		/// with the specified signature of a member on the same type as the consumer.
		/// </summary>
		/// <param name="memberSignature">The signature of the member depended on.</param>
		public DynamicDependencyAttribute(string memberSignature)
		{
			MemberSignature = memberSignature;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicDependencyAttribute"/> class
		/// with the specified signature of a member on a <see cref="System.Type"/>.
		/// </summary>
		/// <param name="memberSignature">The signature of the member depended on.</param>
		/// <param name="type">The <see cref="System.Type"/> containing <paramref name="memberSignature"/>.</param>
		public DynamicDependencyAttribute(string memberSignature, Type type)
		{
			MemberSignature = memberSignature;
			Type = type;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicDependencyAttribute"/> class
		/// with the specified signature of a member on a type in an assembly.
		/// </summary>
		/// <param name="memberSignature">The signature of the member depended on.</param>
		/// <param name="typeName">The full name of the type containing the specified member.</param>
		/// <param name="assemblyName">The assembly name of the type containing the specified member.</param>
		public DynamicDependencyAttribute(string memberSignature, string typeName, string assemblyName)
		{
			MemberSignature = memberSignature;
			TypeName = typeName;
			AssemblyName = assemblyName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicDependencyAttribute"/> class
		/// with the specified types of members on a <see cref="System.Type"/>.
		/// </summary>
		/// <param name="memberTypes">The types of members depended on.</param>
		/// <param name="type">The <see cref="System.Type"/> containing the specified members.</param>
		public DynamicDependencyAttribute(DynamicallyAccessedMemberTypes memberTypes, Type type)
		{
			MemberTypes = memberTypes;
			Type = type;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicDependencyAttribute"/> class
		/// with the specified types of members on a type in an assembly.
		/// </summary>
		/// <param name="memberTypes">The types of members depended on.</param>
		/// <param name="typeName">The full name of the type containing the specified members.</param>
		/// <param name="assemblyName">The assembly name of the type containing the specified members.</param>
		public DynamicDependencyAttribute(DynamicallyAccessedMemberTypes memberTypes, string typeName, string assemblyName)
		{
			MemberTypes = memberTypes;
			TypeName = typeName;
			AssemblyName = assemblyName;
		}

		/// <summary>
		/// Gets the signature of the member depended on.
		/// </summary>
		/// <remarks>
		/// Either <see cref="MemberSignature"/> must be a valid string or <see cref="MemberTypes"/>
		/// must not equal <see cref="DynamicallyAccessedMemberTypes.None"/>, but not both.
		/// </remarks>
		public string? MemberSignature { get; }

		/// <summary>
		/// Gets the <see cref="DynamicallyAccessedMemberTypes"/> which specifies the type
		/// of members depended on.
		/// </summary>
		/// <remarks>
		/// Either <see cref="MemberSignature"/> must be a valid string or <see cref="MemberTypes"/>
		/// must not equal <see cref="DynamicallyAccessedMemberTypes.None"/>, but not both.
		/// </remarks>
		public DynamicallyAccessedMemberTypes MemberTypes { get; }

		/// <summary>
		/// Gets the <see cref="System.Type"/> containing the specified member.
		/// </summary>
		/// <remarks>
		/// If neither <see cref="Type"/> nor <see cref="TypeName"/> are specified,
		/// the type of the consumer is assumed.
		/// </remarks>
		public Type? Type { get; }

		/// <summary>
		/// Gets the full name of the type containing the specified member.
		/// </summary>
		/// <remarks>
		/// If neither <see cref="Type"/> nor <see cref="TypeName"/> are specified,
		/// the type of the consumer is assumed.
		/// </remarks>
		public string? TypeName { get; }

		/// <summary>
		/// Gets the assembly name of the specified type.
		/// </summary>
		/// <remarks>
		/// <see cref="AssemblyName"/> is only valid when <see cref="TypeName"/> is specified.
		/// </remarks>
		public string? AssemblyName { get; }

		/// <summary>
		/// Gets or sets the condition in which the dependency is applicable, e.g. "DEBUG".
		/// </summary>
		public string? Condition { get; set; }
	}
}
#endif
