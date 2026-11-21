namespace Microsoft.Maui
{
	/// <summary>
	/// Provides an optional clear-selection affordance for a picker.
	/// </summary>
	public interface IPickerClearButton
	{
		/// <summary>
		/// Gets whether a clear button is shown when the picker has a selection.
		/// </summary>
		bool IsClearButtonVisible { get; }
	}
}
