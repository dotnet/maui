namespace Microsoft.Maui
{
	/// <summary>
	/// Enumerates values that influences clear button visibility behavior on input fields.
	/// Typically this is shown as a button inside of the input field, near the end, with which the input can be cleared with a tap on said button.
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