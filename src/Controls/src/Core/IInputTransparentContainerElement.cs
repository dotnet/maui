#nullable disable

namespace Microsoft.Maui.Controls
{
	// There are 2 Layout types: Controls and Compatibility
	interface IInputTransparentContainerElement
	{
		bool InputTransparent { get; }

		/// <summary>
		/// Gets or sets a value that controls whether child elements
		/// inherit the input transparency of this layout when the tranparency is <see langword="true"/>.
		/// </summary>
		/// <value>
		/// <see langword="true" /> to cause child elements to inherit the input transparency of this layout,
		/// when this layout's <see cref="VisualElement.InputTransparent" /> property is <see langword="true" />.
		/// <see langword="false" /> to cause child elements to ignore the input tranparency of this layout.
		/// </value>
		bool CascadeInputTransparent { get; }

		Element Parent { get; }
	}
}