#nullable enable
using System;

namespace Microsoft.Maui.Controls
{
	internal interface IWrappedValue
	{
		object? Value { get; }
		Type ValueType { get; }
	}
}
