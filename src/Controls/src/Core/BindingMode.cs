namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Specifies the direction of data flow in a binding.
	/// </summary>
	public enum BindingMode
	{
		/// <summary>
		/// Uses the default binding mode for the target property.
		/// </summary>
		Default,
		/// <summary>
		/// Updates both the target and source when either changes.
		/// </summary>
		TwoWay,
		/// <summary>
		/// Updates the target when the source changes.
		/// </summary>
		OneWay,
		/// <summary>
		/// Updates the source when the target changes.
		/// </summary>
		OneWayToSource,
		/// <summary>
		/// Updates the target only once when the binding is created.
		/// </summary>
		OneTime,
	}
}