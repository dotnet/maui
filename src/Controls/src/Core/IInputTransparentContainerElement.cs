#nullable disable

namespace Microsoft.Maui.Controls
{
	// There are 2 Layout types: Controls and Compatibility
	interface IInputTransparentContainerElement
	{
		bool InputTransparent { get; }

		bool CascadeInputTransparent { get; }

		Element Parent { get; }
	}
}