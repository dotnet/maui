namespace Microsoft.Maui.Controls.Xaml.UnitTests;

/// <summary>
/// Helper for lazy style tests. Forces initialization of a source-gen lazy style
/// by applying it to a target element.
/// </summary>
static class LazyStyleTestHelper
{
	/// <summary>
	/// Forces a lazy style to initialize by applying it to the given target.
	/// Use this when tests need to inspect Setters before the style is naturally applied.
	/// </summary>
	internal static void ForceInitialize(this Style style, BindableObject target)
		=> ((IStyle)style).Apply(target, new SetterSpecificity());
}
