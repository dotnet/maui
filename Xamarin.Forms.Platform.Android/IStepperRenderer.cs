using AButton = Android.Widget.Button;

namespace Xamarin.Forms.Platform.Android
{
	public interface IStepperRenderer
	{
		Stepper Element { get; }

		AButton UpButton { get; }

		AButton DownButton { get; }

		AButton CreateButton();
	}
}