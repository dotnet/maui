namespace Microsoft.Maui
{
	/// <summary>
	/// Enumerates values that influence clear button visibility behavior on input fields.
	/// Typically this is a button inside of the input field, near the end, which clears the input when pressed.
	/// </summary>
	public enum ClearButtonVisibility
	{
		/// <summary>
		/// Never show a clear button.
		/// </summary>
		Never,

		/// <summary>
		/// Only show a clear button in the input field while the input field has focus and is being edited.
		/// </summary>
		WhileEditing
	}
}