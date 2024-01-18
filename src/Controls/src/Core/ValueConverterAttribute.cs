using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
#pragma warning disable RS0016 // Symbol 'ValueConverterAttribute' is not part of the declared public API 
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
	public sealed class ValueConverterAttribute : Attribute
	{
		public ValueConverterAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type converterType)
		{
			Debug.Assert(typeof(IValueConverter).IsAssignableFrom(converterType));

			ConverterType = converterType;
		}

		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		public Type ConverterType { get; }
	}
#pragma warning restore RS0016
}
