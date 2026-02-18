using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Microsoft.Maui.Controls;

internal static class RemappingDebugHelper
{
	/// <summary>
	/// Debug-only check that ensures no intermediate type between <paramref name="derivedType"/>
	/// and <paramref name="baseWithField"/> declares its own <c>s_forceStaticConstructor</c> field
	/// that we'd be accidentally skipping.
	/// </summary>
	[Conditional("DEBUG")]
	public static void AssertBaseClassForRemapping(
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicFields)] Type derivedType,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicFields)] Type baseWithField)
	{
		var type = derivedType.BaseType;
		while (type is not null && type != baseWithField)
		{
			var field = type.GetField("s_forceStaticConstructor", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
			Debug.Assert(field is null,
				$"Type {type} declares s_forceStaticConstructor but {derivedType} bypasses it by referencing {baseWithField}.s_forceStaticConstructor directly. " +
				$"Update {derivedType}'s static constructor to reference {type}.s_forceStaticConstructor instead.");
			type = type.BaseType;
		}

		Debug.Assert(type == baseWithField,
			$"{baseWithField} is not a base class of {derivedType}.");
	}
}
