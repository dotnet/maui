#nullable disable

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Interface for elements that support input transparency for their container.
	/// There are 2 Layout types: Controls and Compatibility
	/// </summary>
	interface IInputTransparentContainerElement
	{
		bool InputTransparent { get; }

		bool CascadeInputTransparent { get; }

		Element Parent { get; }
	}
}