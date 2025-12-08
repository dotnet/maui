#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls;

[EditorBrowsable(EditorBrowsableState.Never)]
public interface IPaddingElement
{
	/// <summary>
	/// Gets or sets the inner padding of the layout.
	/// The default value is a <see cref="Thickness"/> with all values set to 0.
	/// </summary>
	/// <remarks>The padding is the space between the bounds of a layout and the bounding region into which its children should be arranged into.</remarks>
	//note to implementor: implement this property publicly
	Thickness Padding { get; }

	//note to implementor: but implement this method explicitly
	void OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue);
	Thickness PaddingDefaultValueCreator();
}