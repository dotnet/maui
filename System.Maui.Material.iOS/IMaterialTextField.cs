using MaterialComponents;
using MTextInputControllerBase = MaterialComponents.TextInputControllerBase;

namespace System.Maui.Material.iOS
{
	internal interface IMaterialTextField
	{
		SemanticColorScheme ColorScheme { get; set; }
		ContainerScheme ContainerScheme { get; }
		TypographyScheme TypographyScheme { get; set; }
		MTextInputControllerBase ActiveTextInputController { get; set; }
		ITextInput TextInput { get; }
	}
}