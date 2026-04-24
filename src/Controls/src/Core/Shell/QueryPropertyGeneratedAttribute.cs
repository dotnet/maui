using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Marker attribute applied by the QueryProperty source generator to indicate
	/// that a class has a generated IQueryAttributable implementation.
	/// This is used by ShellContent to skip the reflection-based fallback.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class QueryPropertyGeneratedAttribute : Attribute
	{
	}
}
