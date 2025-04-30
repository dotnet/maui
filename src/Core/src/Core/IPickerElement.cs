namespace Microsoft.Maui;

/// <summary>
/// Defines a UI element that can display content in a dropdown or popup that can be opened and closed.
/// </summary>
/// <remarks>
/// The <see cref="IPickerElement"/> interface is implemented by controls that have an expandable region
/// which can be shown or hidden, such as Picker, DatePicker, or TimePicker.
/// This interface allows for programmatic control of the dropdown state.
/// </remarks>
public interface IPickerElement
{
	/// <summary>
	/// Gets or sets a value indicating whether the dropdown is currently open.
	/// </summary>
	/// <value>
	/// <c>true</c> if the dropdown is currently open and visible; otherwise, <c>false</c>.
	/// </value>
	/// <remarks>
	/// Setting this property programmatically will open or close the dropdown.
	/// Controls may also update this property when the dropdown is opened or closed through user interaction.
	/// </remarks>
	bool IsOpen { get; set; }

	/// <summary>
	/// Called when the <see cref="IsOpen"/> property changes.
	/// </summary>
	/// <param name="oldValue">The previous value of <see cref="IsOpen"/>.</param>
	/// <param name="newValue">The new value of <see cref="IsOpen"/>.</param>
	/// <remarks>
	/// This method is intended for handling UI updates or additional logic 
	/// when the dropdown's visibility state changes.
	/// Implementers may trigger animations, adjust layout, or notify other components.
	/// </remarks>
	void OnIsOpenPropertyChanged(bool oldValue, bool newValue);
}