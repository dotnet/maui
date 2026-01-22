namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Specifies the mode for resolving a relative binding source.
	/// </summary>
	public enum RelativeBindingSourceMode
	{
		// 0 reserved for possible future implementation of PreviousData 

		/// <summary>
		/// Binds to the element that applied the control template containing this binding.
		/// </summary>
		TemplatedParent = 1,

		/// <summary>
		/// Binds to the element on which the binding is set.
		/// </summary>
		Self = 2,

		/// <summary>
		/// Binds to an ancestor element of the specified type in the visual tree.
		/// </summary>
		FindAncestor = 3,

		/// <summary>
		/// Binds to the binding context of an ancestor element of the specified type.
		/// </summary>
		FindAncestorBindingContext = 4,
	}
}
