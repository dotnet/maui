namespace Microsoft.Maui.Controls.Xaml.UnitTests;

/// <summary>
/// Helper for lazy style tests. Forces initialization of a source-gen lazy style
/// without applying the style.
/// </summary>
static class LazyStyleTestHelper
{
	/// <summary>
	/// Forces a lazy style to initialize for <paramref name="target"/>.
	/// Use this when tests need to inspect Setters before the style is naturally applied.
	/// Unlike IStyle.Apply, this does not apply setters to the target — it only populates
	/// the style's Setters collection.
	/// </summary>
	internal static void ForceInitialize(this Style style, BindableObject target)
		=> style.EnsureInitializedFor(target);
}
