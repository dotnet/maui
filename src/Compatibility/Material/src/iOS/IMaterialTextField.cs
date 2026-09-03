using CoreGraphics;
using MaterialComponents;
using MTextInputControllerBase = MaterialComponents.TextInputControllerBase;

namespace Microsoft.Maui.Controls.Compatibility.Material.iOS
{
	internal interface IMaterialTextField
	{
		SemanticColorScheme ColorScheme { get; set; }
		ContainerScheme ContainerScheme { get; }
		TypographyScheme TypographyScheme { get; set; }
		MTextInputControllerBase ActiveTextInputController { get; set; }
		ITextInput TextInput { get; }
		CGSize? BackgroundSize { get; set; }
	}
}