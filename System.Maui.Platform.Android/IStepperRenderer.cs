using AButton = Android.Widget.Button;

namespace System.Maui.Platform.Android
{
	public interface IStepperRenderer
	{
		Stepper Element { get; }

		AButton UpButton { get; }

		AButton DownButton { get; }

		AButton CreateButton();
	}
}
