using System.Collections.Generic;

namespace Microsoft.Maui.Controls.SourceGen.Css;

/// <summary>
/// Represents a parsed CSS rule (selector + declarations) for source generation.
/// </summary>
readonly record struct CssRule(string Selector, IReadOnlyList<KeyValuePair<string, string>> Declarations, HashSet<string>? ImportantProperties = null);

/// <summary>
/// Represents a parsed @media group (condition + rules) for source generation.
/// </summary>
readonly record struct CssMediaGroup(string Condition, IReadOnlyList<CssRule> Rules);
