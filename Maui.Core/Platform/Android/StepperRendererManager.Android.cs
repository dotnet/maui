using System.ComponentModel;
using Android.Views;
using AButton = Android.Widget.Button;
using AView = Android.Views.View;

namespace System.Maui.Platform
{
	public interface IStepperRenderer
	{
		AButton UpButton { get; }

		AButton DownButton { get; }

		AButton CreateButton();

		IStepper Element { get; }
	}

	class StepperRendererHolder : Java.Lang.Object
	{
		internal IStepperRenderer _renderer;
		public StepperRendererHolder(IStepperRenderer renderer)
		{
			_renderer = renderer;
		}
	}

	public static class StepperRendererManager
	{
		public static void CreateStepperButtons<TButton>(IStepperRenderer renderer, out TButton downButton, out TButton upButton)
			where TButton : AButton
		{
			downButton = (TButton)renderer.CreateButton();
			//downButton.Id = Platform.GenerateViewId();
			downButton.Focusable = true;
			upButton = (TButton)renderer.CreateButton();
			//upButton.Id = Platform.GenerateViewId();
			upButton.Focusable = true;

			downButton.Gravity = GravityFlags.Center;
			downButton.Tag = new StepperRendererHolder(renderer);
			downButton.SetOnClickListener(StepperListener.Instance);
			upButton.Gravity = GravityFlags.Center;
			upButton.Tag = new StepperRendererHolder(renderer);
			upButton.SetOnClickListener(StepperListener.Instance);

			// IMPORTANT:
			// Do not be decieved. These are NOT the same characters. Neither are a "minus" either.
			// The Text is a visually pleasing "minus", and the description is the phonetically correct "minus".
			// The little key on your keyboard is a dash/hyphen.
			downButton.Text = "－";
			downButton.ContentDescription = "−";

			// IMPORTANT:
			// Do not be decieved. These are NOT the same characters.
			// The Text is a visually pleasing "plus", and the description is the phonetically correct "plus"
			// (which, unlike the minus, IS found on your keyboard).
			upButton.Text = "＋";
			upButton.ContentDescription = "+";

			downButton.NextFocusForwardId = upButton.Id;
		}

		public static void UpdateButtons<TButton>(IStepperRenderer renderer, TButton downButton, TButton upButton, PropertyChangedEventArgs e = null)
			where TButton : AButton
		{
			if (!(renderer?.Element is IStepper stepper))
				return;
			// NOTE: a value of `null` means that we are forcing an update
			downButton.Enabled = stepper.IsEnabled && stepper.Value > stepper.Minimum;
			upButton.Enabled = stepper.IsEnabled && stepper.Value < stepper.Maximum;

		}

		class StepperListener : Java.Lang.Object, AView.IOnClickListener
		{
			public static readonly StepperListener Instance = new StepperListener();

			public void OnClick(AView v)
			{
				if (!(v?.Tag is StepperRendererHolder rendererHolder))
					return;

				if (!(rendererHolder._renderer?.Element is IStepper stepper))
					return;

				var increment = stepper.Increment;
				if (v == rendererHolder._renderer.DownButton)
					increment = -increment;

				rendererHolder._renderer.Element.Value = stepper.Value + increment;
				UpdateButtons(rendererHolder._renderer, rendererHolder._renderer.DownButton, rendererHolder._renderer.UpButton);
			}
		}
	}
}
