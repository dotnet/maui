using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui
{
	class TrimmerHelper
	{
		public const string ConcurrentDictionary = "We cannot specify DynamicallyAccessedMemberTypes on ConcurrentDictionary.GetOrAdd().";
		public const string MakeGenericType = "We will need to suppress this warning until we can remove usage of Type.MakeGenericType().";

		/// <summary>
		/// Calls Type.GetType(), but with DynamicallyAccessedMemberTypes.Interfaces|PublicParameterlessConstructor.
		/// This allows illink to statically analyze System.Reflection usage.
		/// </summary>
		[UnconditionalSuppressMessage("Trimming", "IL2073", Justification = "We cannot specify DynamicallyAccessedMemberTypes on object.GetType().")]
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		public static Type GetType(object obj) => obj.GetType();
	}
}
