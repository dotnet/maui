using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls;

internal static class RemappingHelper
{
	/// <summary>
	/// Ensures the static constructor of <paramref name="baseType"/> has run before the
	/// caller's static constructor continues, so that base-level mapper remappings are
	/// applied first. In DEBUG builds, also validates that no intermediate type in the
	/// hierarchy is accidentally skipped.
	/// </summary>
	[UnconditionalSuppressMessage("Trimming", "IL2059",
		Justification = "We intentionally trigger the static constructor of the base type to ensure mapper remappings are applied in order.")]
	public static void EnsureBaseTypeRemapped(Type derivedType, Type baseType)
	{
		Debug.Assert(derivedType.IsAssignableTo(baseType),
			$"{baseType} is not a base class of {derivedType}.");

		RuntimeHelpers.RunClassConstructor(baseType.TypeHandle);
	}
}
