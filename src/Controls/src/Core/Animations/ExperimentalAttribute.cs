#if !NET8_0_OR_GREATER
#nullable enable
namespace System.Diagnostics.CodeAnalysis
{
	// Polyfill for the netstandard targets; the compiler recognizes the attribute by name.
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
	internal sealed class ExperimentalAttribute : Attribute
	{
		public ExperimentalAttribute(string diagnosticId) => DiagnosticId = diagnosticId;

		public string DiagnosticId { get; }

		public string? UrlFormat { get; set; }
	}
}
#endif
