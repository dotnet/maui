#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.StyleSheets
{
	/// <summary>
	/// Represents a pre-parsed CSS rule for use by the source generator.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public readonly struct CompiledCssRule : IEquatable<CompiledCssRule>
	{
		/// <summary>The CSS selector text (e.g. ".primary", "label > .title").</summary>
		public readonly string Selector;

		/// <summary>The CSS declarations as key-value pairs (already shorthand-expanded and unit-resolved).</summary>
		public readonly KeyValuePair<string, string>[] Declarations;

		/// <summary>Property names that were declared with !important.</summary>
		public readonly string[] ImportantProperties;

		/// <summary>Creates a new compiled CSS rule.</summary>
		public CompiledCssRule(string selector, KeyValuePair<string, string>[] declarations, string[] importantProperties = null)
		{
			Selector = selector;
			Declarations = declarations;
			ImportantProperties = importantProperties;
		}

		/// <inheritdoc/>
		public bool Equals(CompiledCssRule other) => string.Equals(Selector, other.Selector, StringComparison.Ordinal) && ReferenceEquals(Declarations, other.Declarations);

		/// <inheritdoc/>
		public override bool Equals(object obj) => obj is CompiledCssRule other && Equals(other);

		/// <inheritdoc/>
		public override int GetHashCode()
		{
#if NETSTANDARD2_0
			return Selector?.GetHashCode() ?? 0;
#else
			return Selector?.GetHashCode(StringComparison.Ordinal) ?? 0;
#endif
		}

		/// <summary>Equality operator.</summary>
		public static bool operator ==(CompiledCssRule left, CompiledCssRule right) => left.Equals(right);

		/// <summary>Inequality operator.</summary>
		public static bool operator !=(CompiledCssRule left, CompiledCssRule right) => !left.Equals(right);
	}
}
