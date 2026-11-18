namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a checkbox that supports programmatic indeterminate state.
	/// </summary>
	public interface ICheckBox2 : ICheckBox
	{
		/// <summary>
		/// Gets or sets the state: true for checked, false for unchecked, or null for indeterminate
		/// when <see cref="IsThreeState"/> is true. Otherwise, null is normalized to false.
		/// </summary>
		new bool? IsChecked { get; set; }

		/// <summary>
		/// Gets whether application code can assign indeterminate. User interaction remains binary.
		/// </summary>
		bool IsThreeState { get; }
	}
}
