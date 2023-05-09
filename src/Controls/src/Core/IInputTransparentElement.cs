#nullable disable

namespace Microsoft.Maui.Controls
{
	// There are 2 Layout types: Controls and Compatibility
	interface IInputTransparentElement
	{
		bool InputTransparent2 { get; }

		bool CascadeInputTransparent2 { get; }

		Element Parent2 { get; }
	}
}