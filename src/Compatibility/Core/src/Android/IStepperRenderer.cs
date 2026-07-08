using AButton = Android.Widget.Button;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IStepperRenderer
	{
		Stepper Element { get; }

		AButton UpButton { get; }

		AButton DownButton { get; }

		AButton CreateButton();
	}
}
