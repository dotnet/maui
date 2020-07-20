using CoreGraphics;
using MaterialComponents;
using MTextInputControllerBase = MaterialComponents.TextInputControllerBase;

namespace Xamarin.Forms.Material.iOS
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