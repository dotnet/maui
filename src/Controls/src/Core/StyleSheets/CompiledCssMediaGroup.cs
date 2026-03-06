#nullable disable
using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.StyleSheets
{
	/// <summary>
	/// Represents a @media conditional group of CSS rules for use by the source generator.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public readonly struct CompiledCssMediaGroup : IEquatable<CompiledCssMediaGroup>
	{
		/// <summary>The media condition text, e.g. "(min-width: 768px)".</summary>
		public readonly string Condition;

		/// <summary>The CSS rules that apply when the condition is satisfied.</summary>
		public readonly CompiledCssRule[] Rules;

		/// <summary>Creates a new compiled media group.</summary>
		public CompiledCssMediaGroup(string condition, CompiledCssRule[] rules)
		{
			Condition = condition;
			Rules = rules;
		}

		/// <inheritdoc/>
		public bool Equals(CompiledCssMediaGroup other) => string.Equals(Condition, other.Condition, StringComparison.Ordinal) && ReferenceEquals(Rules, other.Rules);

		/// <inheritdoc/>
		public override bool Equals(object obj) => obj is CompiledCssMediaGroup other && Equals(other);

		/// <inheritdoc/>
		public override int GetHashCode()
		{
#if NETSTANDARD2_0
			return Condition?.GetHashCode() ?? 0;
#else
			return Condition?.GetHashCode(StringComparison.Ordinal) ?? 0;
#endif
		}

		/// <summary>Equality operator.</summary>
		public static bool operator ==(CompiledCssMediaGroup left, CompiledCssMediaGroup right) => left.Equals(right);

		/// <summary>Inequality operator.</summary>
		public static bool operator !=(CompiledCssMediaGroup left, CompiledCssMediaGroup right) => !left.Equals(right);
	}
}
